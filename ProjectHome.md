CrystalBoy is a GameBoy emulator written in C#.

This project was started in August 2008 for a lot off different reasons.
One of these reasons was to challenge C# and see whether an emulator could be successfully run. And as you can see, it works pretty well. (Though not as fast as a native one. But some optimizations are still possible)
Surprinsingly, it didn't take very long to have this running correctly (looking at my archives, it took something like 1 month), but some features are missing, as I turned to other projects.

Features:
  * Emulated Hardware:
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
  * Video renderers:
    * GDI+ (Slow)
    * Using Managed DirectX: (Only for 32 bit Windows…)
      * Direct3D
    * Using SlimDX (SlimDX March 2011 (for .NET 2.0) installation required)
      * Direct3D 9
      * Direct2D (For Windows 7)

Amongst other things, the emulator still lacks audio, and I'll definitely take the time to add it someday. (Like, before I die)
Most obvious bugs should have been corrected, but there are still some out there…


The SVN version has correct multithreading, and quite good SGB (mainly border) emulation, which will come in the next release.

For building the code, Visual Studio 2010 is required. You may have some success with SharpDevelop and MonoDevelop, but those were not tested.
Even if the resulting assemblies run on .NET 2.0, a C# 3.0+ compiler is needed. I will try to keep the .NET 2.0 compatibility as long as possible, but it's quite certain it will be dropped someday.