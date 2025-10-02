using Vintagestory.API.Client;

namespace ClosedCaptions;

public class CaptionsDialog : HudElement
{
    private CaptionsModSystem captionsMod;
    public CaptionsList captionsList;
    
    public CaptionsDialog(ICoreClientAPI capi, CaptionsModSystem captionsMod) : base(capi) {
        this.captionsMod = captionsMod;
        
        ElementBounds dialogBounds = ElementBounds.FixedSize(300, 450).WithAlignment(EnumDialogArea.RightBottom).WithFixedPadding(50);
        SingleComposer = capi.Gui.CreateCompo("captions", dialogBounds);
        
        ElementBounds listBounds = ElementBounds.FixedSize(300, 450);
        dialogBounds.WithChild(listBounds);
        captionsList = new CaptionsList(SingleComposer.Api, listBounds);
        SingleComposer.AddInteractiveElement(captionsList, "captionsList");
        
        SingleComposer.Compose(false);
    }
    
    public override string ToggleKeyCombinationCode => null;
    
    public override bool OnEscapePressed() {
        return base.OnEscapePressed();
    }
}