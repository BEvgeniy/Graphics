using System;
using System.Collections.Generic;
using Graphics.Engine.VulkanDriver.VkInstance;
using Graphics.Engine.VulkanDriver.VkSurface;
using Vulkan;

namespace Graphics.Engine.VulkanDriver.VkDevice.Physical
{
    /// <summary>
    /// Определяет параметры для создания создания объекта-обертки над физическим устройством
    /// </summary>
    internal sealed class VulkanPhysicalDeviceCreateInfo
    {
        /// <summary>
        /// Объект-обертка над экземпляром Vulkan
        /// </summary>
        public VulkanInstance VulkanInstance { get; set; }

        /// <summary>
        /// Физическое устройство доступное в системе
        /// </summary>
        public PhysicalDevice PhysicalDevice { get; set; }

        /// <summary>
        /// Поверхность отрисовки (связана с окном вывода изображения)
        /// </summary>
        public VulkanSurface VulkanSurface { get; set; }
    }
}