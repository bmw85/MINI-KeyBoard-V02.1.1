// Profile switch key - allows programming a key to switch profiles
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HIDTester;

public class LayerSwitchKey : UserControl
{
  private IContainer components;
  private RadioButton rbSwitchToLayer1;
  private RadioButton rbSwitchToLayer2;
  private RadioButton rbSwitchToLayer3;
  private RadioButton rbToggleLayers;
  private Label lblDescription;
  private Label lblInfo;

  private readonly Color textColor = UiTheme.TextPrimary;
  private readonly Color panelBackColor = UiTheme.BgPanel;
  private readonly Color layer1Color = UiTheme.Profile1;
  private readonly Color layer2Color = UiTheme.Profile2;
  private readonly Color layer3Color = UiTheme.Profile3;

  public LayerSwitchKey()
  {
    this.InitializeComponent();
    this.ApplyModernTheme();
  }

  private void ApplyModernTheme()
  {
    this.BackColor = this.panelBackColor;

    foreach (Control ctrl in this.Controls)
    {
      if (ctrl is RadioButton rb)
      {
        rb.ForeColor = this.textColor;
        rb.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
        rb.Cursor = Cursors.Hand;
      }
      else if (ctrl is Label lbl)
      {
        lbl.ForeColor = this.textColor;
      }
    }

    this.rbSwitchToLayer1.ForeColor = this.layer1Color;
    this.rbSwitchToLayer2.ForeColor = this.layer2Color;
    this.rbSwitchToLayer3.ForeColor = this.layer3Color;
    this.rbToggleLayers.ForeColor = UiTheme.Accent;
  }

  private void RbSwitchToLayer1_CheckedChanged(object sender, EventArgs e)
  {
    if (!this.rbSwitchToLayer1.Checked)
      return;

    SetLayerSwitchData(1);
    this.lblInfo.Text = "Клавиша переключит на Профиль 1";
    this.lblInfo.ForeColor = this.layer1Color;
  }

  private void RbSwitchToLayer2_CheckedChanged(object sender, EventArgs e)
  {
    if (!this.rbSwitchToLayer2.Checked)
      return;

    SetLayerSwitchData(2);
    this.lblInfo.Text = "Клавиша переключит на Профиль 2";
    this.lblInfo.ForeColor = this.layer2Color;
  }

  private void RbSwitchToLayer3_CheckedChanged(object sender, EventArgs e)
  {
    if (!this.rbSwitchToLayer3.Checked)
      return;

    SetLayerSwitchData(3);
    this.lblInfo.Text = "Клавиша переключит на Профиль 3";
    this.lblInfo.ForeColor = this.layer3Color;
  }

  private void RbToggleLayers_CheckedChanged(object sender, EventArgs e)
  {
    if (!this.rbToggleLayers.Checked)
      return;

    SetLayerSwitchData(0);
    this.lblInfo.Text = "Клавиша будет циклически менять профили (1 → 2 → 3 → 1)";
    this.lblInfo.ForeColor = UiTheme.Accent;
  }

  private void SetLayerSwitchData(byte targetLayer)
  {
    FormMain.KeyParam.Data_Send_Buff[(int)FormMain.KeyParam.KeyType_Num] = (byte)4;
    FormMain.KeyParam.Data_Send_Buff[5] = targetLayer;
    FormMain.KeyParam.Data_Send_Buff[6] = (byte)0xA1;

    for (int i = 0; i < 10; i++)
    {
      FormMain.KeyParam.KeyChar[i] = null;
    }

    if (targetLayer == 0)
    {
      FormMain.KeyParam.KeyChar[0] = "[Смена профиля]";
    }
    else
    {
      FormMain.KeyParam.KeyChar[0] = $"[Профиль {targetLayer}]";
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.rbSwitchToLayer1 = new RadioButton();
    this.rbSwitchToLayer2 = new RadioButton();
    this.rbSwitchToLayer3 = new RadioButton();
    this.rbToggleLayers = new RadioButton();
    this.lblDescription = new Label();
    this.lblInfo = new Label();
    this.SuspendLayout();

    this.lblDescription.AutoSize = true;
    this.lblDescription.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
    this.lblDescription.Location = new Point(20, 15);
    this.lblDescription.Name = "lblDescription";
    this.lblDescription.Size = new Size(400, 25);
    this.lblDescription.Text = "Смена профиля — назначить клавише смену профиля";

    this.rbSwitchToLayer1.AutoSize = true;
    this.rbSwitchToLayer1.Location = new Point(30, 60);
    this.rbSwitchToLayer1.Name = "rbSwitchToLayer1";
    this.rbSwitchToLayer1.Size = new Size(200, 25);
    this.rbSwitchToLayer1.Text = "Перейти на Профиль 1";
    this.rbSwitchToLayer1.UseVisualStyleBackColor = true;
    this.rbSwitchToLayer1.CheckedChanged += new EventHandler(this.RbSwitchToLayer1_CheckedChanged);

    this.rbSwitchToLayer2.AutoSize = true;
    this.rbSwitchToLayer2.Location = new Point(30, 95);
    this.rbSwitchToLayer2.Name = "rbSwitchToLayer2";
    this.rbSwitchToLayer2.Size = new Size(200, 25);
    this.rbSwitchToLayer2.Text = "Перейти на Профиль 2";
    this.rbSwitchToLayer2.UseVisualStyleBackColor = true;
    this.rbSwitchToLayer2.CheckedChanged += new EventHandler(this.RbSwitchToLayer2_CheckedChanged);

    this.rbSwitchToLayer3.AutoSize = true;
    this.rbSwitchToLayer3.Location = new Point(30, 130);
    this.rbSwitchToLayer3.Name = "rbSwitchToLayer3";
    this.rbSwitchToLayer3.Size = new Size(200, 25);
    this.rbSwitchToLayer3.Text = "Перейти на Профиль 3";
    this.rbSwitchToLayer3.UseVisualStyleBackColor = true;
    this.rbSwitchToLayer3.CheckedChanged += new EventHandler(this.RbSwitchToLayer3_CheckedChanged);

    this.rbToggleLayers.AutoSize = true;
    this.rbToggleLayers.Location = new Point(30, 175);
    this.rbToggleLayers.Name = "rbToggleLayers";
    this.rbToggleLayers.Size = new Size(280, 25);
    this.rbToggleLayers.Text = "Цикл профилей (1 → 2 → 3)";
    this.rbToggleLayers.UseVisualStyleBackColor = true;
    this.rbToggleLayers.CheckedChanged += new EventHandler(this.RbToggleLayers_CheckedChanged);

    this.lblInfo.AutoSize = true;
    this.lblInfo.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
    this.lblInfo.ForeColor = Color.Gray;
    this.lblInfo.Location = new Point(30, 220);
    this.lblInfo.Name = "lblInfo";
    this.lblInfo.Size = new Size(300, 20);
    this.lblInfo.Text = "Выберите вариант смены профиля выше";

    this.AutoScaleDimensions = new SizeF(6F, 13F);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.Controls.Add(this.lblDescription);
    this.Controls.Add(this.rbSwitchToLayer1);
    this.Controls.Add(this.rbSwitchToLayer2);
    this.Controls.Add(this.rbSwitchToLayer3);
    this.Controls.Add(this.rbToggleLayers);
    this.Controls.Add(this.lblInfo);
    this.Name = "LayerSwitchKey";
    this.Size = new Size(600, 300);
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
