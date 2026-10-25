using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HIDTester;

public class MULKey : UserControl
{
  private const int ButtonWidth = 130;
  private const int ButtonHeight = 26;
  private const int ColumnGap = 8;
  private const int RowGap = 6;
  private const int Columns = 5;

  private IContainer components;
  private Button KEY_Play;
  private Button KEY_VolumeAdd;
  private Button KEY_VolumeSub;
  private Button KEY_PreSong;
  private Button KEY_NextSong;
  private Button KEY_Mute;

  private readonly Color panelBackColor = UiTheme.BgPanel;

  private sealed class ConsumerAction
  {
    public string Text { get; }
    public ushort Usage { get; }

    public ConsumerAction(string text, ushort usage)
    {
      this.Text = text;
      this.Usage = usage;
    }
  }

  public MULKey()
  {
    this.InitializeComponent();
    this.ApplyModernTheme();
  }

  private void ApplyModernTheme()
  {
    this.BackColor = this.panelBackColor;
    UiTheme.ApplyToButtons(this);
  }

  private void MULGeneral_Char_Set()
  {
    FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KeyType_Num] |= (byte) 2;
  }

  private void SetConsumerUsage(ushort usage, string text)
  {
    if (FormMain.KeyParam.ReportID == (byte) 0 || FormMain.KeyParam.ReportID == (byte) 2)
    {
      MessageBox.Show(
        $"Эта команда поддерживается только устройствами с ReportID 3. Текущий ReportID: {FormMain.KeyParam.ReportID}.",
        "Мультимедийная команда",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information);
      return;
    }

    FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num] = (byte) (usage & 0xFF);
    FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num + 1] = (byte) (usage >> 8);
    FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = text;
    this.MULGeneral_Char_Set();
  }

  private void KEY_Play_Click(object sender, EventArgs e)
  {
    if (FormMain.KeyParam.ReportID == (byte) 0)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num] = (byte) 64;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_Play.Text;
      this.MULGeneral_Char_Set();
    }
    else if (FormMain.KeyParam.ReportID == (byte) 2)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num + 1] = (byte) 4;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_Play.Text;
      this.MULGeneral_Char_Set();
    }
    else
    {
      this.SetConsumerUsage(0x00CD, this.KEY_Play.Text);
    }
  }

  private void KEY_PreSong_Click(object sender, EventArgs e)
  {
    if (FormMain.KeyParam.ReportID == (byte) 0)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num] = (byte) 128;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_PreSong.Text;
      this.MULGeneral_Char_Set();
    }
    else if (FormMain.KeyParam.ReportID == (byte) 2)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num + 1] = (byte) 11;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_PreSong.Text;
      this.MULGeneral_Char_Set();
    }
    else
    {
      this.SetConsumerUsage(0x00B6, this.KEY_PreSong.Text);
    }
  }

  private void KEY_NextSong_Click(object sender, EventArgs e)
  {
    if (FormMain.KeyParam.ReportID == (byte) 0)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num + 1] = (byte) 1;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_NextSong.Text;
      this.MULGeneral_Char_Set();
    }
    else if (FormMain.KeyParam.ReportID == (byte) 2)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num + 1] = (byte) 10;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_NextSong.Text;
      this.MULGeneral_Char_Set();
    }
    else
    {
      this.SetConsumerUsage(0x00B5, this.KEY_NextSong.Text);
    }
  }

  private void KEY_Mute_Click(object sender, EventArgs e)
  {
    if (FormMain.KeyParam.ReportID == (byte) 0)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num] = (byte) 4;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_Mute.Text;
      this.MULGeneral_Char_Set();
    }
    else if (FormMain.KeyParam.ReportID == (byte) 2)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num + 1] = (byte) 1;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_Mute.Text;
      this.MULGeneral_Char_Set();
    }
    else
    {
      this.SetConsumerUsage(0x00E2, this.KEY_Mute.Text);
    }
  }

  private void KEY_VolumeAdd_Click(object sender, EventArgs e)
  {
    if (FormMain.KeyParam.ReportID == (byte) 0)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num] = (byte) 2;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_VolumeAdd.Text;
      this.MULGeneral_Char_Set();
    }
    else if (FormMain.KeyParam.ReportID == (byte) 2)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num] = (byte) 64;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_VolumeAdd.Text;
      this.MULGeneral_Char_Set();
    }
    else
    {
      this.SetConsumerUsage(0x00E9, this.KEY_VolumeAdd.Text);
    }
  }

  private void KEY_VolumeSub_Click(object sender, EventArgs e)
  {
    if (FormMain.KeyParam.ReportID == (byte) 0)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num] = (byte) 1;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_VolumeSub.Text;
      this.MULGeneral_Char_Set();
    }
    else if (FormMain.KeyParam.ReportID == (byte) 2)
    {
      FormMain.KeyParam.Data_Send_Buff[(int) FormMain.KeyParam.KEY_Char_Num] = (byte) 128;
      FormMain.KeyParam.KeyChar[(int) FormMain.KeyParam.KEY_Char_Num - 5] = this.KEY_VolumeSub.Text;
      this.MULGeneral_Char_Set();
    }
    else
    {
      this.SetConsumerUsage(0x00EA, this.KEY_VolumeSub.Text);
    }
  }

  private Button CreateButton(string text, EventHandler clickHandler)
  {
    Button button = new Button();
    button.Name = "KEY_" + text.Replace(" ", "_").Replace("/", "_").Replace("+", "Add").Replace("-", "Sub");
    button.Size = new Size(ButtonWidth, ButtonHeight);
    button.Text = text;
    button.UseVisualStyleBackColor = true;
    button.Click += clickHandler;
    return button;
  }

  private void AddGroupTitle(string text, int y)
  {
    Label label = new Label();
    label.Text = text;
    label.Font = UiTheme.UiFontBold;
    label.ForeColor = UiTheme.TextPrimary;
    label.Location = new Point(12, y);
    label.Size = new Size(680, 20);
    this.Controls.Add(label);
  }

  private void AddButtonGroup(string title, Button[] buttons, ref int y)
  {
    this.AddGroupTitle(title, y);
    y += 24;

    for (int index = 0; index < buttons.Length; ++index)
    {
      buttons[index].Location = new Point(
        12 + (index % Columns) * (ButtonWidth + ColumnGap),
        y + (index / Columns) * (ButtonHeight + RowGap));
      buttons[index].TabIndex = this.Controls.Count;
      this.Controls.Add(buttons[index]);
    }

    y += ((buttons.Length + Columns - 1) / Columns) * (ButtonHeight + RowGap);
  }

  private Button[] CreateConsumerButtons(ConsumerAction[] actions)
  {
    Button[] buttons = new Button[actions.Length];
    for (int index = 0; index < actions.Length; ++index)
    {
      ConsumerAction action = actions[index];
      buttons[index] = this.CreateButton(action.Text, (sender, args) => this.SetConsumerUsage(action.Usage, action.Text));
    }
    return buttons;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.SuspendLayout();

    int y = 8;
    this.KEY_Play = this.CreateButton("Play/Pause", this.KEY_Play_Click);
    this.KEY_PreSong = this.CreateButton("Prev Song", this.KEY_PreSong_Click);
    this.KEY_NextSong = this.CreateButton("Next Song", this.KEY_NextSong_Click);
    this.KEY_Mute = this.CreateButton("Mute", this.KEY_Mute_Click);
    this.KEY_VolumeAdd = this.CreateButton("Volume+", this.KEY_VolumeAdd_Click);
    this.KEY_VolumeSub = this.CreateButton("Volume-", this.KEY_VolumeSub_Click);

    this.AddButtonGroup("Медиа — основные", new Button[]
    {
      this.KEY_Play,
      this.KEY_PreSong,
      this.KEY_NextSong,
      this.KEY_Mute,
      this.KEY_VolumeAdd,
      this.KEY_VolumeSub
    }, ref y);

    this.AddButtonGroup("Медиа — дополнительные", this.CreateConsumerButtons(new ConsumerAction[]
    {
      new ConsumerAction("Stop", 0x00B7),
      new ConsumerAction("Play", 0x00B0),
      new ConsumerAction("Pause", 0x00B1),
      new ConsumerAction("Fast Forward", 0x00B3),
      new ConsumerAction("Rewind", 0x00B4),
      new ConsumerAction("Repeat", 0x00BC),
      new ConsumerAction("Shuffle", 0x00B9),
      new ConsumerAction("Eject", 0x00B8)
    }), ref y);

    this.AddButtonGroup("Браузер", this.CreateConsumerButtons(new ConsumerAction[]
    {
      new ConsumerAction("Browser Back", 0x0224),
      new ConsumerAction("Browser Forward", 0x0225),
      new ConsumerAction("Browser Refresh", 0x0227),
      new ConsumerAction("Browser Stop", 0x0226),
      new ConsumerAction("Browser Home", 0x0223),
      new ConsumerAction("Browser Search", 0x0221),
      new ConsumerAction("Favorites", 0x0228)
    }), ref y);

    this.AddButtonGroup("Запуск приложений", this.CreateConsumerButtons(new ConsumerAction[]
    {
      new ConsumerAction("Launch Browser", 0x0196),
      new ConsumerAction("Launch Calculator", 0x0192),
      new ConsumerAction("Launch Mail", 0x018A),
      new ConsumerAction("Launch Media Player", 0x0193),
      new ConsumerAction("Launch File Explorer", 0x0194)
    }), ref y);

    this.AddButtonGroup("Система", this.CreateConsumerButtons(new ConsumerAction[]
    {
      new ConsumerAction("Sleep", 0x0032),
      new ConsumerAction("Power", 0x0030)
    }), ref y);

    this.AutoScaleDimensions = new SizeF(6F, 12F);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.AutoScroll = true;
    this.Name = nameof(MULKey);
    this.Size = new Size(760, 385);
    this.ResumeLayout(false);
  }
}
