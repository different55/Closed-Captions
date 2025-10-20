using Vintagestory.API.Client;
using Cairo;
using Vintagestory.API.MathTools;
using System;
using System.Collections.Generic;
using Vintagestory.API.Config;
using System.Reflection;
using Vintagestory.API.Common;

namespace ClosedCaptions;

public class CaptionsList : GuiElement
{
    private LoadedTexture textTexture;
    private ImageSurface imageSurface;
    private Context ctx;
    
    private TextDrawUtil textUtil;
    private CairoFont font;
    
    private CaptionsConfig cfg => CaptionsModSystem.config;
    
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
    public Caption[] captions;
    private double textHeight; 
    
    public CaptionsList(ICoreClientAPI capi, ElementBounds bounds) : base(capi, bounds) {
        textTexture = new LoadedTexture(capi);
        imageSurface = new ImageSurface(
            Format.Argb32,
            (int)Math.Round(cfg.Width*cfg.UIScale+2),
            (int)Math.Round(cfg.Height*cfg.MaxCaptions*cfg.UIScale+2)
            );
        ctx = genContext(imageSurface);
        
        font = CairoFont.WhiteMediumText().WithFont(cfg.Font).WithFontSize(cfg.FontSize);
        textUtil = new TextDrawUtil();
        font.SetupContext(CairoFont.FontMeasuringContext);
        textHeight = CairoFont.FontMeasuringContext.TextExtents("TEST").Height;
        
        captions = new Caption[cfg.MaxCaptions];
        for (int i = 0; i < cfg.MaxCaptions; i++) { captions[i] = new Caption(); }

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
        for (var i = 0; i < cfg.MaxCaptions; i++)
        {
            // Early out if we hit the end of the active sounds.
            if (!captions[i].active) break;
            
            captions[i].age += deltaTime;
            
            // Early out if this sound is still young.
            if (captions[i].age < cfg.FadeDuration) continue;

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
            if (!sound.IsPlaying) continue;
            ProcessSound(sound.Params);
        }
    }

    private void RemoveSound(int index)
    {
        for (var j = index; j < cfg.MaxCaptions - 1; j++)
            captions[j] = captions[j + 1];
        captions[cfg.MaxCaptions - 1] = new Caption();
    }
    
    private void DrawCaptions(Context ctx)
    {
        font.SetupContext(ctx);
        
        ctx.SetSourceRGB(0, 0, 0);
        ctx.Operator = Operator.Clear;
        ctx.Paint();
        ctx.Operator = Operator.Over;
        
        double y = cfg.Height * cfg.MaxCaptions;
        var playerPos = api.World.Player.Entity.Pos;
        
        var midHeight = cfg.Height / 2;
        var midWidth = cfg.Width / 2;
        var arrowHeight = (int)cfg.Height - 8;
        var arrowWidth = (int)cfg.Height/2 - 2;
        
        foreach (var sound in captions)
        {
            if (!sound.active) continue;
            y -= cfg.Height;
        
            var brightness = ((1 - (sound.age / cfg.FadeDuration)) * Math.Max(1, sound.volume) / 2 + 0.5);
            
            ctx.SetSourceRGBA(0, 0, 0, 0.25 + (brightness * 0.5));
            ctx.Rectangle(2, y+1, cfg.Width-2, cfg.Height-2);
            ctx.Fill();
            ctx.SetSourceRGBA(.25, .25, .25, 0.5 + (brightness * 0.5));
            ctx.LineWidth = 1.0;

            var soundName = sound.name;
            if (soundName.StartsWith("?"))
                soundName = soundName.Substring(1);
            
            if (soundName.StartsWith("!"))
            {
                soundName = soundName.Substring(1);
                ctx.SetSourceRGB(brightness, brightness * 0.635, brightness * .27);
                ctx.Rectangle(1, y, cfg.Width, cfg.Height);
                ctx.Stroke();
            }
            else if (soundName.StartsWith("+"))
            {
                soundName = soundName.Substring(1);
                ctx.SetSourceRGB(brightness * 0.3, brightness, brightness*0.8);
                ctx.Rectangle(1, y, cfg.Width, cfg.Height);
                ctx.Stroke();
            }
            else
            {
                ctx.SetSourceRGB(brightness, brightness, brightness);
            }
            textUtil.DrawTextLine(ctx, font, soundName, cfg.Width/2 - sound.textWidth/2, y+(cfg.Height-textHeight)/2 - 1);
            
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
                ctx.NewPath();
                ctx.Arc(1+cfg.Width*.1-2*cfg.UIScale, y+midHeight, 4*cfg.UIScale, 0, GameMath.TWOPI);
                ctx.Fill();
                ctx.NewPath();
                ctx.Arc(1+cfg.Width*.9+2*cfg.UIScale, y+midHeight, 4*cfg.UIScale, 0, GameMath.TWOPI);
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
                DrawTriangle(ctx, 1+Math.Round(cfg.Width*.1-arrowWidth*.2), y+midHeight, -arrowWidth, arrowHeight);
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
        for (var i = 1; i < cfg.MaxCaptions; i++)
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