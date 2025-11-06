using Vintagestory.API.Client;
using Cairo;
using Vintagestory.API.MathTools;
using System;
using System.Collections.Generic;

namespace ClosedCaptions;

public class CaptionsList : GuiElement
{
    private LoadedTexture _texture;
    
    private CairoFont _font;

    private static CaptionsConfig Cfg => CaptionsSystem.Config;
    private static List<Caption> Captions => Caption.Captions;

    private bool GrowUp => 
        Cfg.Position == EnumDialogArea.LeftBottom ||
        Cfg.Position == EnumDialogArea.CenterBottom ||
        Cfg.Position == EnumDialogArea.RightBottom ||
        Cfg.Position == EnumDialogArea.FixedBottom;

    private Color _warning;
    private Color _notice;

    private TextExtents _fontMetrics;
    
    public CaptionsList(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        _texture = new LoadedTexture(capi);
        
        _font = CairoFont.WhiteMediumText().WithFont(Cfg.Font).WithFontSize(Cfg.FontSize);
        _fontMetrics = _font.GetTextExtents("Waves Crash");
        
        _warning = new Color(Cfg.WarningRed, Cfg.WarningGreen, Cfg.WarningBlue);
        _notice = new Color(Cfg.NoticeRed, Cfg.NoticeGreen, Cfg.NoticeBlue);
    }
    
    public override void Dispose()
    {
        base.Dispose();
        _texture?.Dispose();
    }
    
    public override void RenderInteractiveElements(float deltaTime) {
        Caption.SyncCaptions();
        RenderTexture();
    }
    
    private void RenderTexture() {
        var surface = new ImageSurface(Format.Argb32, Cfg.Width+2, Cfg.Height*Cfg.MaxCaptions+2);
        var ctx = genContext(surface);
        
        Bounds.CalcWorldBounds();
        RenderCaptions(ctx);
        generateTexture(surface, ref _texture);
        api.Render.Render2DTexturePremultipliedAlpha(_texture.TextureId, (int)Bounds.renderX, (int)Bounds.renderY, (int)Bounds.InnerWidth, (int)Bounds.InnerHeight);
        
        ctx.Dispose();
        surface.Dispose();
    }
    
