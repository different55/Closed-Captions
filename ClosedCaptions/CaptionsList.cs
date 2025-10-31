using Vintagestory.API.Client;
using Cairo;
using Vintagestory.API.MathTools;
using System;
using System.Collections.Generic;

namespace ClosedCaptions;

public class CaptionsList : GuiElement
{
    private LoadedTexture textTexture;
    private ImageSurface imageSurface;
    private Context ctx;
    
    private CairoFont font;

    private CaptionsConfig cfg => CaptionsSystem.config;
    private List<Caption> captions => Caption.Captions;

    private bool GrowUp => 
        cfg.Position == EnumDialogArea.LeftBottom ||
        cfg.Position == EnumDialogArea.CenterBottom ||
        cfg.Position == EnumDialogArea.RightBottom ||
        cfg.Position == EnumDialogArea.FixedBottom;

    private Color warning;
    private Color notice;

    private TextExtents fontMetrics;
    
    public CaptionsList(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        textTexture = new LoadedTexture(capi);
        imageSurface = new ImageSurface(
            Format.Argb32,
            cfg.Width+2,
            cfg.Height*cfg.MaxCaptions+2
            );
        ctx = genContext(imageSurface);
        
        font = CairoFont.WhiteMediumText().WithFont(cfg.Font).WithFontSize(cfg.FontSize);
        fontMetrics = font.GetTextExtents("Waves Crash");
        
        warning = new Color(cfg.WarningRed, cfg.WarningGreen, cfg.WarningBlue);
        notice = new Color(cfg.NoticeRed, cfg.NoticeGreen, cfg.NoticeBlue);
    }
    
    public override void Dispose() {
        textTexture?.Dispose();
        imageSurface?.Dispose();
        ctx?.Dispose();
    }
    
    public override void ComposeElements(Context ctx, ImageSurface surface) {
        font.SetupContext(ctx);
        Bounds.CalcWorldBounds();
        DrawCaptions(ctx);
        generateTexture(imageSurface, ref textTexture);
    }
    
    public override void RenderInteractiveElements(float deltaTime) {
        Caption.SyncCaptions();
        DrawCaptions(ctx);
        generateTexture(imageSurface, ref textTexture);
        api.Render.Render2DTexturePremultipliedAlpha(textTexture.TextureId, (int)Bounds.renderX, (int)Bounds.renderY, (int)Bounds.InnerWidth, (int)Bounds.InnerHeight);
    }
    
