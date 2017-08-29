using System.Collections.Generic;
using Graphics.Engine.VulkanDriver.VkDevice.Physical;
using VulkanSharp;

namespace Graphics.Engine.VulkanDriver.VkDevice.Logical
{
    /// <summary>
    /// Создает объект-обертку над логическим устройством (видеоадаптером).
    /// </summary>
    internal sealed class VulkanLogicalDevice
    {
        public VulkanLogicalDevice()
        {
            VulkanEnabledLogicalDeviceExtentions  = new List<ExtensionProperties>();
        }
        
        /// <summary>
        /// Видеоадаптер, для которого было создано логическое устройство (помещенное в объект обертку).
        /// </summary>
        public VulkanPhysicalDevice VulkanPhysicalDevice { get; private set; }

        /// <summary>
        /// Логическое устройство созданное поверх физического (видеоадаптер). 
        /// При создании логического устройства указываются необходимые пользователю расширения и другие требуемые возможности, 
        /// которые доступны у физического устройства. При создании логического устройства не обязательно указывать все-возможные 
        /// свойства (фичи), которые предлагает нам физическое устройство. Поэтому мы можем создать несколько логических устройств, у которых
        /// для создания использовался один и тот же видеоадаптер.
        /// </summary>
        public Device Device { get; private set; }

        /// <summary>
        /// Названия расширений, которые подключены к созданному логическому устройству.
        /// </summary>
        public IReadOnlyList<ExtensionProperties> VulkanEnabledLogicalDeviceExtentions { get; private set; }

        /// <summary>
        /// Возможности физического устройства, которые используются логическим устройством.
        /// </summary>
        public PhysicalDeviceFeatures VulkanEnabledLogicalDeviceFeatures { get; private set; }

        /// <summary>
        /// Создает логическое устройство с указанными пользователем расширениями и свойствами (фичами).
        /// </summary>
        public void Create(VulkanLogicalDeviceCreateInfo vulkanLogicalDeviceCreateInfo)
        {
            //VulkanEnabledLogicalDeviceExtentions = requestedExtentions;
            //VulkanEnabledLogicalDeviceFeatures = requestedFeatures;
        }
    }
}
