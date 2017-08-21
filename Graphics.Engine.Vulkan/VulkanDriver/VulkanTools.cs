using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphic.Engine.VulkanDriver
{
    internal static class VulkanTools
    {
        public static String PhysicalDeviceTypeString(Vulkan.PhysicalDeviceType type)
        {
            switch (type)
            {
                case Vulkan.PhysicalDeviceType.Other:
                    return "Неизвестное устройство";
                case Vulkan.PhysicalDeviceType.IntegratedGpu:
                    return "Встроенный видеоадаптер";
                case Vulkan.PhysicalDeviceType.DiscreteGpu:
                    return "Внешний видеоадаптер";
                case Vulkan.PhysicalDeviceType.VirtualGpu:
                    return "Виртуальный видеоадаптер";
                case Vulkan.PhysicalDeviceType.Cpu:
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
    }
}
