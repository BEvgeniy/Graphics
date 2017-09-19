using Graphics.Engine.VulkanDriver.VkInstance;
using OpenTK;

namespace Graphics.Engine.VulkanDriver.VkSurface
{
    internal sealed class VulkanSurfaceCreateInfo
    {
        /// <summary>
        /// Объект-обертка над экземпляром Vulkan
        /// </summary>
        public VulkanInstance VulkanInstance { get; set; }

        /// <summary>
        /// Экземпляр окна для вывода изображения
        /// </summary>
        public INativeWindow VulkanWindow { get; set; }
    }
}