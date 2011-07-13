CrystalBoy Game Boy Emulator
----------------------------

DISCLAIMER:

This software is provided as-is, use it at your own risk.
The author shall not be held responsible for any bus or data losses caused by use of the software.

Prerequisites:
	* Computer with Windows XP+
	* .NET Framework 2.0 or better
	* For 32 bit systems, if you want to use the Managed DirectX (MDX) render method:
		* Up-to date DirectX version
		* If the program crashes or throws an error during start-up, you need to make sure you have the latest version of DirectX, by downloading the installer on MS website.
	* For 64 bit systems, if you want hardware accelerated render methods using SlimDX: (.NET 2.0)
		* Up-to date DirectX version
		* SlimDX end user version from March 2011 (.NET 2.0)

Useful links:
.NET Framework 4.0 Web Installer: http://www.microsoft.com/downloads/en/details.aspx?FamilyID=9cfb2d51-5ff4-4491-b0e5-b386f32c0992&displaylang=en
SlimDX March 2011 End-User Runtime (.NET 2.0): http://slimdx.googlecode.com/files/SlimDX%20Runtime%20for%20.NET%202.0%20%28March%202011%29.msi
DirectX Web Installer: http://www.microsoft.com/download/en/details.aspx?id=35

Depending on your configuration you may need to install some of the packages linked above.
On old Windows XP systems, DirectX Web Setup needs to be run in order to install the Managed Direct X assemblies.
NOTE 1: Please do not send a bug report if you did not check that all the prerequisites are installed.
NOTE 2: At first run, the emulator may inform you that one or more plugin assemblies have been removed from the configuration file… This is not a bug.

Since the Direct2D render method has been quite enhanced compared to others, I suggest you use it *if you have Windows 7*.
However, the performance with Direct2D might be slightly lower than with Direct3D 9, although I haven't been able to really measure anything.

Changes for version 1.4
	* Now supports loading the DMG, SGB and GBC bootstrap ROMs
	* Better GBC hardware emulation (now supports switching to fake B&W mode)
		- Using GBC bootstrap ROM allows to play B&W games with automatic color palette
		- The GBC automatic palettes are also emulated without the bootstrap ROM
		- The OAM and VRAM are initialized more accurately
	* MBC2 support (maybe not totally accurate, but certainly good enough)
	* Instant FPS calculation… Should be instantaneously more accurate, but much more variable than the method used previously.
	* Multithreaded emulation. (Should only improve uncapped FPS for now)
		- I did not test extensively with all the rendering plugins, but things should be ok as the UI thread and and rendering thread never execute at the same time.
		- Please send a bug report if you encunter a bug related to multithreaded rendering…
	* LCD and Timer timings emulation redone 95%… 
		- Compatibility increased
		- Wario Land II startup screen now shows correctly
		- Link's Awakening's bug seems to be gone
		- Brain Bender's startup screen now render correctly, like it did a [very] very long time ago.
	* Fixed the sprite priority emulation again… Pokémon Crystal intro should play fine.
	* Fixed small - but sometimes nasty - bugs
		- Now able to run Mental Respirator by Phantasy (The Gin & Tonic trick is not emulated yet)
		- Most demos are able to start or even run correctly. (Demotronic still doesn't, but it helped correct one of the nasty bugs ;)

Changes for version 1.3
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