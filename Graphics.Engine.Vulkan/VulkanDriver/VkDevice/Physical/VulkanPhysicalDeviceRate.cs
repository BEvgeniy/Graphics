using System;
using VulkanSharp;
using Version = System.Version;

namespace Graphics.Engine.VulkanDriver.VkDevice.Physical
{
    internal sealed class VulkanPhysicalDeviceRate
    {
        /// <summary>
        /// Определяет рейтинг устройства
        /// </summary>
        public UInt32 Rate { get; set; }

        /// <summary>
        /// Тип физического устройства (видеоадаптера)
        /// </summary>
        public PhysicalDeviceType PhysicalDeviceType { get; set; }

        /// <summary>
        /// Тип физического устройства (видеоадаптера)
        /// </summary>
        public Version PhysicalDeviceSupportedVulkanApiVersion { get; set; }

        /// <summary>
        /// Физическое устройство доступное в системе
        /// </summary>
        public PhysicalDevice PhysicalDevice { get; set; }
    }
}