using Vintagestory.API.Client;
using Cairo;
using Vintagestory.API.MathTools;
using System;

namespace ClosedCaptions;

public class CaptionsList : GuiElement
{
    private LoadedTexture textTexture;
    private TextDrawUtil textUtil;
    private CairoFont font;

    private static readonly int MAX_SOUNDS = 10;
    private static readonly int MAX_AGE_SECONDS = 4;
    public class Sound {
        public double age = -1;
        public string name;
        public double textWidth;
        public double yaw;
        public double volume;

        public bool active
        {
            get => age >= 0;
            set => age = (value) ? 0 : -1;
        }
    }
    public Sound[] soundList = new Sound[MAX_SOUNDS];
    
    public CaptionsList(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        textTexture = new LoadedTexture(capi);
        font = CairoFont.WhiteMediumText().WithFont("Lora").WithFontSize(28);
        textUtil = new TextDrawUtil();
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
        ImageSurface imageSurface = new ImageSurface(Format.Argb32, 300, 320);
        Context context = genContext(imageSurface);
        DrawText(context);
        generateTexture(imageSurface, ref textTexture);
        context.Dispose();
        imageSurface.Dispose();
    }
    
    private void Update(float deltaTime) {
        for (var i = 0; i < MAX_SOUNDS; i++)
        {
            // Early out if we hit the end of the active sounds.
            if (!soundList[i].active) break;
            
            soundList[i].age += deltaTime;
            
            // Early out if this sound is still young.
            if (soundList[i].age < MAX_AGE_SECONDS) continue;

            RemoveSound(i);
        }
    }

    private void RemoveSound(int index)
    {
        for (var j = index; j < MAX_SOUNDS - 1; j++)
            soundList[j] = soundList[j + 1];
        soundList[MAX_SOUNDS - 1] = new Sound();
    }
    
    private void DrawText(Context ctx)
    {
        font.SetupContext(ctx);
        //ctx.SetSourceRGBA(0, .1, .5, 0.75);
        //ctx.Rectangle(0, 0, 300, 450);
        //ctx.Fill();
        
        double y = 32 * MAX_SOUNDS;
        foreach (var sound in soundList)
        {
            y -= 32;
            if (!sound.active) continue;
        
            var brightness = ((1 - (sound.age / MAX_AGE_SECONDS)) * Math.Max(1, sound.volume) / 2 + 0.5);
            
            ctx.SetSourceRGBA(0, 0, 0, 0.25 + (brightness * 0.5));
            ctx.Rectangle(0, y, 300, 32);
            ctx.Fill();
            ctx.Rectangle(0, y, 300, 32);
            ctx.SetSourceRGBA(.25, .25, .25, 0.5 + (brightness * 0.5));
            ctx.LineWidth = 1.0;
            ctx.Stroke();

            var soundName = sound.name;
            if (soundName.StartsWith("!"))
            {
                soundName = soundName.Substring(1);
                ctx.SetSourceRGB(brightness, brightness * 0.35, brightness * .25);
            }
            else
            {
                ctx.SetSourceRGB(brightness, brightness, brightness);
            }
            textUtil.DrawTextLine(ctx, font, soundName, 150 - sound.textWidth / 2, y+6);

            if (Double.IsNaN(sound.yaw)) continue;
            
            var direction = GameMath.Mod((sound.yaw + api.World.Player.CameraYaw) / GameMath.TWOPI * 12, 12);
            if (direction > 2 && direction < 4)
            {
                ctx.NewPath();
                ctx.MoveTo(292, y+15);
                ctx.LineTo(277, y+15 - 13);
                ctx.LineTo(277, y+15 + 13);
                ctx.LineTo(292, y+15);
                ctx.MoveTo(280, y+15);
                ctx.LineTo(265, y+15 - 13);
                ctx.LineTo(265, y+15 + 13);
                ctx.LineTo(280, y+15);
                ctx.ClosePath();
                ctx.Fill();
                //textUtil.DrawTextLine(ctx, font, ">>", 270, y+4);
            }
            else if (direction > 1 && direction < 5)
            {
                ctx.NewPath();
                ctx.MoveTo(280, y+15);
                ctx.LineTo(265, y+15 - 13);
                ctx.LineTo(265, y+15 + 13);
                ctx.LineTo(280, y+15);
                ctx.Fill();
                //textUtil.DrawTextLine(ctx, font, ">", 270, y+4);
            }
            else if (direction > 8 && direction < 10)
            {
                ctx.NewPath();
                ctx.MoveTo(13, y+15);
                ctx.LineTo(28, y+15 - 13);
                ctx.LineTo(28, y+15 + 13);
                ctx.LineTo(13, y+15);
                ctx.MoveTo(25, y+15);
                ctx.LineTo(40, y+15 - 13);
                ctx.LineTo(40, y+15 + 13);
                ctx.LineTo(25, y+15);
                ctx.ClosePath();
                ctx.Fill();
                //textUtil.DrawTextLine(ctx, font, "<<", 30, y+4);
            }
            else if (direction > 7 && direction < 11)
            {
                ctx.NewPath();
                ctx.MoveTo(25, y+15);
                ctx.LineTo(40, y+15 - 13);
                ctx.LineTo(40, y+15 + 13);
                ctx.LineTo(25, y+15);
                ctx.Fill();
                //textUtil.DrawTextLine(ctx, font, "<", 30, y+4);
            }
        }
    }
    
    public void AddSound(string name, double yaw, double volume)
    {
        // Refresh existing slot if it's already present. 
        foreach (var sound in soundList)
        {
            if (sound.active && sound.name == name)
            {
                sound.age = 0;
                sound.yaw = yaw;
                sound.volume = volume;
                return;
            }
        }
     
        // Fill an empty slot.
        foreach (var sound in soundList)
        {
            if (sound.active) continue;
            sound.age = 0;
            sound.name = name;
            font.SetupContext(CairoFont.FontMeasuringContext);
            sound.textWidth = CairoFont.FontMeasuringContext.TextExtents(sound.name).Width;
            sound.yaw = yaw;
            sound.volume = volume;
            return;
        }
        
        // Else, recycle the oldest active slot.
        int oldestSound = 0;
        for (var i = 1; i < MAX_SOUNDS; i++)
        {
            if (soundList[i].age > soundList[oldestSound].age)
            {
                oldestSound = i;
            }
        }
        RemoveSound(oldestSound);
        AddSound(name, yaw, volume);
    }
}