    private void DrawCaptions(Context ctx)
    {
        font.SetupContext(ctx);
        
        ctx.SetSourceRGB(0, 0, 0);
        ctx.Operator = Operator.Clear;
        ctx.Paint();
        ctx.Operator = Operator.Over;
        
        double y = (GrowUp) ? cfg.Height * cfg.MaxCaptions : -cfg.Height;
        var playerPos = api.World.Player.Entity.Pos;
        
        var midHeight = cfg.Height/2;
        var arrowHeight = cfg.Height - 8;
        var arrowWidth = cfg.Height/2 - 2;
        
        foreach (var caption in captions)
        {
            y -= (GrowUp) ? cfg.Height : -cfg.Height;
        
            var brightness = ((1 - ((caption.age - cfg.Duration + cfg.FadeDuration) / cfg.FadeDuration)) * Math.Max(1, caption.volume) / 2 + 0.5);

            var bg = new Color(0, 0, 0, 1);
            var fg = new Color(1, 1, 1, 1);
            var stroke = new Color(0.25, 0.25, 0.25, 1);
            var bgBrightness = (cfg.BackgroundOpacity * 0.3333) + (brightness * cfg.BackgroundOpacity * 0.6667);
            var fgBrightness = (cfg.TextOpacity * 0.5) + (brightness * cfg.TextOpacity * 0.5);

            // Get display name, stripping special characters and adding indicators if ShowSymbols is enabled.
            var soundName = GetDisplayName(caption);
            
            // Modify colors for special sound types.
            if (caption.urgency == Caption.Urgency.Warning)
            {
                fg = warning;
                stroke = warning;
                if (cfg.InvertedWarnings)
                {
                    fg = bg;
                    stroke = bg;
                    bg = warning;
                }
            }
            else if (caption.urgency == Caption.Urgency.Notice)
            {
                fg = notice;
                stroke = notice;
            }
            
            // Set alpha values.
            bg.A = bgBrightness;
            fg.A = fgBrightness;
            stroke.A = fg.A;
            
            // Draw background
            ctx.SetSourceColor(bg);
            ctx.Rectangle(2, y+1, cfg.Width-2, cfg.Height-2);
            ctx.Fill();
            
            // Draw stroke
            ctx.SetSourceColor(stroke);
            ctx.Rectangle(2, y+1, cfg.Width-2, cfg.Height-2);
            ctx.LineWidth = 1.0;
            ctx.Stroke();
            
            // Draw text
            ctx.SetSourceColor(fg);
            var textWidth = font.GetTextExtents(soundName).Width;
            ctx.MoveTo(cfg.Width/2 - (textWidth/2), y + midHeight + (fontMetrics.Height/2 - (fontMetrics.Height + fontMetrics.YBearing)));
            ctx.ShowText(soundName);
            
            // Skip drawing arrows for positionless sounds.
            if (caption.position == null || caption.position.IsZero) continue;
            
            var dist = caption.position.DistanceTo(playerPos.XYZFloat);
            var yaw = Math.Atan2(caption.position.Z - playerPos.Z, caption.position.X - playerPos.X);
            
            // Skip drawing arrows for sounds that are too close.
            if (dist < 1.5) continue;

            // Ignore directionality for overlapping sounds.
            // TODO: Use direction of most recent sound, once I actually start storing whole sounds.
            if (caption.activeSounds > 1) continue;
            
            // 0 is directly in front of the player
            // ±4 is directly left/right of the player, respectively
            // ±8 is directly behind the player
            var direction = GameMath.Mod((yaw + api.World.Player.CameraYaw) / GameMath.TWOPI * 16 + 4, 16) - 8;
            
            // BEHIND YOU
            if (Math.Abs(direction) > 6)
            {
                ctx.NewPath();
                ctx.Arc(1+cfg.Width*.1, y+midHeight, cfg.Height/8, 0, GameMath.TWOPI);
                ctx.Fill();
                ctx.NewPath();
                ctx.Arc(1+cfg.Width*.9, y+midHeight, cfg.Height/8, 0, GameMath.TWOPI);
                ctx.Fill();
            }
            // >>
            else if (direction > 3)
            {
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.9+arrowWidth), y+midHeight, arrowWidth, arrowHeight);
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.9+arrowWidth*.2), y+midHeight, arrowWidth, arrowHeight);
            }
            // >
            else if (direction > 1)
            {
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.9+arrowWidth*.2), y+midHeight, arrowWidth, arrowHeight);
            }
            // <<
            else if (direction < -3)
            {
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.1-arrowWidth), y+midHeight, -arrowWidth, arrowHeight);
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.1-arrowWidth*.2), y+midHeight, -arrowWidth, arrowHeight);
            }
            // <
            else if (direction < -1)
            {
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.1-arrowWidth*.55), y+midHeight, -arrowWidth, arrowHeight);
            }
        }
    }

    private string GetDisplayName(Caption caption)
    {
        if (!cfg.ShowSymbols) return caption.name;
        return caption.urgency switch
        {
            Caption.Urgency.Warning => "! " + caption.name + " !",
            Caption.Urgency.Notice => "+ " + caption.name + " +",
            _ => caption.name
        };
    }

    private void DrawTriangle(Context ctx, double x, double y, double w, double h)
    {
        ctx.NewPath();
        ctx.MoveTo(x, y);
        ctx.LineTo(x - w, y - h/2);
        ctx.LineTo(x - w, y + h/2);
        ctx.LineTo(x, y);
        ctx.Fill();
    }
}