    private void RenderCaptions(Context ctx)
    {
        _font.SetupContext(ctx);
        
        ctx.SetSourceRGB(0, 0, 0);
        ctx.Operator = Operator.Clear;
        ctx.Paint();
        ctx.Operator = Operator.Over;
        
        double y = (GrowUp) ? Cfg.Height * Cfg.MaxCaptions : -Cfg.Height;
        var playerPos = api.World.Player.Entity.Pos;
        
        var midHeight = Cfg.Height/2;
        var arrowHeight = Cfg.Height - 8;
        var arrowWidth = Cfg.Height/2 - 2;
        
        foreach (var caption in Captions)
        {
            y -= (GrowUp) ? Cfg.Height : -Cfg.Height;
        
            var brightness = ((1 - ((caption.Age - Cfg.Duration + Cfg.FadeDuration) / Cfg.FadeDuration)) * Math.Max(1, caption.Audibility) / 2 + 0.5);

            var bg = new Color(0, 0, 0, 1);
            var fg = new Color(1, 1, 1, 1);
            var stroke = new Color(0.25, 0.25, 0.25, 1);
            var bgBrightness = (Cfg.BackgroundOpacity * 0.3333) + (brightness * Cfg.BackgroundOpacity * 0.6667);
            var fgBrightness = (Cfg.TextOpacity * 0.5) + (brightness * Cfg.TextOpacity * 0.5);

            // Get display name, stripping special characters and adding indicators if ShowSymbols is enabled.
            var soundName = GetDisplayName(caption);
            
            // Modify colors for special sound types.
            if (caption.HasTag("notice"))
            {
                fg = _notice;
                stroke = _notice;
            }
            else if (caption.HasTag("warning"))
            {
                fg = _warning;
                stroke = _warning;
                if (Cfg.InvertedWarnings)
                {
                    fg = bg;
                    stroke = bg;
                    bg = _warning;
                }
            }
            
            // Set alpha values.
            bg.A = bgBrightness;
            fg.A = fgBrightness;
            stroke.A = fg.A;
            
            // Draw background
            ctx.SetSourceColor(bg);
            ctx.Rectangle(2, y+1, Cfg.Width-2, Cfg.Height-2);
            ctx.Fill();
            
            // Draw stroke
            ctx.SetSourceColor(stroke);
            ctx.Rectangle(2, y+1, Cfg.Width-2, Cfg.Height-2);
            ctx.LineWidth = 1.0;
            ctx.Stroke();
            
            // Draw text
            ctx.SetSourceColor(fg);
            var textWidth = _font.GetTextExtents(soundName).Width;
            ctx.MoveTo(Cfg.Width/2.0 - (textWidth/2), y + midHeight + (_fontMetrics.Height/2 - (_fontMetrics.Height + _fontMetrics.YBearing)));
            ctx.ShowText(soundName);
            
            // Skip drawing arrows for positionless sounds.
            if (caption.Position == null || caption.Position.IsZero) continue;
            
            var dist = caption.Position.DistanceTo(playerPos.XYZFloat);
            var yaw = Math.Atan2(caption.Position.Z - playerPos.Z, caption.Position.X - playerPos.X);
            
            // Skip drawing arrows for sounds that are too close.
            if (dist < 1.5) continue;
            
            // 0 is directly in front of the player
            // ±4 is directly left/right of the player, respectively
            // ±8 is directly behind the player
            var direction = GameMath.Mod((yaw + api.World.Player.CameraYaw) / GameMath.TWOPI * 16 + 4, 16) - 8;

            switch (direction)
            {
                // BEHIND YOU
                case > 6:
                case < -6:
                    ctx.NewPath();
                    ctx.Arc(1+Cfg.Width*.1, y+midHeight, Cfg.Height/8.0, 0, GameMath.TWOPI);
                    ctx.Fill();
                    ctx.NewPath();
                    ctx.Arc(1+Cfg.Width*.9, y+midHeight, Cfg.Height/8.0, 0, GameMath.TWOPI);
                    ctx.Fill();
                    break;
                // >>
                case > 3:
                    RenderTriangle(ctx, 1+Math.Round(Cfg.Width*.9+arrowWidth), y+midHeight, arrowWidth, arrowHeight);
                    RenderTriangle(ctx, 1+Math.Round(Cfg.Width*.9+arrowWidth*.2), y+midHeight, arrowWidth, arrowHeight);
                    break;
                // >
                case > 1:
                    RenderTriangle(ctx, 1+Math.Round(Cfg.Width*.9+arrowWidth*.2), y+midHeight, arrowWidth, arrowHeight);
                    break;
                // <<
                case < -3:
                    RenderTriangle(ctx, 1+Math.Round(Cfg.Width*.1-arrowWidth), y+midHeight, -arrowWidth, arrowHeight);
                    RenderTriangle(ctx, 1+Math.Round(Cfg.Width*.1-arrowWidth*.2), y+midHeight, -arrowWidth, arrowHeight);
                    break;
                // <
                case < -1:
                    RenderTriangle(ctx, 1+Math.Round(Cfg.Width*.1-arrowWidth*.55), y+midHeight, -arrowWidth, arrowHeight);
                    break;
            }
        }
    }

    private static string GetDisplayName(Caption caption)
    {
        if (!Cfg.ShowSymbols) return caption.Name;
        if (caption.HasTag("warning")) return "! " + caption.Name + " !";
        if (caption.HasTag("notice")) return "+ " + caption.Name + " +";
        return caption.Name;
    }

    private static void RenderTriangle(Context ctx, double x, double y, double w, double h)
    {
        ctx.NewPath();
        ctx.MoveTo(x, y);
        ctx.LineTo(x - w, y - h/2);
        ctx.LineTo(x - w, y + h/2);
        ctx.LineTo(x, y);
        ctx.Fill();
    }
}