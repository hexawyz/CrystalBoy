CrystalBoy Game Boy Emulator
----------------------------

DISCLAIMER:

This software is provided as-is, use it at your own risk.
The author shall not be held responsible for any bus or data losses caused by use of the software.

Préréquisites:
	* Computer with Windows XP+
	* .NET Framework 2.0 or better
	* For 32 bit systems, if you want to use the Managed DirectX (MDX) render method:
		* Up-to date DirectX version
		* If the program crashes or throws an error during start-up, you need to make sure you have the latest version of DirectX, by downloading the installer on MS website.
	* For 64 bit systems, if you want hardware accelerated render methods using SlimDX:
		* Up-to date DirectX version
		* SlimDX end user version from March 2011

Since the Direct2D render method has been quite enhanced compared to others, I suggest you use it *if you have Windows 7*.

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
	* Various chanegs in emulation for hardware type switching. (Quite logically, changing hardware now requires the emulation to be reset)