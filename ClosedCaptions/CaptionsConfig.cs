using Vintagestory.API.Client;

namespace ClosedCaptions;

public class CaptionsConfig
{
    public bool EnableCaptions = true;
    public float CaptionDurationSeconds = 4.0f;
    public float CaptionFadeoutSeconds = 1.0f;
    public float CaptionGUIScale = 1.0f;
    public EnumDialogArea DialogPosition = EnumDialogArea.RightBottom;
    public int MaxCaptions = 10;
    public double CaptionsWidth = 300;
    public double CaptionsHeight = 32;
    public string CaptionsFont = "Lora";
}