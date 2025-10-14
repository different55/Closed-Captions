using Vintagestory.API.Client;
using Cairo;
using Vintagestory.API.MathTools;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace ClosedCaptions;

public class CaptionsList : GuiElement
{
    private LoadedTexture textTexture;
    private TextDrawUtil textUtil;
    private CairoFont font;

    private static readonly int MAX_SOUNDS = 10;
    private static readonly int MAX_AGE_SECONDS = 4;
    private static readonly double RANGE_THRESHOLD = 0.8;
    
    public class Sound {
        public double age = -1;
        public string name;
        public double textWidth;
        public Vec3f position;
        public double volume;

        public bool active
        {
            get => age >= 0;
            set => age = (value) ? 0 : -1;
        }
    }
    public Sound[] sounds = new Sound[MAX_SOUNDS];
    
    public CaptionsList(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        textTexture = new LoadedTexture(capi);
        font = CairoFont.WhiteMediumText().WithFont("Lora").WithFontSize(28);
        textUtil = new TextDrawUtil();
        for (int i = 0; i < MAX_SOUNDS; i++) { sounds[i] = new Sound(); }
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
        DrawCaptions(context);
        generateTexture(imageSurface, ref textTexture);
        context.Dispose();
        imageSurface.Dispose();
    }
    
    private void Update(float deltaTime) {
        for (var i = 0; i < MAX_SOUNDS; i++)
        {
            // Early out if we hit the end of the active sounds.
            if (!sounds[i].active) break;
            
            sounds[i].age += deltaTime;
            
            // Early out if this sound is still young.
            if (sounds[i].age < MAX_AGE_SECONDS) continue;

            RemoveSound(i);
        }
    }

    private void RemoveSound(int index)
    {
        for (var j = index; j < MAX_SOUNDS - 1; j++)
            sounds[j] = sounds[j + 1];
        sounds[MAX_SOUNDS - 1] = new Sound();
    }
    
    private void DrawCaptions(Context ctx)
    {
        font.SetupContext(ctx);
        //ctx.SetSourceRGBA(0, .1, .5, 0.75);
        //ctx.Rectangle(0, 0, 300, 320);
        //ctx.Fill();
        
        double y = 32 * MAX_SOUNDS;
        var playerPos = api.World.Player.Entity.Pos;
        
        foreach (var sound in sounds)
        {
            if (!sound.active) continue;
            y -= 32;
        
            var brightness = ((1 - (sound.age / MAX_AGE_SECONDS)) * Math.Max(1, sound.volume) / 2 + 0.5);
            
            ctx.SetSourceRGBA(0, 0, 0, 0.25 + (brightness * 0.5));
            ctx.Rectangle(1, y+1, 298, 30);
            ctx.Fill();
            //ctx.Rectangle(0, y, 300, 32);
            ctx.SetSourceRGBA(.25, .25, .25, 0.5 + (brightness * 0.5));
            ctx.LineWidth = 1.0;
            //ctx.Stroke();

            var soundName = sound.name;
            if (soundName.StartsWith("?"))
                soundName = soundName.Substring(1);
            
            if (soundName.StartsWith("!"))
            {
                soundName = soundName.Substring(1);
                ctx.SetSourceRGB(brightness, brightness * 0.635, brightness * .27);
                ctx.Rectangle(0, y, 300, 32);
                ctx.Stroke();
            }
            else if (soundName.StartsWith("~"))
            {
                soundName = soundName.Substring(1);
                ctx.SetSourceRGB(brightness * 0.3, brightness, brightness*0.8);
                ctx.Rectangle(0, y, 300, 32);
                ctx.Stroke();
            }
            else
            {
                ctx.SetSourceRGB(brightness, brightness, brightness);
            }
            textUtil.DrawTextLine(ctx, font, soundName, 150 - sound.textWidth / 2, y+3);
            
            if (sound.position == null || sound.position.IsZero) continue;
            
            var dist = sound.position.DistanceTo(playerPos.XYZFloat);
            var yaw = Math.Atan2(sound.position.Z - playerPos.Z, sound.position.X - playerPos.X);
            
            if (dist < 2) continue;
            
            // 0 is directly in front of the player
            // ±4 is directly left/right of the player, respectively
            // ±8 is directly behind the player
            var direction = GameMath.Mod((yaw + api.World.Player.CameraYaw) / GameMath.TWOPI * 16 + 4, 16) - 8;
            
            api.Logger.Debug("[Captions] Sound: " + sound.name + " direction: " + direction);

            // BEHIND YOU
            if (Math.Abs(direction) > 6)
            {
                // d=r*4*(sqrt(2)-1)/3
                ctx.NewPath();
                ctx.Arc(26, y+16, 4, 0, GameMath.TWOPI);
                ctx.Fill();
                ctx.NewPath();
                ctx.Arc(274, y+16, 4, 0, GameMath.TWOPI);
                ctx.Fill();
            }
            // >>
            else if (direction > 3)
            {
                DrawTriangle(ctx, 292, y + 16, 15, 26);
                DrawTriangle(ctx, 280, y + 16, 15, 26);
                //textUtil.DrawTextLine(ctx, font, ">>", 270, y+4);
            }
            // >
            else if (direction > 1)
            {
                DrawTriangle(ctx, 280, y + 16, 15, 26);
                //textUtil.DrawTextLine(ctx, font, ">", 270, y+4);
            }
            // <<
            else if (direction < -3)
            {
                DrawTriangle(ctx, 25, y + 16, -15, 26);
                DrawTriangle(ctx, 13, y + 16, -15, 26);
                //textUtil.DrawTextLine(ctx, font, "<<", 30, y+4);
            }
            // <
            else if (direction < -1)
            {
                DrawTriangle(ctx, 25, y + 16, -15, 26);
                //textUtil.DrawTextLine(ctx, font, "<", 30, y+4);
            }
        }
    }

