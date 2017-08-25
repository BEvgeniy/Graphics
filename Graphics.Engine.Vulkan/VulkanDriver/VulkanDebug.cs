using System;
using VulkanSharp;

namespace Graphics.Engine.VulkanDriver
{
    internal static class VulkanDebug
    {
        //static readonly Instance.DebugReportCallback DebugCallback = new Instance.DebugReportCallback(VulkanDebugInfo);

        public static void SetupDebugging(Instance vulkanInstance, DebugReportFlagsExt flags)
        {
           // vulkanInstance.EnableDebug(DebugCallback, flags);
        }

        private static Bool32 VulkanDebugInfo(DebugReportFlagsExt flags,
            DebugReportObjectTypeExt objectType, UInt64 objectHandle, IntPtr location,
            Int32 messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData)
        {
            var layerString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(layerPrefix);
            var messageString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);

            if ((flags & DebugReportFlagsExt.Error) == DebugReportFlagsExt.Error)
            {
               
            }
            return new Bool32(true);
        }
    }
}