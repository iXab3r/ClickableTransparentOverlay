using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ClickableTransparentOverlay.Win32;

public static class NativeWarmup
{
    private const uint GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT = 0x00000002;

    /// <summary>
    ///     (a) LoadLibrary(name) to let Windows resolve it (PATH / AddDllDirectory / etc),
    ///     (b) GetModuleHandleEx + GetModuleFileName to get the full path,
    ///     (c) NativeLibrary.Load(fullPath) so later loads by full path are consistent.
    ///     Returns the handle from NativeLibrary.Load(fullPath) and the resolved full path.
    /// </summary>
    public static (nint Handle, string FullPath) LoadByNameResolvePathThenLoadByFullPath(string dllName)
    {
        if (string.IsNullOrWhiteSpace(dllName))
        {
            throw new ArgumentException("DLL name must not be null/empty.", nameof(dllName));
        }

        // (a) Let Windows resolve it using its current search configuration.
        var hLoadLibrary = LoadLibraryW(dllName);
        if (hLoadLibrary == 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"LoadLibrary failed for '{dllName}'.");
        }

        try
        {
            // (b) Ask Windows for the module handle (unchanged refcount) and query its fully qualified path.
            if (!GetModuleHandleExW(GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, dllName, out var hModule) || hModule == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"GetModuleHandleEx failed for '{dllName}'.");
            }

            var fullPath = GetModuleFilePath(hModule);

            // Normalize (optional, but nice for consistency)
            fullPath = Path.GetFullPath(fullPath);

            // (c) Load using NativeLibrary with the full path.
            // This will typically return the same module handle but increments the module refcount.
            var hNative = NativeLibrary.Load(fullPath);

            // Avoid leaking the extra refcount from LoadLibrary (keep only the NativeLibrary refcount).
            // If you want the module to have 2 refs (stays loaded longer), delete this.
            _ = FreeLibrary(hLoadLibrary);

            return (hNative, fullPath);
        }
        catch
        {
            // If anything fails after LoadLibrary, undo the LoadLibrary refcount to avoid pinning it.
            _ = FreeLibrary(hLoadLibrary);
            throw;
        }
    }

    private static string GetModuleFilePath(nint hModule)
    {
        // Typical MAX_PATH is 260, but modules can exceed it. Grow until it fits.
        var sb = new StringBuilder(512);

        while (true)
        {
            var len = GetModuleFileNameW(hModule, sb, (uint) sb.Capacity);
            if (len == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "GetModuleFileName failed.");
            }

            // If len == capacity-1, it may be truncated; grow and retry.
            if (len < sb.Capacity - 1)
            {
                return sb.ToString(0, (int) len);
            }

            sb.Capacity *= 2;
        }
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern nint LoadLibraryW(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeLibrary(nint hModule);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool GetModuleHandleExW(uint dwFlags, string lpModuleName, out nint phModule);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern uint GetModuleFileNameW(nint hModule, StringBuilder lpFilename, uint nSize);
}