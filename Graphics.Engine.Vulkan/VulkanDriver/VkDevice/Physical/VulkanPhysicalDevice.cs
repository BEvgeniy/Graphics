using System;
using System.Collections.Generic;
using System.Linq;
using Graphics.Engine.VulkanDriver.VkInstance;
using VulkanSharp;

namespace Graphics.Engine.VulkanDriver.VkDevice.Physical
{
    /// <summary>
    /// Создает объект-обертку над физическим устройством (видеоадаптером)
    /// </summary>
    internal sealed class VulkanPhysicalDevice
    {
        private Boolean _isInit;

        public VulkanPhysicalDevice()
        {
            PhysicalDeviceSupportedExtensions = new List<ExtensionProperties>();
            PhysicalDeviceQueueFamilyProperties = new List<QueueFamilyProperties>();
        }

        /// <summary>
        /// Экземпляр(объект или инстанс) Vulkan - хранит все состояния для текущего приложения
        /// Создается один раз при инициализации. 
        /// </summary>
        public VulkanInstance VulkanInstance { get; private set; }

        /// <summary>
        /// Выбранный видеоадаптер. 
        /// Видеоадаптер который был выбран системой автоматически (рекомендуется), либо указанный в настройках.
        /// Видеоадаптер может быть задан явно, через указание в настройках, в случае, когда в системе имеется несколько видеоадаптеров и ведется разработка, 
        /// либо по какой-то причине выбранный системой видаоадаптер не устраивает или не отрабатывает как от него ожидают.
        /// </summary>
        public PhysicalDevice PhysicalDevice { get; private set; }

        /// <summary>
        /// Тип физического устройства (видеоадаптера)
        /// </summary>
        public PhysicalDeviceType PhysicalDeviceType { get; private set; }

        /// <summary>
        /// Свойства видеоадаптера, такие как: версия драйвера, производитель, ограничения физического устройства (напр.: максимальный размер текструры)
        /// </summary>
        public PhysicalDeviceProperties PhysicalDeviceProperties { get; private set; }

        /// <summary>
        /// Возможности видеоадаптера такие как: поддержка геометрическо шейдера или шейдера тессиляции
        /// </summary>
        public PhysicalDeviceFeatures PhysicalDeviceFeatures { get; private set; }

        /// <summary>
        /// Свойства памяти видеоадаптера, используются регулярно, для создания всех видов буферов
        /// </summary>
        public PhysicalDeviceMemoryProperties PhysicalDeviceMemoryProperties { get; private set; }

        /// <summary>
        /// Список названий расширений, которые поддерживает видеоадаптер
        /// </summary>
        public IReadOnlyList<ExtensionProperties> PhysicalDeviceSupportedExtensions { get; private set; }

        /// <summary>
        /// Свойства семейств очередей видеоадаптера
        /// </summary>
        public IReadOnlyList<QueueFamilyProperties> PhysicalDeviceQueueFamilyProperties { get; private set; }

        /// <summary>
        /// Если физическое устройство поддерживает работу с графическими командами, 
        /// то данное свойство содержит индекс указывающий на это семейство очередей
        /// </summary>
        public Int32 GraphicsQueueIndex { get; private set; }

        /// <summary>
        /// Если физическое устройство поддерживает работу с представлением (поддерживает вывод графики на экран), 
        /// то данное свойство содержит индекс указывающий на это семейство очередей
        /// Желательно (избежать необходимости синхронизировать доступ к ресурсам), 
        /// чтобы индекс очереди был равен индексу <see cref="GraphicsQueueIndex"/>
        /// </summary>
        public Int32 PresentQueueIndex { get; private set; }

        /// <summary>
        /// Если физическое устройство поддерживает работу с командами вычисления, 
        /// то данное свойство содержит индекс указывающий на это семейство очередей
        /// Желательно, чтобы данное семейство поддерживало только этот тип команд
        /// </summary>
        public Int32 ComputeQueueIndex { get; private set; }

        /// <summary>
        /// Если физическое устройство поддерживает работу с командами передачи, 
        /// то данное свойство содержит индекс указывающий на это семейство очередей
        /// Желательно, чтобы данное семейство поддерживало только этот тип команд
        /// </summary>
        public Int32 TransferQueueIndex { get; private set; }

        /// <summary>
        /// Свойства семейства очередей приведенные в человеческий вид
        /// </summary>
        public IReadOnlyList<VulkanPhysicalDeviceQueueFamiliesParams> VulkanAvailablePhysicalDeviceQueueFamiliesParams
        {
            get;
            private set;
        }

