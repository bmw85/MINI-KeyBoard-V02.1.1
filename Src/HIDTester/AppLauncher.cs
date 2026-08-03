using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace HIDTester;

/// <summary>
/// Listens for Ctrl+Alt+Shift+F1..F12 (USB or Bluetooth) and launches mapped executables.
/// Uses a low-level keyboard hook — more reliable than RegisterHotKey for BT keyboards.
/// Mappings: %AppData%\MINI KeyBoard\app_launches.json
/// </summary>
public sealed class AppLauncher : IDisposable
{
  private const int WhKeyboardLl = 13;
  private const int WmKeyDown = 0x0100;
  private const int WmSysKeyDown = 0x0104;
  private const int WmKeyUp = 0x0101;
  private const int WmSysKeyUp = 0x0105;

  private const int VkShift = 0x10;
  private const int VkControl = 0x11;
  private const int VkMenu = 0x12; // Alt
  private const int VkF1 = 0x70;

  // HID usage IDs F1=58 .. F12=69
  public static byte HidCodeForFKey(int fIndex1Based)
  {
    if (fIndex1Based < 1 || fIndex1Based > 12) return 0;
    return (byte)(57 + fIndex1Based);
  }

  public static byte FunModifierCtrlAltShift => (byte)(1 | 2 | 4); // Ctrl|Shift|Alt

  private readonly Dictionary<int, string> _mappings = new Dictionary<int, string>();
  private LowLevelKeyboardProc _proc;
  private IntPtr _hookId = IntPtr.Zero;
  private bool _listening;
  private bool _disposed;
  private int _lastLaunchF;
  private long _lastLaunchTick;

  public AppLauncher(IntPtr hwnd)
  {
    // hwnd kept for API compatibility; hook is process-wide
  }

  public static string ConfigDirectory
  {
    get
    {
      return Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MINI KeyBoard");
    }
  }

  public static string ConfigPath => Path.Combine(ConfigDirectory, "app_launches.json");

  public IReadOnlyDictionary<int, string> Mappings => _mappings;

  public void Start()
  {
    if (_listening) return;
    Load();
    _proc = HookCallback;
    // For WH_KEYBOARD_LL, module handle of this process is required on older Windows;
    // Zero also works on Win10+ for global LL hooks.
    IntPtr hMod = IntPtr.Zero;
    try
    {
      using (Process cur = Process.GetCurrentProcess())
      using (ProcessModule mod = cur.MainModule)
        hMod = GetModuleHandle(mod != null ? mod.ModuleName : null);
    }
    catch { /* ignore */ }

    _hookId = SetWindowsHookEx(WhKeyboardLl, _proc, hMod, 0);
    if (_hookId == IntPtr.Zero)
      _hookId = SetWindowsHookEx(WhKeyboardLl, _proc, IntPtr.Zero, 0);
    _listening = _hookId != IntPtr.Zero;
  }

  public void Stop()
  {
    if (!_listening) return;
    if (_hookId != IntPtr.Zero)
    {
      UnhookWindowsHookEx(_hookId);
      _hookId = IntPtr.Zero;
    }
    _proc = null;
    _listening = false;
  }

  public void SetMapping(int fKey1Based, string exePath)
  {
    if (fKey1Based < 1 || fKey1Based > 12) return;
    if (string.IsNullOrWhiteSpace(exePath))
      _mappings.Remove(fKey1Based);
    else
      _mappings[fKey1Based] = exePath.Trim();
    Save();
  }

  public string GetMapping(int fKey1Based)
  {
    string path;
    return _mappings.TryGetValue(fKey1Based, out path) ? path : null;
  }

  private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
  {
    if (nCode >= 0)
    {
      int msg = wParam.ToInt32();
      if (msg == WmKeyDown || msg == WmSysKeyDown)
      {
        int vk = Marshal.ReadInt32(lParam);
        TryLaunchFromKey(vk);
      }
    }
    return CallNextHookEx(_hookId, nCode, wParam, lParam);
  }

  private void TryLaunchFromKey(int vk)
  {
    if (vk < VkF1 || vk > VkF1 + 11)
      return;

    // Prefer live OS state — more reliable for Bluetooth (modifiers may arrive slightly apart)
    if (!IsDown(VkControl) || !IsDown(VkMenu) || !IsDown(VkShift))
      return;

    int fIndex = (vk - VkF1) + 1;
    if (!_mappings.ContainsKey(fIndex))
      return;

    // Debounce repeats / double reports from BT stacks
    long now = Environment.TickCount;
    if (fIndex == _lastLaunchF && unchecked((int)(now - _lastLaunchTick)) < 600)
      return;
    _lastLaunchF = fIndex;
    _lastLaunchTick = now;

    Launch(fIndex);
  }

  private static bool IsDown(int vk)
  {
    return (GetAsyncKeyState(vk) & 0x8000) != 0;
  }

