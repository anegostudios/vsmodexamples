# Vintage Story Server Mod Samples and Boilerplate

This repository contains multiple sample mods and a configured Visual Studio project that let's you start Vintage Story and test your mod directly from within Visual Studio.
- You might need to fix the path to the references of Vintage Story (like VintagestoryAPI.dll, Newtonsoft). The project is configured to use the environment variable "VINTAGE_STORY" for the game directory (default installation path is %appdata%/Vintagestory).
- There is a post build event that copies the .dll and .pdb files into bin/NameOfMod
- The game starts (defined in the launchSettings.json) with the commandline argument --addModPath which has currentSolutionDir/bin as argument. Also it uses the VINTAGE_STORY environment variable (add it to your system or change it here to your path).
- If you build in Release than the mods get copied to SolutionDir/release without pdb files.
- Even [Edit&Continue](https://msdn.microsoft.com/en-us/library/bcew296c.aspx) should work just fine \o/

## Jetbrains Rider IDE

If you use the Rider IDE, it has native support for Visual Studio projects, and you are able to import the entire folder which it'll configure for you.
- On each SLN (each mod folder, inside the VS project), you need to Right Click -> Add -> Add Reference -> `VintagestoryAPI.dll` in order to fix references
- On Linux only, you need to edit your `Properties/launchSettings.json` to launch Mono instead of the exe directly, due to a [bug](https://youtrack.jetbrains.com/issue/RIDER-75160) in Rider, for example:
```json
{
  "profiles": {
    "start-all-mods-in-sln": {
      "commandName": "Executable",
      "executablePath": "/usr/bin/mono",
      "commandLineArgs": "/usr/share/vintagestory/Vintagestory.exe --addModPath=\"$VINTAGE_STORY\" --dataPath=\"$SOLUTION_DIR\"",
      "environmentVariables": {
        "VINTAGE_STORY": "/usr/share/vintagestory/",
        "SOLUTION_DIR": "/home/f/projects/vsmodexamples/Mods/AnnoyingTextSystem/bin"
      }
    }
  }
}
```

# Documentation

For API Documentation and Guides please refer to [our official Wiki](http://wiki.vintagestory.at/). A good starting point is the [Setting up your Development Environment](https://wiki.vintagestory.at/index.php?title=Modding:Setting_up_your_Development_Environment) page.
