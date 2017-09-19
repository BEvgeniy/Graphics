using System;
using Vulkan;
using Version = System.Version;

namespace Graphics.Engine.VulkanDriver
{
    internal static class VulkanTools
    {
        public static String PhysicalDeviceTypeString(PhysicalDeviceType type)
        {
            switch (type)
            {
                case PhysicalDeviceType.Other:
                    return "Неизвестное устройство";
                case PhysicalDeviceType.IntegratedGpu:
                    return "Встроенный видеоадаптер";
                case PhysicalDeviceType.DiscreteGpu:
                    return "Внешний видеоадаптер";
                case PhysicalDeviceType.VirtualGpu:
                    return "Виртуальный видеоадаптер";
                case PhysicalDeviceType.Cpu:
                    return "Процессор";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static String GetVersionAsString(UInt32 apiVersion)
        {
            return (apiVersion >> 22) + "." +
                   ((apiVersion >> 12) & 0x3ff) + "." +
                   (apiVersion & 0xfff);
        }

        public static UInt32 GetVulkanVersion(UInt32 major, UInt32 minor, UInt32 patch)
        {
            return Vulkan.Version.Make(major, minor, patch);
        }

        public static Version GetDotNetVersion(UInt32 vulkanVersion)
        {
            return new Version((Int32) (vulkanVersion >> 22),
                (Int32) ((vulkanVersion >> 12) & 0x3ff),
                (Int32) (vulkanVersion & 0xfff));
        }
      
    }
}