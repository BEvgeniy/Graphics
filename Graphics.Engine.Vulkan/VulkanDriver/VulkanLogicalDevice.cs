using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphics.Engine.VulkanDriver
{
    /// <summary>
    /// Создает объект-обертку над логическим устройством (видеоадаптером)
    /// </summary>
    internal class VulkanLogicalDevice
    {
        public VulkanLogicalDevice(VulkanPhysicalDevice vulkanPhysicalDevice)
        {
            VulkanPhysicalDevice = vulkanPhysicalDevice;
        }
        
        /// <summary>
        /// Видеоадаптер, для которого было создано логическое устройство (помещенное в объект обертку)
        /// </summary>
        public VulkanPhysicalDevice VulkanPhysicalDevice { get; }

        /// <summary>
        /// Названия расширений, которые подключены к созданному логическому устройству
        /// </summary>
        public IReadOnlyList<Vulkan.ExtensionProperties> VulkanEnabledLogicalDeviceExtentions { get; private set; }

        /// <summary>
        /// Возможности физического устройства, которые используются логическим устройством
        /// </summary>
        public Vulkan.PhysicalDeviceFeatures VulkanEnabledLogicalDeviceFeatures { get; private set; }

        public void Create(Vulkan.PhysicalDeviceFeatures requestedFeatures, List<Vulkan.ExtensionProperties> requestedExtentions)
        {
            VulkanEnabledLogicalDeviceExtentions = requestedExtentions;
            VulkanEnabledLogicalDeviceFeatures = requestedFeatures;

        }
    }
}
