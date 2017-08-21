using System;
using Vulkan;

namespace Graphics.Engine.VulkanDriver
{
    internal static class VulkanDebug
    {
        static readonly Vulkan.Instance.DebugReportCallback DebugCallback = new Vulkan.Instance.DebugReportCallback(VulkanDebugInfo);

        public static void SetupDebugging(Vulkan.Instance vulkanInstance, Vulkan.DebugReportFlagsExt flags)
        {
            vulkanInstance.EnableDebug(DebugCallback, flags);
        }

        private static Vulkan.Bool32 VulkanDebugInfo(Vulkan.DebugReportFlagsExt flags,
            Vulkan.DebugReportObjectTypeExt objectType, UInt64 objectHandle, IntPtr location,
            Int32 messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData)
        {
            string layerString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(layerPrefix);
            string messageString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);

            if ((flags & Vulkan.DebugReportFlagsExt.Error) == Vulkan.DebugReportFlagsExt.Error)
            {
               
            }
            return new Bool32(true);
        }
    }
}