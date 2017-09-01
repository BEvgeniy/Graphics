using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphics.Engine.VulkanDriver.VkDevice.Logical;
using Graphics.Engine.VulkanDriver.VkDevice.Physical;
using Graphics.Engine.VulkanDriver.VkSurface;

namespace Graphics.Engine.VulkanDriver.VkSwapchain
{
    internal sealed class VulkanSwapchainCreateInfo
    {

        /// <summary>
        /// Объект-обертка над физическим устройством
        /// </summary>
        public VulkanPhysicalDevice VulkanPhysicalDevice { get; set; }

        /// <summary>
        /// Объект-обертка над логическим устройством
        /// </summary>
        public VulkanLogicalDevice VulkanLogicalDevice { get; set; }

        /// <summary>
        /// Поверхность отрисовки (связана с окном вывода изображения)
        /// </summary>
        public VulkanSurface VulkanSurface { get; set; }
    }
}