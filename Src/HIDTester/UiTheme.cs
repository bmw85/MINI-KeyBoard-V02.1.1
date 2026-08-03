using System.Drawing;
using System.Windows.Forms;

namespace HIDTester;

/// <summary>Shared dark minimalist theme helpers.</summary>
public static class UiTheme
{
  public static readonly Color BgDeep = Color.FromArgb(0x12, 0x12, 0x12);
  public static readonly Color BgPanel = Color.FromArgb(0x1A, 0x1A, 0x1A);
  public static readonly Color BgElevated = Color.FromArgb(0x22, 0x22, 0x22);
  public static readonly Color BgControl = Color.FromArgb(0x2A, 0x2A, 0x2A);
  public static readonly Color TextPrimary = Color.FromArgb(0xE8, 0xE8, 0xE8);
  public static readonly Color Accent = Color.FromArgb(0xC8, 0xC8, 0xC8);
  public static readonly Color Border = Color.FromArgb(0x3A, 0x3A, 0x3A);
  public static readonly Color KeyDefault = Color.FromArgb(0x2A, 0x2A, 0x2A);
  public static readonly Color KeyHover = Color.FromArgb(0x38, 0x38, 0x38);
  public static readonly Color KeySelected = Color.FromArgb(0x48, 0x48, 0x48);
  public static readonly Color MenuBack = Color.FromArgb(0x16, 0x16, 0x16);
  public static readonly Color MenuHover = Color.FromArgb(0x2A, 0x2A, 0x2A);
  public static readonly Color Profile1 = Color.FromArgb(0x55, 0x55, 0x55);
  public static readonly Color Profile2 = Color.FromArgb(0x3D, 0x5A, 0x3D);
  public static readonly Color Profile3 = Color.FromArgb(0x5A, 0x3D, 0x3D);

  public static readonly Font UiFont = new Font("Segoe UI", 9F, FontStyle.Regular);
  public static readonly Font UiFontBold = new Font("Segoe UI", 9F, FontStyle.Bold);
  public static readonly Font KeyFont = new Font("Segoe UI", 8F, FontStyle.Bold);

  public static void StyleButton(Button btn, bool isAction = false)
  {
    if (btn == null) return;
    btn.FlatStyle = FlatStyle.Flat;
    btn.FlatAppearance.BorderSize = 1;
    btn.FlatAppearance.BorderColor = Border;
    btn.FlatAppearance.MouseOverBackColor = KeyHover;
    btn.FlatAppearance.MouseDownBackColor = KeySelected;
    btn.BackColor = isAction ? Color.FromArgb(0x35, 0x35, 0x35) : KeyDefault;
    btn.ForeColor = TextPrimary;
    btn.Font = isAction ? UiFontBold : KeyFont;
    btn.Cursor = Cursors.Hand;
    btn.UseVisualStyleBackColor = false;
  }

  public static void StyleTextBox(TextBox txt)
  {
    if (txt == null) return;
    txt.BackColor = BgControl;
    txt.ForeColor = TextPrimary;
    txt.BorderStyle = BorderStyle.FixedSingle;
    txt.Font = new Font("Consolas", 10F);
  }

  public static void StyleKeyButton(Button btn)
  {
    StyleButton(btn, false);
  }

  public static void StyleActionButton(Button btn)
  {
    StyleButton(btn, true);
    btn.FlatAppearance.BorderSize = 0;
  }

  public static void ApplyToButtons(Control parent)
  {
    if (parent == null) return;
    foreach (Control ctrl in parent.Controls)
    {
      if (ctrl is Button btn)
        StyleButton(btn);
      else if (ctrl is Label lbl)
      {
        lbl.ForeColor = TextPrimary;
        lbl.Font = UiFont;
      }
      else if (ctrl is TextBox tb)
        StyleTextBox(tb);
      else if (ctrl is NumericUpDown num)
      {
        num.BackColor = BgControl;
        num.ForeColor = TextPrimary;
      }
      else if (ctrl is RadioButton rb)
      {
        rb.ForeColor = TextPrimary;
        rb.Font = UiFont;
        rb.Cursor = Cursors.Hand;
      }
    }
  }
}
