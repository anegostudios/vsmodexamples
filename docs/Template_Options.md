# VintageStory.Mod.Templates Options
- --Platform [Windows/Linux (default Windows)]

    The current platform for development (Only important for how to launch the mod for testing).

- --vsInstall [ /path/to/vs , default $(VINTAGE_STORY)]

    The directory of the Vintage Story installation to use for references and Client/Server launcher.

- --AddSampleCode [default true, disable with --addClientCode false]

    Adds sample code to the template mod

- --IncludeVSSurvivalMod

    Adds VSSurvivalMod as reference to the project

- --IncludeVSEssentials

    Adds VSEssentials as reference to the project

- --IncludeVSCreativeMod

    Adds VSCreativeMod as reference to the project

- --IncludeNewtonsoft

    Adds Newtonsoft.Json as reference to the project

- --IncludeHarmony

    Adds Harmony as reference to the project

- --IncludeVintagestoryLib

    Adds VintagestoryLib as reference to the project

- --IncludeProtobuf

    Adds Protobuf-net as reference to the project
    
- --IncludeCairoSharp

    Adds cairo-sharp as reference to the project

- --IncludeSQLite

    Adds System.Data.SQLite as reference to the project

- --IncludeVSCode

    Include VSCode tasks.json and launch.json