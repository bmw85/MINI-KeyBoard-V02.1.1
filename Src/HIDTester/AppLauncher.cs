using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace HIDTester;

/// <summary>
/// Registers Ctrl+Alt+Shift+F1..F12 hotkeys and launches mapped executables.
/// Mappings stored in %AppData%\MINI KeyBoard\app_launches.json
/// </summary>
public sealed class AppLauncher : IMessageFilter, IDisposable
{
  private const int WmHotkey = 0x0312;
  private const uint ModAlt = 0x0001;
  private const uint ModControl = 0x0002;
  private const uint ModShift = 0x0004;
  private const uint ModNorepeat = 0x4000;

  // Virtual-key codes for F1..F12
  private static readonly uint[] VkFKeys =
  {
    0x70, 0x71, 0x72, 0x73, 0x74, 0x75,
    0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B
  };

  // HID usage IDs F1=58 .. F12=69
  public static byte HidCodeForFKey(int fIndex1Based)
  {
    if (fIndex1Based < 1 || fIndex1Based > 12) return 0;
    return (byte)(57 + fIndex1Based); // F1=58
  }

  public static byte FunModifierCtrlAltShift => (byte)(1 | 2 | 4); // Ctrl|Shift|Alt

  private readonly IntPtr _hwnd;
  private readonly Dictionary<int, string> _mappings = new Dictionary<int, string>();
  private bool _listening;
  private bool _disposed;

  public AppLauncher(IntPtr hwnd)
  {
    _hwnd = hwnd;
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
    RegisterAll();
    Application.AddMessageFilter(this);
    _listening = true;
  }

  public void Stop()
  {
    if (!_listening) return;
    Application.RemoveMessageFilter(this);
    UnregisterAll();
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
    // Re-register so unused slots are free / used slots active
    if (_listening)
    {
      UnregisterAll();
      RegisterAll();
    }
  }

  public string GetMapping(int fKey1Based)
  {
    string path;
    return _mappings.TryGetValue(fKey1Based, out path) ? path : null;
  }

  public bool PreFilterMessage(ref Message m)
  {
    if (m.Msg == WmHotkey)
    {
      int id = m.WParam.ToInt32();
      if (id >= 1 && id <= 12)
        Launch(id);
      return true;
    }
    return false;
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
      Process.Start(new ProcessStartInfo
      {
        FileName = path,
        UseShellExecute = true
      });
    }
    catch
    {
      // Ignore launch failures
    }
  }

  private void RegisterAll()
  {
    uint mods = ModControl | ModAlt | ModShift | ModNorepeat;
    for (int i = 1; i <= 12; i++)
    {
      if (!_mappings.ContainsKey(i)) continue;
      RegisterHotKey(_hwnd, i, mods, VkFKeys[i - 1]);
    }
  }

  private void UnregisterAll()
  {
    for (int i = 1; i <= 12; i++)
      UnregisterHotKey(_hwnd, i);
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
    // Expect: { "F1": "C:\\path\\app.exe", "F2": "..." }
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

  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
