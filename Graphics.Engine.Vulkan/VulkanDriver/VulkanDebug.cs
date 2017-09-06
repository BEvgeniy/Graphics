using System;
using Vulkan;

namespace Graphics.Engine.VulkanDriver
{
    internal static class VulkanDebug
    {
        public static void SetupDebugging(Instance vulkanInstance, DebugReportFlagsExt flags)
        {
            Instance.DebugReportCallback debugCallback = DebugReportCallback;
            vulkanInstance.EnableDebug(debugCallback);
        }

        static Bool32 DebugReportCallback(DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, ulong objectHandle, IntPtr location, int messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData)
        {
            string layerString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(layerPrefix);
            string messageString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);

            System.Console.WriteLine("DebugReport layer: {0} message: {1}", layerString, messageString);

            return false;
        }

    }
}