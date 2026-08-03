using System;
using System.IO;
using System.Windows.Forms;

namespace HIDTester;

internal static class Program
{
  [STAThread]
  private static void Main()
  {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
    Application.ThreadException += (s, e) => ShowFatal(e.Exception);
    AppDomain.CurrentDomain.UnhandledException += (s, e) =>
    {
      if (e.ExceptionObject is Exception ex)
        ShowFatal(ex);
    };

    try
    {
      Application.Run((Form) new FormMain());
    }
    catch (Exception ex)
    {
      ShowFatal(ex);
    }
  }

  private static void ShowFatal(Exception ex)
  {
    try
    {
      string path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        "MINI-KeyBoard-error.txt");
      File.WriteAllText(path, ex.ToString());
    }
    catch
    {
    }

    MessageBox.Show(
      ex.ToString(),
      "MINI KeyBoard — ошибка запуска",
      MessageBoxButtons.OK,
      MessageBoxIcon.Error);
  }
}
