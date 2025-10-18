using HarmonyLib;
using Vintagestory.API.Client;

namespace ClosedCaptions;

public class CaptionsDialog : HudElement
{
    public CaptionsList captionsList;
    
    public CaptionsDialog(ICoreClientAPI capi) : base(capi)
    {
        var cfg = CaptionsModSystem.config;
        ElementBounds dialogBounds = ElementBounds.FixedSize(
            cfg.Width*cfg.UIScale+2,
            cfg.Height*cfg.MaxCaptions*cfg.UIScale+2
            )
            .WithAlignment(cfg.Position)
            .WithFixedPadding(cfg.Padding);
        SingleComposer = capi.Gui.CreateCompo("captions", dialogBounds);
        
        ElementBounds listBounds = ElementBounds.FixedSize(
            cfg.Width*cfg.UIScale+2,
            cfg.Height*cfg.MaxCaptions*cfg.UIScale+2
            );
        dialogBounds.WithChild(listBounds);
        captionsList = new CaptionsList(SingleComposer.Api, listBounds);
        SingleComposer.AddInteractiveElement(captionsList, "captionsList");
        
        SingleComposer.Compose(false);
    }
    
    public override string ToggleKeyCombinationCode => null;
    public override double DrawOrder => 0.1999;
    public override bool Focusable => false;

    public override bool ShouldReceiveMouseEvents() => false;

    public override bool ShouldReceiveKeyboardEvents()
    {
        return false;
    }
    
    public override bool OnEscapePressed()
    {
        return false;
    }
}