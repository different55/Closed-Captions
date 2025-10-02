using Vintagestory.API.Client;
using Cairo;
using Vintagestory.API.MathTools;
using System;

namespace ClosedCaptions;

public class CaptionsList : GuiElement
{
    private LoadedTexture textTexture;
    private TextDrawUtil textUtil;
    public CairoFont font;
    private ICoreClientAPI capi;

    private static readonly int MAX_SOUNDS = 15;
    private static readonly int MAX_AGE_SECONDS = 4;
    public class Sound {
        public bool active = false;
        public double age;
        public string name;
        public double textWidth;
        public double yaw;
        public double volume;
    }
    public Sound[] soundList = new Sound[MAX_SOUNDS];
    
    public CaptionsList(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        textTexture = new LoadedTexture(capi);
        font = CairoFont.WhiteMediumText();
        textUtil = new TextDrawUtil();
        this.capi = capi;
        for (int i = 0; i < MAX_SOUNDS; i++) { soundList[i] = new Sound(); }
    }
    
    public override void Dispose() {
        textTexture?.Dispose();
    }
    
    public override void ComposeElements(Context ctx, ImageSurface surface) {
        font.SetupContext(ctx);
        Bounds.CalcWorldBounds();
        Recompose();
    }
    
    public override void RenderInteractiveElements(float deltaTime) {
        Update(deltaTime);
        Recompose();
        api.Render.Render2DTexturePremultipliedAlpha(textTexture.TextureId, (int)Bounds.renderX, (int)Bounds.renderY, (int)Bounds.InnerWidth, (int)Bounds.InnerHeight);
    }
    
    public void Recompose() {
        ImageSurface imageSurface = new ImageSurface(Format.Argb32, 300, 450);
        Context context = genContext(imageSurface);
        DrawText(context);
        generateTexture(imageSurface, ref textTexture);
        context.Dispose();
        imageSurface.Dispose();
    }
    
    private void Update(float deltaTime) {
        foreach (var sound in soundList) {
            if (sound.active) {
                sound.age += deltaTime;
                if (sound.age > MAX_AGE_SECONDS) {
                    capi.Logger.Debug("[CAPTIONS] Ended: " + sound.name);
                    sound.active = false;
                }
            }
        }
    }
    
    private void DrawText(Context ctx)
    {
        font.SetupContext(ctx);
        
        ctx.SetSourceRGBA(.5, .1, .5, 0.75);
        ctx.Rectangle(0, 0, 300, 450);
        ctx.Fill();
        
        double y = 30 * MAX_SOUNDS;
        foreach (var sound in soundList)
        {
            y -= 30;
            if (!sound.active) continue;

            // TODO: Just calculate this when it's loaded.
            if (sound.textWidth == -1)
            {
                sound.textWidth = ctx.TextExtents(sound.name).Width;
            }
        
            var brightness = ((1 - (sound.age / MAX_AGE_SECONDS)) * Math.Max(1, sound.volume) / 2 + 0.5);
            
            ctx.SetSourceRGBA(0, 0, 0, 0.5 + (brightness * 0.5));
            ctx.Rectangle(0, y, 300, 30);
            ctx.Fill();

            var soundName = sound.name;
            if (soundName.StartsWith("!"))
            {
                soundName = soundName.Substring(1);
                ctx.SetSourceRGB(brightness, brightness * 0.25, brightness * .125);
            }
            else
            {
                ctx.SetSourceRGB(brightness, brightness, brightness);
            }
            textUtil.DrawTextLine(ctx, font, soundName, 150 - sound.textWidth / 2, y+2);

            if (Double.IsNaN(sound.yaw)) continue;
            
            var direction = GameMath.Mod((sound.yaw + api.World.Player.CameraYaw) / GameMath.TWOPI * 12, 12);
            if (direction > 2 && direction < 4)
            {
                textUtil.DrawTextLine(ctx, font, ">>", 270, y);
            }
            else if (direction > 1 && direction < 5)
            {
                textUtil.DrawTextLine(ctx, font, ">", 270, y);
            }
            else if (direction > 8 && direction < 10)
            {
                textUtil.DrawTextLine(ctx, font, "<<", 30, y);
            }
            else if (direction > 7 && direction < 11)
            {
                textUtil.DrawTextLine(ctx, font, "<", 30, y);
            }
        }
    }
    
    public void Add(string name, double yaw, double volume)
    {
        capi.Logger.Debug("[CAPTIONS] Started: " + name);
        Sound targetSound = null;
        foreach (var sound in soundList)
        {
            if (sound.active && sound.name == name)
            {
                targetSound = sound;
                break;
            }
        }
        if (targetSound == null)
        {
            targetSound = soundList[0];
            foreach (var sound in soundList)
            {
                if (!sound.active)
                {
                    targetSound = sound;
                    break;
                }
                if (sound.age > targetSound.age)
                {
                    targetSound = sound;
                }
            }
        }
        targetSound.active = true;
        targetSound.name = name;
        targetSound.age = 0;
        targetSound.yaw = yaw;
        targetSound.volume = volume;
        targetSound.textWidth = -1;
    }
}