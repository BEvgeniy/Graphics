using System;
using System.Runtime.InteropServices;
using VulkanSharp;

namespace Graphics.Engine.VulkanDriver
{
    internal static class VulkanDebug
    {
        
        private delegate Bool32 DebugReportCallback(DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, 
            UInt64 objectHandle, IntPtr location, Int32 messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData);

        static readonly DebugReportCallback DebugCallback = VulkanDebugInfo;

        private static DebugReportCallbackExt _debugReportCallbackExt = null;

        public static void SetupDebugging(Instance vulkanInstance, DebugReportFlagsExt flags)
        {
            var createInfo = new DebugReportCallbackCreateInfoExt
            {
                Flags = flags,
                PfnCallback = Marshal.GetFunctionPointerForDelegate(DebugCallback)
            };
            _debugReportCallbackExt = vulkanInstance.CreateDebugReportCallbackEXT(createInfo, (AllocationCallbacks)null);
        }

        private static Bool32 VulkanDebugInfo(DebugReportFlagsExt flags,
            DebugReportObjectTypeExt objectType, UInt64 objectHandle, IntPtr location,
            Int32 messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData)
        {
            var layerString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(layerPrefix);
            var messageString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);

            System.Console.WriteLine("DebugReport layer: {0} message: {1}", layerString, messageString);

            if ((flags & DebugReportFlagsExt.Error) == DebugReportFlagsExt.Error)
            {
                
            }
            return new Bool32(true);
        }
    }
}