    public void DrawTriangle(Context ctx, double x, double y, double w, double h)
    {
        ctx.NewPath();
        ctx.MoveTo(x, y);
        ctx.LineTo(x - w, y - h/2);
        ctx.LineTo(x - w, y + h/2);
        ctx.LineTo(x, y);
        ctx.Fill();
    }

    public void ProcessSound(SoundParams sound)
    {
        api.Logger.Debug("[Captions] Sound: " + sound.Location.Path);
        // Unknown condition yoinked without understanding from SubtitlesMod
        var player = api.World.Player;
        if (player == null) return;
        
        // Ignore music
        if (sound.SoundType == EnumSoundType.Music) return;
        
        // Calculate sound ID
        var path = sound.Location.Path;
        var id = path.StartsWith("sounds/") && path.EndsWith(".ogg") ? path.Substring(7, path.Length - 7 - 4) : path;
        // Strip trailing digit
        var lastChar = id.ToCharArray(id.Length - 1, 1)[0];
        if (lastChar >= '0' && lastChar <= '9')
            id = id.Substring(0, id.Length - 1);
        
        var name = Lang.GetIfExists("captions:" + id);
        // Ignore specified sounds
        if (name == "") return;
        // Unnamed sounds
        if (name == null) name = id;
        
        api.Logger.Debug("[Captions] Sound name: " + name);
        
        var position = sound.Position;
        if (position == null || position.IsZero)
        {
            AddSound(name, null, sound.Volume);
            return;
        }

        var playerPos = player.Entity.Pos.AsBlockPos;
        var dx = sound.Position.X - playerPos.X;
        var dy = sound.Position.Y - playerPos.Y;
        var dz = sound.Position.Z - playerPos.Z;
        var dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        
        // Ignore sounds that are out of range.
        if (dist > sound.Range * RANGE_THRESHOLD) return;
        
        api.Logger.Debug("[Captions] Blessed: " + name);
        
        AddSound(name, sound.Position, sound.Volume);
        foreach (var s in sounds)
        {
            if (!s.active) continue;
            api.Logger.Debug($"[Captions] Active: {s.name} age={s.age:0.00}s");
        }
    }
    
    public void AddSound(string name, Vec3f position, double volume)
    {
        // Refresh existing slot if it's already present. 
        foreach (var sound in sounds)
        {
            if (sound.active && sound.name == name)
            {
                api.Logger.Debug("[Captions] Refreshed: " + name);
                sound.age = 0;
                sound.position = position;
                sound.volume = volume;
                return;
            }
        }
     
        // Fill an empty slot.
        foreach (var sound in sounds)
        {
            if (sound.active) continue;
            api.Logger.Debug("[Captions] Added: " + name);
            sound.age = 0;
            sound.name = name;
            font.SetupContext(CairoFont.FontMeasuringContext);
            sound.textWidth = CairoFont.FontMeasuringContext.TextExtents(sound.name).Width;
            sound.position = position;
            sound.volume = volume;
            return;
        }
        
        // Else, recycle the oldest active slot.
        int oldestSound = 0;
        for (var i = 1; i < MAX_SOUNDS; i++)
        {
            if (sounds[i].age > sounds[oldestSound].age)
            {
                oldestSound = i;
            }
        }
        api.Logger.Debug("[Captions] Recycled: " + sounds[oldestSound].name + " -> " + name);
        RemoveSound(oldestSound);
        AddSound(name, position, volume);
    }
}