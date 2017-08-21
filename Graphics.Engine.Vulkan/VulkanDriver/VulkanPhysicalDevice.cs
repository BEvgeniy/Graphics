using System;
using System.Collections.Generic;
using System.Linq;

namespace Graphics.Engine.VulkanDriver
{
    /// <summary>
    /// Создает объект-обертку над физическим устройством (видеоадаптером)
    /// </summary>
    internal sealed class VulkanPhysicalDevice
    {
        public VulkanPhysicalDevice(VulkanInstance vulkanInstance, Vulkan.PhysicalDevice physicalDevice)
        {
            PhysicalDevice = physicalDevice ?? throw new ArgumentNullException(nameof(physicalDevice),
                                 "При инициализации класса не указан видеоадаптер");
            VulkanInstance = vulkanInstance ?? throw new ArgumentNullException(nameof(vulkanInstance),
                                 "При инициализации класса не указан инстанс Vulkan");
            VulkanPhysicalDeviceSupportedExtensions = new List<Vulkan.ExtensionProperties>();
            VulkanPhysicalDeviceQueueFamilyProperties = new List<Vulkan.QueueFamilyProperties>();
        }

        /// <summary>
        /// Экземпляр(объект или инстанс) Vulkan - хранит все состояния для текущего приложения
        /// Создается один раз при инициализации. 
        /// </summary>
        public VulkanInstance VulkanInstance { get; }

        /// <summary>
        /// Выбранный видеоадаптер. 
        /// Видеоадаптер который был выбран системой автоматически (рекомендуется), либо указанный в настройках.
        /// Видеоадаптер может быть задан явно, через указание в настройках, в случае, когда в системе имеется несколько видеоадаптеров и ведется разработка, 
        /// либо по какой-то причине выбранный системой видаоадаптер не устраивает или не отрабатывает как от него ожидают.
        /// </summary>
        public Vulkan.PhysicalDevice PhysicalDevice { get; }

        /// <summary>
        /// Свойства видеоадаптера, такие как: версия драйвера, производитель, ограничения физического устройства (напр.: максимальный размер текструры)
        /// </summary>
        public Vulkan.PhysicalDeviceProperties VulkanPhysicalDeviceProperties { get; private set; }

        /// <summary>
        /// Возможности видеоадаптера такие как: поддержка геометрическо шейдера или шейдера тессиляции
        /// </summary>
        public Vulkan.PhysicalDeviceFeatures VulkanPhysicalDeviceFeatures { get; private set; }

        /// <summary>
        /// Свойства памяти видеоадаптера, используются регулярно, для создания всех видов буферов
        /// </summary>
        public Vulkan.PhysicalDeviceMemoryProperties VulkanPhysicalDeviceMemoryProperties { get; private set; }

        /// <summary>
        /// Список названий расширений, которые поддерживает видеоадаптер
        /// </summary>
        public IReadOnlyList<Vulkan.ExtensionProperties> VulkanPhysicalDeviceSupportedExtensions { get; private set; }

        /// <summary>
        /// Свойства семейств очередей видеоадаптера
        /// </summary>
        public IReadOnlyList<Vulkan.QueueFamilyProperties> VulkanPhysicalDeviceQueueFamilyProperties
        {
            get;
            private set;
        }

        public Vulkan.ExtensionProperties GetExtensionPropertiesByName(String extentionName)
        {
            return VulkanPhysicalDeviceSupportedExtensions.FirstOrDefault(e => e.ExtensionName == extentionName);
        }

        public void Create()
        {
            VulkanPhysicalDeviceProperties = PhysicalDevice.GetProperties();
            VulkanPhysicalDeviceFeatures = PhysicalDevice.GetFeatures();
            VulkanPhysicalDeviceMemoryProperties = PhysicalDevice.GetMemoryProperties();
            var extensions = PhysicalDevice.EnumerateDeviceExtensionProperties();
            if (extensions != null && extensions.Length > 0)
            {
                VulkanPhysicalDeviceSupportedExtensions = extensions;
            }
            var queues = PhysicalDevice.GetQueueFamilyProperties();
            if (queues != null && queues.Length > 0)
            {
                VulkanPhysicalDeviceQueueFamilyProperties = queues;
            }
        }
    }
}