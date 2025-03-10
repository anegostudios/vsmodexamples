using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Common;

namespace NativeInterop;

public class NativeInteropModSystem : ModSystem
{
    public override void StartServerSide(ICoreServerAPI sapi)
    {
        // First register the DllImportResolver so when we call the native function it can find the native binary
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
        
        // this will trigger the DllImportResolver() and find the binary and then call it
        var outputPtr = NativeInterop.CopyString("test string");
        string outString = Marshal.PtrToStringAnsi(outputPtr);
        Mod.Logger.Notification(outString);

        // here we should make sure to free the string resource allocated by the native library
        // this step is not included in this example, make sure this works cross platform
    }

    private IntPtr DllImportResolver(string libraryname, Assembly assembly, DllImportSearchPath? searchpath)
    {
        var suffix = RuntimeEnv.OS switch
        {
            OS.Windows => ".dll",
            OS.Mac => ".dylib",
            OS.Linux => ".so",
            _ => throw new ArgumentOutOfRangeException()
        };
        if (NativeLibrary.TryLoad($"{((ModContainer)Mod).FolderPath}/native/{libraryname}{suffix}", out var handle))
        {
            return handle;
        }
        return IntPtr.Zero;
    }
}

public class NativeInterop
{
    // "libcopystring" this is the name of the native binary, it will be used by the DllImportResolver as the libraryname
    [DllImport("libcopystring", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CopyString(string input);
}