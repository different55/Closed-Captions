using Vintagestory.API.Client;

namespace ClosedCaptions;

public class CaptionsConfig
{
    /// <summary>
    ///  Enable or disable closed captions.
    /// </summary>
    public bool Enabled = true;
    /// <summary>
    ///  Set how long captions are displayed after the sound stops, in seconds.
    ///  TODO: Do.
    /// </summary>
    public float Duration = 4.0f;
    /// <summary>
    ///  Set how long captions take to fade out, in seconds.
    /// </summary>
    public float FadeDuration = 1.0f;
    /// <summary>
    ///  Set the scale of the captions UI independent of the global UI scale.
    ///  TODO: Implement using RuntimeEnv.
    /// </summary>
    public float UIScale = 1.0f;
    /// <summary>
    ///  Set the position of the captions on the screen.
    /// </summary>
    public EnumDialogArea Position = EnumDialogArea.RightBottom;
    /// <summary>
    ///  Set the maximum number of captions to display at once.
    /// </summary>
    public int MaxCaptions = 10;
    /// <summary>
    ///  Set the width of the captions box in pixels.
    /// </summary>
    public double Width = 300;
    /// <summary>
    ///  Set the height of each caption in pixels.
    /// </summary>
    public double Height = 34;
    /// <summary>
    ///  Set the padding between the captions box and the screen edges in pixels.
    /// </summary>
    public double Padding = 16;
    /// <summary>
    ///  Set the font used for captions.
    /// </summary>
    public string Font = "Lora";
    /// <summary>
    ///  Set the font size used for captions.
    /// </summary>
    public float FontSize = 28;
}