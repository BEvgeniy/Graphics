using System;
using VulkanSharp;

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

        public static UInt32 GetVersion(UInt32 major, UInt32 minor, UInt32 patch)
        {
            return VulkanSharp.Version.Make(major, minor, patch);
        }
    }
}