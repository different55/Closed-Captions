using System.Collections.Generic;
using Vintagestory.API.Client;

namespace ClosedCaptions;

public class Caption
{
    public static Queue<ILoadedSound> ActiveSounds;
    public static List<Caption> Captions = [];
    
    // Synchronizes the internal caption list with the currently active sounds.
    public static void SyncCaptions()
    {
        // Reset the sound references in all captions.
        foreach (var caption in Captions)
            caption.sounds = new SoundParams[];
    }
    
    public double age = 0;
    public string name;
    public List<SoundParams> sounds;
}