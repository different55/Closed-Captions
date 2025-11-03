using Vintagestory.API.Client;

namespace ClosedCaptions;

public class CaptionsDialog : HudElement
{
    public CaptionsList CaptionsList;
    
    public CaptionsDialog(ICoreClientAPI capi) : base(capi)
    {
        var cfg = CaptionsSystem.Config;
        var dialogBounds = ElementBounds.FixedSize(
            cfg.Width+2,
            cfg.Height*cfg.MaxCaptions+2
            )
            .WithAlignment(cfg.Position)
            .WithFixedPadding(cfg.Padding);
        SingleComposer = capi.Gui.CreateCompo("captions", dialogBounds);
        
        var listBounds = ElementBounds.FixedSize(
            cfg.Width+2,
            cfg.Height*cfg.MaxCaptions+2
            );
        dialogBounds.WithChild(listBounds);
        CaptionsList = new CaptionsList(SingleComposer.Api, listBounds);
        SingleComposer.AddInteractiveElement(CaptionsList, "captionsList");
        
        SingleComposer.Compose(false);
    }
    
    public override string ToggleKeyCombinationCode => null;
    public override double DrawOrder => 0.1999;
    public override bool Focusable => false;
    public override bool ShouldReceiveMouseEvents() => false;
    public override bool ShouldReceiveKeyboardEvents() => false;
    public override bool OnEscapePressed() => false;
}