        public void Create(VulkanPhysicalDeviceCreateInfo vulkanPhysicalDeviceCreateInfo)
        {
            if (_isInit)
            {
                return;
            }

            lock (this)
            {
                if (_isInit)
                {
                    return;
                }

                PhysicalDevice = vulkanPhysicalDeviceCreateInfo.PhysicalDevice;
                VulkanInstance = vulkanPhysicalDeviceCreateInfo.VulkanInstance;

                PhysicalDeviceProperties = PhysicalDevice.GetProperties();
                PhysicalDeviceType = PhysicalDeviceProperties.DeviceType;
                PhysicalDeviceFeatures = PhysicalDevice.GetFeatures();
                PhysicalDeviceMemoryProperties = PhysicalDevice.GetMemoryProperties();

                var extensions = PhysicalDevice.EnumerateDeviceExtensionProperties(null);
                if (extensions != null && extensions.Length > 0)
                {
                    PhysicalDeviceSupportedExtensions = extensions;
                }

                var queues = PhysicalDevice.GetQueueFamilyProperties();
                if (queues != null && queues.Length > 0)
                {
                    PhysicalDeviceQueueFamilyProperties = queues;
                }

                var queueParams = new List<VulkanPhysicalDeviceQueueFamiliesParams>();

                for (var i = 0; i < PhysicalDeviceQueueFamilyProperties.Count; i++)
                {
                    var queueFamilyPropertiesItem = PhysicalDeviceQueueFamilyProperties[i];
                    var queueFamiliesParams = new VulkanPhysicalDeviceQueueFamiliesParams
                    {
                        PresentIndex = -1,
                        ComputeIndex = -1,
                        GraphicsIndex = -1,
                        TransferIndex = -1,
                        QueueMaxCount = queueFamilyPropertiesItem.QueueCount
                    };
                    if (PhysicalDevice.GetSurfaceSupportKHR((UInt32) i,
                        vulkanPhysicalDeviceCreateInfo.VulkanSurface.Surface))
                    {
                        queueFamiliesParams.IsSupportPresent = true;
                        queueFamiliesParams.PresentIndex = i;
                    }

                    if ((queueFamilyPropertiesItem.QueueFlags & QueueFlags.Compute) == QueueFlags.Compute)
                    {
                        queueFamiliesParams.IsSupportCompute = true;
                        queueFamiliesParams.ComputeIndex = i;
                        if ((queueFamilyPropertiesItem.QueueFlags | QueueFlags.Compute) == QueueFlags.Compute)
                        {
                            queueFamiliesParams.IsSupportSeparatedCompute = true;
                            queueParams.Add(queueFamiliesParams);
                            continue;
                        }
                    }
                    if ((queueFamilyPropertiesItem.QueueFlags & QueueFlags.Transfer) == QueueFlags.Transfer)
                    {
                        queueFamiliesParams.IsSupportTransfer = true;
                        queueFamiliesParams.TransferIndex = i;
                        if ((queueFamilyPropertiesItem.QueueFlags | QueueFlags.Transfer) == QueueFlags.Transfer)
                        {
                            queueFamiliesParams.IsSupportSeparatedTransfer = true;
                            queueParams.Add(queueFamiliesParams);
                            continue;
                        }
                    }
                    if ((queueFamilyPropertiesItem.QueueFlags & QueueFlags.Graphics) == QueueFlags.Graphics)
                    {
                        queueFamiliesParams.IsSupportGraphics = true;
                        queueFamiliesParams.GraphicsIndex = i;
                        if ((queueFamilyPropertiesItem.QueueFlags | QueueFlags.Graphics) == QueueFlags.Graphics)
                        {
                            queueFamiliesParams.IsSupportSeparatedGraphics = true;
                            queueParams.Add(queueFamiliesParams);
                        }
                    }
                    queueParams.Add(queueFamiliesParams);
                }

                VulkanAvailablePhysicalDeviceQueueFamiliesParams = queueParams;

                GraphicsQueueIndex = -1;
                PresentQueueIndex = -1;
                ComputeQueueIndex = -1;
                TransferQueueIndex = -1;

                var queueFamilyParams =
                    VulkanAvailablePhysicalDeviceQueueFamiliesParams.FirstOrDefault(
                        queue => queue.IsSupportSeparatedGraphics &&
                                 queue.IsSupportPresent) ??
                    VulkanAvailablePhysicalDeviceQueueFamiliesParams.FirstOrDefault(
                        queue => queue.IsSupportGraphics &&
                                 queue.IsSupportPresent);

                if (queueFamilyParams != null)
                {
                    GraphicsQueueIndex = queueFamilyParams.GraphicsIndex;
                    PresentQueueIndex = queueFamilyParams.PresentIndex;
                }
                else
                {
                    queueFamilyParams =
                        VulkanAvailablePhysicalDeviceQueueFamiliesParams.FirstOrDefault(
                            queue => queue.IsSupportSeparatedGraphics) ??
                        VulkanAvailablePhysicalDeviceQueueFamiliesParams.FirstOrDefault(
                            queue => queue.IsSupportGraphics);

                    if (queueFamilyParams != null)
                    {
                        GraphicsQueueIndex = queueFamilyParams.GraphicsIndex;
                    }

                    queueFamilyParams =
                        VulkanAvailablePhysicalDeviceQueueFamiliesParams.FirstOrDefault(
                            queue => queue.IsSupportPresent);

                    if (queueFamilyParams != null)
                    {
                        PresentQueueIndex = queueFamilyParams.PresentIndex;
                    }
                }

                queueFamilyParams =
                    VulkanAvailablePhysicalDeviceQueueFamiliesParams.FirstOrDefault(
                        queue => queue.IsSupportCompute &&
                                 queue.IsSupportSeparatedCompute) ??
                    VulkanAvailablePhysicalDeviceQueueFamiliesParams.FirstOrDefault(
                        queue => queue.IsSupportCompute);

                if (queueFamilyParams != null)
                {
                    ComputeQueueIndex = queueFamilyParams.ComputeIndex;
                }

                queueFamilyParams =
                    VulkanAvailablePhysicalDeviceQueueFamiliesParams.FirstOrDefault(
                        queue => queue.IsSupportTransfer &&
                                 queue.IsSupportSeparatedTransfer) ??
                    VulkanAvailablePhysicalDeviceQueueFamiliesParams.FirstOrDefault(
                        queue => queue.IsSupportTransfer);

                if (queueFamilyParams != null)
                {
                    TransferQueueIndex = queueFamilyParams.TransferIndex;
                }

                _isInit = true;
            }
        }

        public ExtensionProperties GetExtensionPropertiesByName(String extentionName)
        {
            return PhysicalDeviceSupportedExtensions.FirstOrDefault(e => e.ExtensionName == extentionName);
        }
    }
}