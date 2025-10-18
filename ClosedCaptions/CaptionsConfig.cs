using Vintagestory.API.Client;

namespace ClosedCaptions;

public class CaptionsConfig
{
    public bool Enabled = true;
    public float Duration = 4.0f;
    public float FadeDuration = 1.0f;
    public float UIScale = 1.0f;
    public EnumDialogArea Position = EnumDialogArea.RightBottom;
    public int MaxCaptions = 10;
    public double Width = 300;
    public double Height = 34;
    public double Padding = 16;
    public string Font = "Lora";
    public float FontSize = 28;
}