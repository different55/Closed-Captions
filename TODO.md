* Have brightness dim slightly immediately, so that sound effects in one channel in rapid succession are noticeable.
* Make some sounds group by location.
* Experiment with smooth transitions between directional states.
* Custom settings GUI.
  * GUI should warn users if current font settings may not fit inside their current captionbox settings. 
* Rename "audibility"
* Improve font sizing.
* Check on the hooved animals, are they passive or hostile?
* Swap "rain patters" and "rain pours". Maybe change one to rain splashes?
*** Add list of sounds to each caption.
* Add normalization for sound levels.
* Add different methods to consolidate channel sounds (loudest, closest, priority).
* Make size of icons configurable.
* Currently, throttled sounds are suppressed entirely. Maybe higher priorities can Refresh.
* Make hostile and notice tags override throttling.
* Add an "all" pseudo-tag that affects all sounds.
* Redo the AddCaption status checks - right now the priority check is permanently true because of the first chunk of SyncCaptions. This among other things would be fixed by stashing a list of all currently active sounds inside each caption.
* Just feels odd to have tracking hanging out down in Caption.cs as a bunch of static fields. Maybe this should be part of the System?