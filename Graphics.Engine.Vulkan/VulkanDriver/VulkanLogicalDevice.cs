using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulkanSharp;
 
namespace Graphics.Engine.VulkanDriver
{
    /// <summary>
    /// Создает объект-обертку над логическим устройством (видеоадаптером).
    /// </summary>
    internal class VulkanLogicalDevice
    {
        public VulkanLogicalDevice(VulkanPhysicalDevice vulkanPhysicalDevice)
        {
            VulkanPhysicalDevice = vulkanPhysicalDevice;
        }
        
        /// <summary>
        /// Видеоадаптер, для которого было создано логическое устройство (помещенное в объект обертку).
        /// </summary>
        public VulkanPhysicalDevice VulkanPhysicalDevice { get; }

        /// <summary>
        /// Логическое устройство созданное поверх физического (видеоадаптер). 
        /// При создании логического устройства указываются необходимые пользователю расширения и другие требуемые возможности, 
        /// которые доступны у физического устройства. При создании логического устройства не обязательно указывать все-возможные 
        /// свойства (фичи), которые предлагает нам физическое устройство. Поэтому мы можем создать несколько логических устройств, у которых
        /// для создания использовался один и тот же видеоадаптер.
        /// </summary>
        public Device Device { get; }

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
        /// <param name="requestedFeatures"></param>
        /// <param name="requestedExtentions"></param>
        public void Create(PhysicalDeviceFeatures requestedFeatures, List<ExtensionProperties> requestedExtentions)
        {
            VulkanEnabledLogicalDeviceExtentions = requestedExtentions;
            VulkanEnabledLogicalDeviceFeatures = requestedFeatures;


        }
    }
}
