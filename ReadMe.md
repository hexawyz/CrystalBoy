CrystalBoy Game Boy Emulator
============================

CrystalBoy is a GameBoy emulator written in C#.

This project was started in August 2008 for a lot off different reasons.
One of these reasons was to challenge C# and see whether an emulator could be successfully run.
And as you can see, it works fairly well. (Though not as fast as a native one. But some optimizations are still possible.)

## Features:

  * Emulated Hardware
    * Game Boy B&W
    * Super Game Boy (Partial, no SGB screen coloring for now)
    * Game Boy Color
    * Game Boy Advance in Game Boy Color Mode
  * Windows Forms UI
    * Graphical debugger
    * Map Viewer
    * Emulated hardware can be changed in the menus.
    * Saves RAM and RTC data to .sav files, like other emulators.
    * fr-FR localization
  * Video renderers
    * GDI+ (Slow)
    * SharpDX / Direct2D (Not reimplemented yet)

Amongst other things, the emulator still lacks audio, but this feature is still planned.
Most obvious bugs should have been corrected, but there are still some out there…

## Disclaimer:

This software is provided as-is, use it at your own risk.
The author shall not be held responsible for any bus or data losses caused by use of the software.

## Prerequisites:

  * A recent version of Windows (ideally at least Windows 7)
  * .NET Framework 4.5.1 or better
  * Up-to date DirectX version

## Changelog

### Version 1.6

  * New rendering infrastructure making a better use of threads.
  * More accurate clock emulation (The target is 60 FPS for now, not 59.7FPS like on regular GB)
  * The status bar now displays the speed as a % of the target speed instead of virtual FPS.
  * The GDI+ renderer is embedded into the main binary.
  * The GDI+ renderer is now able to display the border (still a WIP, as this is subjet to frameskipping)
  * The emulator is high DPI aware.


### Version 1.5

  * Super Game Boy emulation
    * Border support


### Version 1.4

  * Now supports loading the DMG, SGB and GBC bootstrap ROMs
  * Better GBC hardware emulation (now supports switching to fake B&W mode)
    * Using GBC bootstrap ROM allows to play B&W games with automatic color palette
    * The GBC automatic palettes are also emulated without the bootstrap ROM
    * The OAM and VRAM are initialized more accurately
  * MBC2 support (maybe not totally accurate, but certainly good enough)
  * Instant FPS calculation… Should be instantaneously more accurate, but much more variable than the method used previously.
  * Multithreaded emulation. (Should only improve uncapped FPS for now)
    * I did not test extensively with all the rendering plugins, but things should be ok as the UI thread and and rendering thread never execute at the same time.
    * Please send a bug report if you encunter a bug related to multithreaded rendering…
  * LCD and Timer timings emulation redone 95%… 
    * Compatibility increased
    * Wario Land II startup screen now shows correctly
    * Link's Awakening's bug seems to be gone
    * Brain Bender's startup screen now render correctly, like it did a [very] very long time ago.
  * Fixed the sprite priority emulation again… Pokémon Crystal intro should play fine.
  * Fixed small - but sometimes nasty - bugs
    * Now able to run Mental Respirator by Phantasy (The Gin & Tonic trick is not emulated yet)
    * Most demos are able to start or even run correctly. (Demotronic still doesn't, but it helped correct one of the nasty bugs ;)


### Version 1.3

  * More accurate CPU emulation (according to the tests, all instructions now compute 100% exact… Only HALT and STOP may need reworking)
  * Direct2D render method with SlimDX (March 2011)
  * More accurate graphics emulation (dynamic color palette changes, correct sprite priority emulation)
  * "Perfect" RTC emulation
  * Bugfixes in Mapper emulation (generic mapper bugfixes and some MBCx specific ones)
  * Enhanced various areas dealing with game information
  * Added support for saving the RAM for games with battery (finally !)
  * Saving the RAM also saves the RTC information for games using the RTC
  * UI is now localizable
    * Added fr-FR localization
  * Better detection of invalid plugins, which should prevent crashes.
  * Hardware Type can now be changed using UI (Menu items previously did nothing…)
  * Various changes in emulation for hardware type switching. (Quite logically, changing hardware now requires the emulation to be reset)

