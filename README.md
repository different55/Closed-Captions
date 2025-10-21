# Closed Captions

Add closed captions/subtitles to Vintage Story. Make the game playable on mute!

Based heavily on goxmeor's existing subtitles mod, but now updated for v1.21, and refined visually somewhat, sorta.

## Features

### Directionality

For sounds within a normal field of view, there will be no arrows or other indicators.

For sounds at the edge of your field of view, a single directional arrow is drawn.

For sounds to your sides, double arrows are shown.

And for sounds behind you, two dots are shown on either side of the sound description.

### Colorfulness

Sounds are also color coded. "Dangerous" sounds that might alert you to a dangerous situation are highlighted in orange. A small handful of positive sound effects are highlighted in blue, like the "projectile hit" tinkle or lore unlocks.

### Configurability

This mod has a JSON config file to customize warning/notice color, position, size, style, and behavior. There are two options to hopefully increase accessibility, one to add symbols to sound names, and one to invert the colors on warning captions to increase visibility.

If you want a GUI to edit your caption settings, this mod has support for Auto Config Lib. After installing that, this mod will show up in your pause menu under Mod Settings.

## Completeness

As of v1.21.4, I believe I have mapped all vanilla sounds to have audio descriptions. If I've missed any, instead of displaying a description like "Drifter groans," it'll show up as "creature/drifter-aggro." If you see any like this for **vanilla** sound effects, please let me know.

## Compatibility

Any unsupported sound effects added by mods will show up with a placeholder/name.

I'm happy to add support for some of the more popular mods myself, but I believe that any mod can add compatibility with this mod by adding caption keys to their language file. If you have a sound at `assets/yourmodid/sounds/creature/dave-singing.ogg`, you'd add a key like this to your language file:

```json

"captions:creature/dave-singing": "Dave sings",

```

But this is entirely untested. Let me know if you try it!

If you want to add support for a sound effect to this mod, I welcome pull requests. For just adding audio descriptions, it's not too terribly technical, so don't be afraid to jump in and ask questions!

Currently, this mod has built-in support for:
 - Primitive Survival

## Limitations

Like the original mod, currently there is no way to move or resize the captions dialog. This is planned for the unknown future, but pull requests are welcome.

Currently, sounds' directions are pinned to where they start, so if the source of a sound moves while the sound is still playing, it will not update. A fix for this is planned.

Related to this, sometimes sounds that occur at your location like "Footsteps" and "Armor clatters" will occasionally flicker as showing as behind you as you walk away from your footsteps.
