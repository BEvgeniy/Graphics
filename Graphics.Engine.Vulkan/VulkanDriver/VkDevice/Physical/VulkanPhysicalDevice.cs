using System;
using System.Collections.Generic;
using System.Linq;
using Graphics.Engine.VulkanDriver.VkInstance;
using Vulkan;

namespace Graphics.Engine.VulkanDriver.VkDevice.Physical
{
    /// <summary>
    /// Создает объект-обертку над физическим устройством (видеоадаптером)
    /// </summary>
    internal sealed class VulkanPhysicalDevice
    {
        #region .props

        /// <summary>
        /// Экземпляр(объект или инстанс) Vulkan - хранит все состояния для текущего приложения
        /// Создается один раз при инициализации. 
        /// </summary>
        public VulkanInstance VulkanInstance { get; private set; }

        /// <summary>
        /// Выбранный видеоадаптер. 
        /// Видеоадаптер, который был выбран системой автоматически (рекомендуется), либо указанный в настройках.
        /// Видеоадаптер может быть задан явно, через указание в настройках, в случае, когда в системе имеется несколько видеоадаптеров и ведется разработка, 
        /// либо по какой-то причине выбранный системой видаоадаптер не устраивает или не отрабатывает как от него ожидают.
        /// </summary>
        public PhysicalDevice PhysicalDevice { get; private set; }

        /// <summary>
        /// Тип физического устройства (видеоадаптера)
        /// </summary>
        public PhysicalDeviceType PhysicalDeviceType { get; private set; }

        /// <summary>
        /// Если физическое устройство поддерживает работу с графическими командами, 
        /// то данное свойство содержит индекс указывающий на это семейство очередей
        /// </summary>
        public Int32 GraphicsQueueIndex { get; private set; }

        /// <summary>
        /// Если физическое устройство поддерживает работу с представлением (поддерживает вывод графики на экран), 
        /// то данное свойство содержит индекс указывающий на это семейство очередей
        /// Желательно (дабы избежать необходимости синхронизировать доступ к ресурсам), 
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
        /// Если физическое устройство поддерживает работу с графическими командами, 
        /// то данное свойство возвращает true
        /// </summary>
        public Boolean IsGraphicsQueueSupported => GraphicsQueueIndex >= 0;

        /// <summary>
        /// Если физическое устройство поддерживает работу с представлением (поддерживает вывод графики на экран), 
        /// то данное свойство возвращает true
        /// </summary>
        public Boolean IsPresentQueueSupported => PresentQueueIndex >= 0;

        /// <summary>
        /// Если физическое устройство поддерживает работу с командами вычисления, 
        /// то данное свойство возвращает true
        /// </summary>
        public Boolean IsComputeQueueSupported => ComputeQueueIndex >= 0;

        /// <summary>
        /// Если физическое устройство поддерживает работу с командами передачи, 
        /// то данное свойство возвращает true
        /// </summary>
        public Boolean IsTransferQueueSupported => TransferQueueIndex >= 0;

        #endregion

        #region .public.sector

        #region .static

        /// <summary>
        /// Возвращает свойства расширения физического устройства по указанному имени или null
        /// </summary>
        public static ExtensionProperties GetExtensionPropertiesByName(IReadOnlyList<ExtensionProperties> extensions,
            String extensionName)
        {
            return extensions.FirstOrDefault(e => e.ExtensionName == extensionName);
        }

        #endregion

        #region .instance

        /// <summary>
        /// Свойства видеоадаптера, такие как: версия драйвера, производитель, ограничения физического устройства (напр.: максимальный размер текструры)
        /// </summary>
        public PhysicalDeviceProperties GetPhysicalDeviceProperties()
        {
            return PhysicalDevice.GetProperties();
        }

        /// <summary>
        /// Возможности видеоадаптера такие как: поддержка геометрическо шейдера или шейдера тессиляции
        /// </summary>
        public PhysicalDeviceFeatures GetPhysicalDeviceFeatures()
        {
            return PhysicalDevice.GetFeatures();
        }

