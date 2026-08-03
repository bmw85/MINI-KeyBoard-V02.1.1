using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HIDTester;

public class LayerFun : UserControl
{
  private IContainer components;
  private RadioButton KEY_FunLayer1;
  private RadioButton KEY_FunLayer2;
  private RadioButton KEY_FunLayer3;
  private Label lblTitle;

  public LayerFun()
  {
    this.InitializeComponent();
    this.ApplyModernTheme();
  }

  private void ApplyModernTheme()
  {
    this.BackColor = UiTheme.BgElevated;
    this.ForeColor = UiTheme.TextPrimary;

    if (this.lblTitle != null)
    {
      this.lblTitle.ForeColor = UiTheme.TextPrimary;
      this.lblTitle.Font = UiTheme.UiFontBold;
      this.lblTitle.Text = "Редактировать профиль:";
      this.lblTitle.BackColor = Color.Transparent;
    }

    StyleProfileRadio(this.KEY_FunLayer1, UiTheme.Profile1, "Профиль 1");
    StyleProfileRadio(this.KEY_FunLayer2, UiTheme.Profile2, "Профиль 2");
    StyleProfileRadio(this.KEY_FunLayer3, UiTheme.Profile3, "Профиль 3");
  }

  private void StyleProfileRadio(RadioButton rb, Color accent, string text)
  {
    if (rb == null) return;

    rb.Text = text;
    rb.ForeColor = UiTheme.TextPrimary;
    rb.BackColor = UiTheme.BgElevated;
    rb.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    rb.Cursor = Cursors.Hand;
    rb.AutoSize = false;
    rb.Size = new Size(150, 24);
    rb.UseVisualStyleBackColor = false;
    rb.FlatStyle = FlatStyle.Standard;
    // Keep accent via Tag for checked redraw
    rb.Tag = accent;
    rb.CheckedChanged += (s, e) => UpdateRadioAccent(rb);
    UpdateRadioAccent(rb);
  }

  private void UpdateRadioAccent(RadioButton rb)
  {
    if (rb == null) return;
    Color accent = rb.Tag is Color c ? c : UiTheme.Accent;
    rb.ForeColor = rb.Checked ? Color.White : UiTheme.TextPrimary;
    rb.BackColor = rb.Checked ? accent : UiTheme.BgElevated;
  }

  private void KEY_FunLayer1_CheckedChanged(object sender, EventArgs e)
  {
    if (!this.KEY_FunLayer1.Checked)
      return;
    FormMain.KeyParam.KEY_Cur_Layer = (byte) 1;
    FormMain.KeyParam.PageBet_Inte_Cmd = (byte) 1;
  }

  private void KEY_FunLayer2_CheckedChanged(object sender, EventArgs e)
  {
    if (!this.KEY_FunLayer2.Checked)
      return;
    FormMain.KeyParam.KEY_Cur_Layer = (byte) 2;
    FormMain.KeyParam.PageBet_Inte_Cmd = (byte) 1;
  }

  private void KEY_FunLayer3_CheckedChanged(object sender, EventArgs e)
  {
    if (!this.KEY_FunLayer3.Checked)
      return;
    FormMain.KeyParam.KEY_Cur_Layer = (byte) 3;
    FormMain.KeyParam.PageBet_Inte_Cmd = (byte) 1;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.KEY_FunLayer1 = new RadioButton();
    this.KEY_FunLayer2 = new RadioButton();
    this.KEY_FunLayer3 = new RadioButton();
    this.lblTitle = new Label();
    this.SuspendLayout();

    this.lblTitle.AutoSize = true;
    this.lblTitle.Location = new Point(6, 4);
    this.lblTitle.Name = "lblTitle";
    this.lblTitle.Size = new Size(160, 15);
    this.lblTitle.Text = "Редактировать профиль:";

    this.KEY_FunLayer1.Location = new Point(6, 28);
    this.KEY_FunLayer1.Name = "KEY_FunLayer1";
    this.KEY_FunLayer1.Size = new Size(150, 24);
    this.KEY_FunLayer1.TabIndex = 0;
    this.KEY_FunLayer1.TabStop = true;
    this.KEY_FunLayer1.Checked = true;
    this.KEY_FunLayer1.Text = "Профиль 1";
    this.KEY_FunLayer1.CheckedChanged += new EventHandler(this.KEY_FunLayer1_CheckedChanged);

    this.KEY_FunLayer2.Location = new Point(6, 56);
    this.KEY_FunLayer2.Name = "KEY_FunLayer2";
    this.KEY_FunLayer2.Size = new Size(150, 24);
    this.KEY_FunLayer2.TabIndex = 1;
    this.KEY_FunLayer2.Text = "Профиль 2";
    this.KEY_FunLayer2.CheckedChanged += new EventHandler(this.KEY_FunLayer2_CheckedChanged);

    this.KEY_FunLayer3.Location = new Point(6, 84);
    this.KEY_FunLayer3.Name = "KEY_FunLayer3";
    this.KEY_FunLayer3.Size = new Size(150, 24);
    this.KEY_FunLayer3.TabIndex = 2;
    this.KEY_FunLayer3.Text = "Профиль 3";
    this.KEY_FunLayer3.CheckedChanged += new EventHandler(this.KEY_FunLayer3_CheckedChanged);

    this.AutoScaleMode = AutoScaleMode.None;
    this.Controls.Add(this.lblTitle);
    this.Controls.Add(this.KEY_FunLayer3);
    this.Controls.Add(this.KEY_FunLayer2);
    this.Controls.Add(this.KEY_FunLayer1);
    this.Name = nameof(LayerFun);
    this.Size = new Size(170, 118);
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