  private void Launch(int fKey1Based)
  {
    string path;
    if (!_mappings.TryGetValue(fKey1Based, out path) || string.IsNullOrWhiteSpace(path))
      return;
    try
    {
      if (!File.Exists(path))
        return;

      // Launch without inheriting admin token when possible (works better for most apps)
      if (!StartUnelevated(path))
      {
        Process.Start(new ProcessStartInfo
        {
          FileName = path,
          UseShellExecute = true
        });
      }
    }
    catch
    {
      // Ignore launch failures
    }
  }

  /// <summary>
  /// Start process as normal user even if this app runs elevated (UAC).
  /// </summary>
  private static bool StartUnelevated(string path)
  {
    try
    {
      if (!IsElevated())
        return false;

      // explorer.exe re-launches the target at medium integrity
      Process.Start(new ProcessStartInfo
      {
        FileName = "explorer.exe",
        Arguments = "\"" + path + "\"",
        UseShellExecute = true
      });
      return true;
    }
    catch
    {
      return false;
    }
  }

  private static bool IsElevated()
  {
    try
    {
      using (var id = System.Security.Principal.WindowsIdentity.GetCurrent())
      {
        var principal = new System.Security.Principal.WindowsPrincipal(id);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
      }
    }
    catch
    {
      return false;
    }
  }

  public void Load()
  {
    _mappings.Clear();
    try
    {
      if (!File.Exists(ConfigPath)) return;
      string json = File.ReadAllText(ConfigPath, Encoding.UTF8);
      ParseSimpleJson(json);
    }
    catch
    {
      // Ignore corrupt config
    }
  }

  public void Save()
  {
    try
    {
      Directory.CreateDirectory(ConfigDirectory);
      var sb = new StringBuilder();
      sb.Append("{\n");
      bool first = true;
      foreach (var kv in _mappings)
      {
        if (!first) sb.Append(",\n");
        first = false;
        sb.Append("  \"F").Append(kv.Key).Append("\": \"");
        sb.Append(EscapeJson(kv.Value));
        sb.Append("\"");
      }
      sb.Append("\n}\n");
      File.WriteAllText(ConfigPath, sb.ToString(), Encoding.UTF8);
    }
    catch
    {
      // Ignore save failures
    }
  }

  private void ParseSimpleJson(string json)
  {
    int i = 0;
    while (i < json.Length)
    {
      int keyStart = json.IndexOf("\"F", i, StringComparison.OrdinalIgnoreCase);
      if (keyStart < 0) break;
      int numStart = keyStart + 2;
      int numEnd = numStart;
      while (numEnd < json.Length && char.IsDigit(json[numEnd])) numEnd++;
      if (numEnd == numStart) { i = keyStart + 2; continue; }
      int fNum;
      if (!int.TryParse(json.Substring(numStart, numEnd - numStart), out fNum) || fNum < 1 || fNum > 12)
      {
        i = numEnd;
        continue;
      }
      int colon = json.IndexOf(':', numEnd);
      if (colon < 0) break;
      int valStart = json.IndexOf('"', colon + 1);
      if (valStart < 0) break;
      int valEnd = valStart + 1;
      var pathSb = new StringBuilder();
      while (valEnd < json.Length)
      {
        char c = json[valEnd];
        if (c == '\\' && valEnd + 1 < json.Length)
        {
          char n = json[valEnd + 1];
          if (n == '"' || n == '\\' || n == '/')
          {
            pathSb.Append(n);
            valEnd += 2;
            continue;
          }
          if (n == 'n') { pathSb.Append('\n'); valEnd += 2; continue; }
          if (n == 'r') { pathSb.Append('\r'); valEnd += 2; continue; }
          if (n == 't') { pathSb.Append('\t'); valEnd += 2; continue; }
          pathSb.Append(n);
          valEnd += 2;
          continue;
        }
        if (c == '"') break;
        pathSb.Append(c);
        valEnd++;
      }
      string path = pathSb.ToString();
      if (!string.IsNullOrWhiteSpace(path))
        _mappings[fNum] = path;
      i = valEnd + 1;
    }
  }

  private static string EscapeJson(string s)
  {
    if (s == null) return "";
    var sb = new StringBuilder(s.Length + 8);
    foreach (char c in s)
    {
      switch (c)
      {
        case '\\': sb.Append("\\\\"); break;
        case '"': sb.Append("\\\""); break;
        case '\n': sb.Append("\\n"); break;
        case '\r': sb.Append("\\r"); break;
        case '\t': sb.Append("\\t"); break;
        default: sb.Append(c); break;
      }
    }
    return sb.ToString();
  }

  public void Dispose()
  {
    if (_disposed) return;
    Stop();
    _disposed = true;
  }

  private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

  [DllImport("user32.dll", SetLastError = true)]
  private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

  [DllImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool UnhookWindowsHookEx(IntPtr hhk);

  [DllImport("user32.dll", SetLastError = true)]
  private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

  [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  private static extern IntPtr GetModuleHandle(string lpModuleName);

  [DllImport("user32.dll")]
  private static extern short GetAsyncKeyState(int vKey);
}
