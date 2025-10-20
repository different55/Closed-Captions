using Vintagestory.API.Client;

namespace ClosedCaptions;

public class CaptionsDialog : HudElement
{
    public CaptionsList captionsList;
    
    public CaptionsDialog(ICoreClientAPI capi) : base(capi) {
        ElementBounds dialogBounds = ElementBounds.FixedSize(300, CaptionsList.MAX_CAPTIONS*32).WithAlignment(EnumDialogArea.RightBottom).WithFixedPadding(16);
        SingleComposer = capi.Gui.CreateCompo("captions", dialogBounds);
        
        ElementBounds listBounds = ElementBounds.FixedSize(300, 320);
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