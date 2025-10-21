using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Vintagestory.API.Client;

namespace ClosedCaptions;

public class CaptionsConfig
{
    public bool Enabled = true;
    [Description("How long to persist captions after the sound stops.")]
    [Range(0, 10)]
    public float Duration = 4.0f;
    [Description("How long it takes for captions to fade out.")]
    [Range(0, 4)]
    public float FadeDuration = 1.0f;
    [Description("Set the maximum number of captions to display at once.")]
    [Range(1, 30)]
    public int MaxCaptions = 10;
    [Description("Set the position of the captions on the screen.")]
    [Category("Position/Size")]
    public EnumDialogArea Position = EnumDialogArea.RightBottom;
    [Description("Set the width of the captions box in pixels.")]
    [Category("Position/Size")]
    [Range(256, 512)]
    public int Width = 300;
    [Description("Set the height of each caption line in pixels.")]
    [Category("Position/Size")]
    [Range(16, 64)]
    public int Height = 34;
    [Description("Set the padding between the captions box and the screen edges in pixels.")]
    [Category("Position/Size")]
    [Range(0, 64)]
    public double Padding = 16;
    [Description("Set the font name.")]
    [Category("Font")]
    public string Font = "Lora";
    [Description("Set the font size.")]
    [Category("Font")]
    [Range(8, 64)]
    public float FontSize = 28;
    [DisplayName("Reload Captions (Save First)")]
    [Browsable(true)]
    [Description("Reload captions with updated settings (hit Save first!)")]
    [Category("Actions")]
    public static void ReloadCaptions() => CaptionsModSystem.Reload();
}