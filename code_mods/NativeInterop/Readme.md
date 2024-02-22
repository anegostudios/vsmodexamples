# NativeInterop

This example shows how you can include your own native binaries into your mod.

Inside the [libcopystring](libcopystring) folder you can find source code in c for the native part.


The two major things needed are:
- you need to include the native binaries inside the `native` folder in you mod
    `NativeInterop/native/libcopystring[.dll,.so,.dylib]`
- setup a DllImportResolver so when your mod tries to acces a native function it can find the native binary first

See [nativeInterop.csproj](nativeInterop/nativeInterop.csproj), [NativeInteropModSystem.cs](nativeInterop/NativeInteropModSystem.cs) and [CakeBuild Program.cs](CakeBuild/Program.cs) for relevant changes to make a mod that calls native code and for how to build it.