        /// <summary>
        /// Свойства памяти видеоадаптера, используются регулярно, для создания всех видов буферов
        /// </summary>
        public PhysicalDeviceMemoryProperties GetPhysicalDeviceMemoryProperties()
        {
            return PhysicalDevice.GetMemoryProperties();
        }

        /// <summary>
        /// Список названий расширений, которые поддерживает видеоадаптер
        /// </summary>
        public IReadOnlyList<ExtensionProperties> GetPhysicalDeviceExtensions()
        {
            var availDeviceExtensions = new List<ExtensionProperties>();
            var availableDeviceExtensions = PhysicalDevice.EnumerateDeviceExtensionProperties();
            if (availableDeviceExtensions != null && availableDeviceExtensions.Length > 0)
            {
                availDeviceExtensions.AddRange(availableDeviceExtensions);
            }
            return availDeviceExtensions;
        }

        /// <summary>
        /// Позволяет узнать поддерживает ли физическое устройство расширение по указанному имени
        /// </summary>
        public Boolean IsExtensionSupportedByDevice(String extensionName)
        {
            return GetExtensionPropertiesByName(GetPhysicalDeviceExtensions(), extensionName) != null;
        }

        /// <summary>
        /// Свойства семейств очередей видеоадаптера
        /// </summary>
        public IReadOnlyList<QueueFamilyProperties> GetPhysicalDeviceQueueFamilyProperties()
        {
            var availDeviceQueueFamilyPropertiesItems = new List<QueueFamilyProperties>();
            var availableDeviceQueueFamilyPropertiesItem = PhysicalDevice.GetQueueFamilyProperties();
            if (availableDeviceQueueFamilyPropertiesItem != null && availableDeviceQueueFamilyPropertiesItem.Length > 0)
            {
                availDeviceQueueFamilyPropertiesItems.AddRange(availableDeviceQueueFamilyPropertiesItem);
            }
            return availDeviceQueueFamilyPropertiesItems;
        }

