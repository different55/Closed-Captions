using Vintagestory.API.Client;

namespace ClosedCaptions;

public class SettingsDialog : HudElement
{
    private CaptionsConfig Cfg => CaptionsSystem.Config; 
    
    public SettingsDialog(ICoreClientAPI capi) : base(capi)
    {
        ComposeGui();
    }

    public void ComposeGui()
    {
        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        ElementBounds textBounds = ElementBounds.Fixed(0, 0, 300, 600);
        ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;
        bgBounds.WithChildren(textBounds);

        SingleComposer = capi.Gui.CreateCompo("captionssettings", dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar("Captions Settings")
            .AddDynamicText("Hello, World!", CairoFont.WhiteDetailText(), textBounds, "Captions Settings")
            .Compose();
    }
    
    public override double DrawOrder => 0.1999;
    public override string ToggleKeyCombinationCode => "Captions Settings";
    
    private void OnTitleBarClose()
    {
        TryClose();
    }
}