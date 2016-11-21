# Vintage Story Server Mod Samples and Boilerplate

This repository contains 2 sample mods and a neatly configured Visual Studio project that let's you start Vintage Story and test your mod 
directly from within Visual Studio.
- In the Projects settings, tab Debug the full path to Vintagestory.exe is configured. Please adjust this one to your machine.
- There Is a post build event that copies the .dll mod file into %appdata%/VintageStory/Mods. You only need to touch that one if you installed vintage story somewhere else
- A command line argument /flatworld is set in the Debug tab as well. It will automatically start a superflat creative world for you to test the mod.
- A tiny mod RedirectLogs.cs redirects the log output into the visual studio output window
- Even Edit&Continue seems to work just fine \o/

