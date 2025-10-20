using Vintagestory.API.Client;
using Cairo;
using Vintagestory.API.MathTools;
using System;
using System.Collections.Generic;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;
using System.Reflection;

namespace ClosedCaptions;

public class CaptionsList : GuiElement
{
    private LoadedTexture textTexture;
    private ImageSurface imageSurface;
    private Context ctx;
    
    private TextDrawUtil textUtil;
    private CairoFont font;
    
    public static readonly int MAX_CAPTIONS = 10;
    private static readonly int FADE_TIME = 4;
    
    public class Caption {
        public double age = -1;
        public string name;
        public double textWidth;
        public Vec3f position;
        public double volume;
        public int activeSounds;

        public bool active
        {
            get => age >= 0;
            set => age = (value) ? 0 : -1;
        }
    }

    private Queue<ILoadedSound> ActiveSounds;
    public Caption[] captions = new Caption[MAX_CAPTIONS];
    
    public CaptionsList(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        textTexture = new LoadedTexture(capi);
        // Normalize font size to screen height so GUI looks consistent on all resolutions
        font = CairoFont.WhiteMediumText().WithFont("Lora").WithFontSize(28f / capi.Render.FrameHeight);
        imageSurface = new ImageSurface(Format.Argb32, 300, 320);
        ctx = genContext(imageSurface);
        textUtil = new TextDrawUtil();
        
        for (int i = 0; i < MAX_CAPTIONS; i++) { captions[i] = new Caption(); }

        FieldInfo field = api.World.GetType().GetField("ActiveSounds", BindingFlags.NonPublic | BindingFlags.Instance);
        ActiveSounds = (Queue<ILoadedSound>)field.GetValue(api.World);
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
        Update(deltaTime);
        DrawCaptions(ctx);
        generateTexture(imageSurface, ref textTexture);
        api.Render.Render2DTexturePremultipliedAlpha(textTexture.TextureId, (int)Bounds.renderX, (int)Bounds.renderY, (int)Bounds.InnerWidth, (int)Bounds.InnerHeight);
    }
    private void Update(float deltaTime)
    {
        SyncCaptions();
        for (var i = 0; i < MAX_CAPTIONS; i++)
        {
            // Early out if we hit the end of the active sounds.
            if (!captions[i].active) break;
            
            captions[i].age += deltaTime;
            
            // Early out if this sound is still young.
            if (captions[i].age < FADE_TIME) continue;

            RemoveSound(i);
        }
    }
    
    // TODO CLEAR TEXTURE BEFOREHAND.

    // Syncronizes the internal caption list with the currently playing ActiveSounds.
    private void SyncCaptions()
    {
        // Reset the activeSound count.
        foreach (var caption in captions)
            caption.activeSounds = 0;

        foreach (var sound in ActiveSounds)
        {
            ProcessSound(sound.Params);
        }
    }

    private void RemoveSound(int index)
    {
        for (var j = index; j < MAX_CAPTIONS - 1; j++)
            captions[j] = captions[j + 1];
        captions[MAX_CAPTIONS - 1] = new Caption();
    }
    
    private void DrawCaptions(Context ctx)
    {
        font.SetupContext(ctx);
        //ctx.SetSourceRGBA(0, .1, .5, 0.75);
        //ctx.Rectangle(0, 0, 300, 320);
        //ctx.Fill();
        ctx.SetSourceRGB(0, 0, 0);
        ctx.Operator = Operator.Clear;
        ctx.Paint();
        ctx.Operator = Operator.Over;
        
        double y = 32 * MAX_CAPTIONS;
        var playerPos = api.World.Player.Entity.Pos;
        
        foreach (var sound in captions)
        {
            if (!sound.active) continue;
            y -= 32;
        
            var brightness = ((1 - (sound.age / FADE_TIME)) * Math.Max(1, sound.volume) / 2 + 0.5);
            
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
        if (dist > sound.Range) return;
        
        api.Logger.Debug("[Captions] Blessed: " + name);
        
        AddSound(name, sound.Position, sound.Volume);
        foreach (var s in captions)
        {
            if (!s.active) continue;
            api.Logger.Debug($"[Captions] Active: {s.name} age={s.age:0.00}s");
        }
    }
    
    public void AddSound(string name, Vec3f position, double volume)
    {
        // Refresh existing slot if it's already present. 
        foreach (var sound in captions)
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
        foreach (var sound in captions)
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
        for (var i = 1; i < MAX_CAPTIONS; i++)
        {
            if (captions[i].age > captions[oldestSound].age)
            {
                oldestSound = i;
            }
        }
        api.Logger.Debug("[Captions] Recycled: " + captions[oldestSound].name + " -> " + name);
        RemoveSound(oldestSound);
        AddSound(name, position, volume);
    }
}