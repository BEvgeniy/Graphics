using System;

namespace Graphic.Engine.VulkanDriver
{
    internal static class VulkanDebug
    {
        public static void SetupDebugging(Vulkan.Instance vulkanInstance, Vulkan.DebugReportFlagsExt flags)
        {
            vulkanInstance.EnableDebug(VulkanDebugInfo, flags);
        }

        private static Vulkan.Bool32 VulkanDebugInfo(Vulkan.DebugReportFlagsExt flags,
            Vulkan.DebugReportObjectTypeExt objectType, UInt64 objectHandle, IntPtr location,
            Int32 messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData)
        {
            throw new NotImplementedException();
        }
    }
}