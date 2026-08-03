using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace HIDTester;

/// <summary>
/// Bind a physical KEY/knob to launch an .exe (written as Ctrl+Alt+Shift+Fn on device).
/// </summary>
public class OpenAppKey : UserControl
{
  private TextBox txtPath;
  private Button btnBrowse;
  private Button btnAssign;
  private Label lblTitle;
  private Label lblPath;
  private Label lblSelectedKey;
  private Label lblNote;
  private Label lblMapping;
  private Timer refreshTimer;

  public OpenAppKey()
  {
    BuildUi();
    this.refreshTimer = new Timer();
    this.refreshTimer.Interval = 400;
    this.refreshTimer.Tick += (s, e) => RefreshSelectedKeyLabel();
    this.refreshTimer.Start();
    RefreshSelectedKeyLabel();
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      this.refreshTimer?.Stop();
      this.refreshTimer?.Dispose();
    }
    base.Dispose(disposing);
  }

  private void BuildUi()
  {
    this.BackColor = UiTheme.BgPanel;
    this.Size = new Size(640, 340);

    this.lblTitle = new Label();
    this.lblTitle.Text = "Открыть приложение";
    this.lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
    this.lblTitle.ForeColor = UiTheme.TextPrimary;
    this.lblTitle.Location = new Point(20, 16);
    this.lblTitle.AutoSize = true;

    this.lblSelectedKey = new Label();
    this.lblSelectedKey.Text = "Клавиша не выбрана";
    this.lblSelectedKey.ForeColor = Color.FromArgb(0xFF, 0xC0, 0x70);
    this.lblSelectedKey.Font = UiTheme.UiFontBold;
    this.lblSelectedKey.Location = new Point(20, 48);
    this.lblSelectedKey.AutoSize = true;

    this.lblPath = new Label();
    this.lblPath.Text = "Путь к .exe:";
    this.lblPath.ForeColor = UiTheme.TextPrimary;
    this.lblPath.Font = UiTheme.UiFont;
    this.lblPath.Location = new Point(20, 82);
    this.lblPath.AutoSize = true;

    this.txtPath = new TextBox();
    this.txtPath.Location = new Point(20, 105);
    this.txtPath.Size = new Size(420, 25);
    this.txtPath.ReadOnly = true;
    UiTheme.StyleTextBox(this.txtPath);

    this.btnBrowse = new Button();
    this.btnBrowse.Text = "Обзор…";
    this.btnBrowse.Location = new Point(450, 102);
    this.btnBrowse.Size = new Size(100, 28);
    UiTheme.StyleActionButton(this.btnBrowse);
    this.btnBrowse.Click += BtnBrowse_Click;

    this.btnAssign = new Button();
    this.btnAssign.Text = "Записать на клавишу";
    this.btnAssign.Location = new Point(20, 150);
    this.btnAssign.Size = new Size(200, 40);
    UiTheme.StyleActionButton(this.btnAssign);
    this.btnAssign.Click += BtnAssign_Click;

    this.lblMapping = new Label();
    this.lblMapping.ForeColor = Color.FromArgb(0xA0, 0xA0, 0xA0);
    this.lblMapping.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
    this.lblMapping.Location = new Point(230, 160);
    this.lblMapping.Size = new Size(360, 25);
    this.lblMapping.Text = "";

    this.lblNote = new Label();
    this.lblNote.Text =
      "1. Нажмите KEY1…KEY16 (или ручку K1–K3) сверху\n" +
      "2. Выберите .exe\n" +
      "3. Нажмите «Записать на клавишу» — комбинация сразу\n" +
      "   прошивается на устройство.\n\n" +
      "Чтобы запуск работал, эта программа должна быть открыта\n" +
      "(можно свернуть в трей).";
    this.lblNote.ForeColor = UiTheme.Accent;
    this.lblNote.Font = UiTheme.UiFont;
    this.lblNote.Location = new Point(20, 210);
    this.lblNote.Size = new Size(560, 110);

    this.Controls.Add(this.lblTitle);
    this.Controls.Add(this.lblSelectedKey);
    this.Controls.Add(this.lblPath);
    this.Controls.Add(this.txtPath);
    this.Controls.Add(this.btnBrowse);
    this.Controls.Add(this.btnAssign);
    this.Controls.Add(this.lblMapping);
    this.Controls.Add(this.lblNote);
  }

  private void RefreshSelectedKeyLabel()
  {
    byte keyNum = FormMain.KeyParam.Data_Send_Buff[(int)FormMain.KeyParam.KeySet_KeyNum];
    if (keyNum == 0)
    {
      this.lblSelectedKey.Text = "Сначала выберите KEY1…KEY16 сверху";
      this.lblMapping.Text = "";
      return;
    }

    string name = DescribeKey(keyNum);
    int fIndex = ((keyNum - 1) % 12) + 1;
    this.lblSelectedKey.Text = $"Выбрано: {name}  →  будет Ctrl+Alt+Shift+F{fIndex}";

    var main = FindFormMain();
    string existing = main?.AppLauncher?.GetMapping(fIndex);
    this.lblMapping.Text = string.IsNullOrEmpty(existing)
      ? ""
      : "Сейчас на F" + fIndex + ": " + Path.GetFileName(existing);
  }

  private static string DescribeKey(byte keyNum)
  {
    if (keyNum >= 1 && keyNum <= 16)
      return "KEY" + keyNum;
    switch (keyNum)
    {
      case 17: return "K2 Влево"; // check actual mapping from click handlers
      default: return "Кнопка #" + keyNum;
    }
  }

  private FormMain FindFormMain()
  {
    Control c = this;
    while (c != null)
    {
      if (c is FormMain fm) return fm;
      c = c.Parent;
    }
    foreach (Form f in Application.OpenForms)
    {
      if (f is FormMain fm) return fm;
    }
    return null;
  }

  private void BtnBrowse_Click(object sender, EventArgs e)
  {
    using (var dlg = new OpenFileDialog())
    {
      dlg.Filter = "Программы (*.exe)|*.exe|Все файлы (*.*)|*.*";
      dlg.Title = "Выберите приложение";
      if (dlg.ShowDialog(this) == DialogResult.OK)
        this.txtPath.Text = dlg.FileName;
    }
  }

  private void BtnAssign_Click(object sender, EventArgs e)
  {
    string path = this.txtPath.Text?.Trim();
    if (string.IsNullOrEmpty(path) || !File.Exists(path))
    {
      MessageBox.Show(
        "Выберите существующий файл .exe.",
        "Приложение",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning);
      return;
    }

    var main = FindFormMain();
    if (main == null)
    {
      MessageBox.Show("Главное окно не найдено.", "Приложение", MessageBoxButtons.OK, MessageBoxIcon.Error);
      return;
    }

    string msg;
    if (!main.ProgramOpenAppToSelectedKey(path, out msg))
    {
      MessageBox.Show(msg, "Приложение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      return;
    }

    MessageBox.Show(msg, "Записано", MessageBoxButtons.OK, MessageBoxIcon.Information);
    RefreshSelectedKeyLabel();
  }
}