        /// <summary>
        /// Свойства семейства очередей приведенные в человеческий вид
        /// </summary>
        public IReadOnlyList<VulkanPhysicalDeviceQueueFamiliesParams>
            GetVulkanAvailablePhysicalDeviceQueueFamiliesParams()
        {
            var queueParams = new List<VulkanPhysicalDeviceQueueFamiliesParams>();
            var queues = GetPhysicalDeviceQueueFamilyProperties();
            for (var i = 0; i < queues.Count; i++)
            {
                var queueFamilyPropertiesItem = queues[i];

                var queueFamiliesParams = new VulkanPhysicalDeviceQueueFamiliesParams
                {
                    PresentIndex = -1,
                    ComputeIndex = -1,
                    GraphicsIndex = -1,
                    TransferIndex = -1,
                    QueueMaxCount = queueFamilyPropertiesItem.QueueCount
                };

                if (PhysicalDevice.GetSurfaceSupportKHR((UInt32) i,
                    VulkanInstance.VulkanSurface.Surface))
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

            return queueParams;
        }

        public SurfaceCapabilitiesKhr GetAvailableSurfaceCapabilities()
        {
            return PhysicalDevice.GetSurfaceCapabilitiesKHR(VulkanInstance.VulkanSurface.Surface);
        }

        public IReadOnlyList<SurfaceFormatKhr> GetAvailableSurfaceFormats()
        {
            var availSurfaceFormats = new List<SurfaceFormatKhr>();
            var availSurfaceFormat = PhysicalDevice.GetSurfaceFormatsKHR(VulkanInstance.VulkanSurface.Surface);
            if (availSurfaceFormat != null && availSurfaceFormat.Length > 0)
            {
                availSurfaceFormats.AddRange(availSurfaceFormat);
            }
            return availSurfaceFormats;
        }

        public IReadOnlyList<PresentModeKhr> GetAvailableSurfacePresentModes()
        {
            var availSurfacePresentModes = new List<PresentModeKhr>();
            var availSurfacePresentMode =
                PhysicalDevice.GetSurfacePresentModesKHR(VulkanInstance.VulkanSurface.Surface);
            if (availSurfacePresentMode != null && availSurfacePresentMode.Length > 0)
            {
                availSurfacePresentModes.AddRange(availSurfacePresentMode);
            }
            return availSurfacePresentModes;
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

                if (PhysicalDevice == null)
                {
                    throw new ArgumentException("Необходимо задать физическое устройство PhysicalDevice",
                        nameof(vulkanPhysicalDeviceCreateInfo));
                }

                VulkanInstance = vulkanPhysicalDeviceCreateInfo.VulkanInstance;

                if (VulkanInstance == null)
                {
                    throw new ArgumentException("Необходимо задать экземпляр объекта VulkanInstance",
                        nameof(vulkanPhysicalDeviceCreateInfo));
                }

                var physicalDeviceProperties = GetPhysicalDeviceProperties();

                PhysicalDeviceType = physicalDeviceProperties.DeviceType;

                var physicalDeviceFeatures = GetPhysicalDeviceFeatures();
                var physicalDeviceMemoryProperties = GetPhysicalDeviceMemoryProperties();
                var physicalDeviceExtensions = GetPhysicalDeviceExtensions();

                var physicalDeviceQueueParams = GetVulkanAvailablePhysicalDeviceQueueFamiliesParams();

                GraphicsQueueIndex = -1;
                PresentQueueIndex = -1;
                ComputeQueueIndex = -1;
                TransferQueueIndex = -1;

                var queueFamilyParams =
                    physicalDeviceQueueParams.FirstOrDefault(
                        queue => queue.IsSupportSeparatedGraphics &&
                                 queue.IsSupportPresent) ??
                    physicalDeviceQueueParams.FirstOrDefault(
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
                        physicalDeviceQueueParams.FirstOrDefault(
                            queue => queue.IsSupportSeparatedGraphics) ??
                        physicalDeviceQueueParams.FirstOrDefault(
                            queue => queue.IsSupportGraphics);

                    if (queueFamilyParams != null)
                    {
                        GraphicsQueueIndex = queueFamilyParams.GraphicsIndex;
                    }

                    queueFamilyParams =
                        physicalDeviceQueueParams.FirstOrDefault(
                            queue => queue.IsSupportPresent);

                    if (queueFamilyParams != null)
                    {
                        PresentQueueIndex = queueFamilyParams.PresentIndex;
                    }
                }

                queueFamilyParams =
                    physicalDeviceQueueParams.FirstOrDefault(
                        queue => queue.IsSupportCompute &&
                                 queue.IsSupportSeparatedCompute) ??
                    physicalDeviceQueueParams.FirstOrDefault(
                        queue => queue.IsSupportCompute);

                if (queueFamilyParams != null)
                {
                    ComputeQueueIndex = queueFamilyParams.ComputeIndex;
                }

                queueFamilyParams =
                    physicalDeviceQueueParams.FirstOrDefault(
                        queue => queue.IsSupportTransfer &&
                                 queue.IsSupportSeparatedTransfer) ??
                    physicalDeviceQueueParams.FirstOrDefault(
                        queue => queue.IsSupportTransfer);

                if (queueFamilyParams != null)
                {
                    TransferQueueIndex = queueFamilyParams.TransferIndex;
                }

                _isInit = true;
            }
        }

        public ExtensionProperties GetExtensionPropertiesByName(String extensionName)
        {
            return GetPhysicalDeviceExtensions().FirstOrDefault(e => e.ExtensionName == extensionName);
        }

        #endregion

        #endregion

        #region .fields

        private Boolean _isInit;

        #endregion

        #region .ctors

        public VulkanPhysicalDevice()
        {
            PresentQueueIndex = -1;
            ComputeQueueIndex = -1;
            GraphicsQueueIndex = -1;
            TransferQueueIndex = -1;
        }

        #endregion
    }
}