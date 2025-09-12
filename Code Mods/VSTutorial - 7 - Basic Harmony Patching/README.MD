# Vintage Story Example Code Mod 7 - Basic Harmony Patching

## Verified for game version 1.21.0.

## Usage
This mod is an example code mod designed to showcase using harmony patches. The appropriate wiki tutorial is not yet uploaded.

The zips folder contains a complete version and a setup version of the mod - You will need to unzip these and load the VSTutorial.sln file.
Using the setup version allows you to follow along with the wiki tutorial.

This mod is designed to work on Windows with Visual Studio. It may require some changes to work with other operating systems or IDEs.

## Version History
 - 1.21.0
   - Verified working for version 1.21.0
   - Updated to .NET 8.0.
   - Update mod name in modinfo.
 - 1.20.4
   - Verified working for version 1.20.4.
   
## Verification Process	
 - Check project builds correctly.
 - Destroy any block with an axe in survival. It should set the player on fire.
 - Check every other collectible has an appended description to it.
 - Test to see if player is regularly (50%) "lucky" and avoids taking any damage.