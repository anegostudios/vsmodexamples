# Vintage Story Server Mod Samples and Boilerplate

This repository contains multiple sample mods and a configured Visual Studio project that let's you start Vintage Story and test your mod directly from within Visual Studio.
- You might need to fix the path to the references of Vintage Story (like VintagestoryAPI.dll, Newtonsoft). The project is configured to use the environment variable "VINTAGE_STORY" for the game directory (default installation path is %appdata%/Vintagestory).
- There is a post build event that copies the .dll and .pdb files into bin/NameOfMod
- The game starts (defined in the launchSettings.json) with the commandline argument --addModPath which has currentSolutionDir/bin as argument. Also it uses the VINTAGE_STORY environment variable (add it to your system or change it here to your path).
- If you build in Release than the mods get copied to SolutionDir/release without pdb files.
- Even [Edit&Continue](https://msdn.microsoft.com/en-us/library/bcew296c.aspx) should work just fine \o/

# Documentation

For API Documentation and Guides please refer to [our official Wiki](http://wiki.vintagestory.at/)
