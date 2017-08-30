using System;
using System.Collections.Generic;
using System.Linq;
using Graphics.Engine.Settings;
using Graphics.Engine.VulkanDriver.VkDevice.Physical;
using Graphics.Engine.VulkanDriver.VkSurface;
using VulkanSharp;

namespace Graphics.Engine.VulkanDriver.VkInstance
{
    /// <summary>
    /// Создает объект-обертку над экземпляром Vulkan
    /// </summary>
    internal sealed class VulkanInstance
    {
        private Boolean _isInit;

        public VulkanInstance()
        {
            AvailableInstanceExtensions = new List<ExtensionProperties>();
            AvailableInstanceLayers = new List<LayerProperties>();
            PhysicalDevices = new List<PhysicalDevice>();
            EnabledInstanceExtensions = new List<ExtensionProperties>();
            EnabledInstanceLayers = new List<LayerProperties>();
        }

        /// <summary>
        /// Предустановленные расширения в системе, которые доступны для задания при создании экземпляра Vulkan 
        /// </summary>
        public IReadOnlyList<ExtensionProperties> AvailableInstanceExtensions { get; private set; }

        /// <summary>
        /// Предустановленные слои в системе, экземпляра которые доступны для задания при создании экземпляра Vulkan 
        /// </summary>
        /// TODO: (надо разобраться какие слои для чего нужны - очень слабое понимание на сегодняшний день)
        public IReadOnlyList<LayerProperties> AvailableInstanceLayers { get; private set; }

        /// <summary>
        /// Экземпляр(объект или инстанс) Vulkan - хранит все состояния для текущего приложения
        /// Создается один раз при инициализации. 
        /// </summary>
        public Instance Instance { get; private set; }

        /// <summary>
        /// Названия расширений, которые подключены к созданному экземпляру Vulkan
        /// </summary>
        public IReadOnlyList<ExtensionProperties> EnabledInstanceExtensions { get; private set; }

        /// <summary>
        /// Названия слоев, которые подключены к созданному экземпляру Vulkan
        /// </summary>
        public IReadOnlyList<LayerProperties> EnabledInstanceLayers { get; private set; }

        /// <summary>
        /// Список физических устройств (видеоадаптеров) доступных в системе (интегрированная или внешняя или тандем (sli))
        /// </summary>
        public IReadOnlyList<PhysicalDevice> PhysicalDevices { get; private set; }

        /// <summary>
        /// Поверхность для отрисовки заданная по умолчанию (создается после создания экземпляра Vulkan)
        /// </summary>
        public VulkanSurface VulkanSurface { get; private set; }

        public ExtensionProperties GetAvailableInstanceExtensionPropertiesByName(String extensionName)
        {
            return GetExtensionPropertiesByName(AvailableInstanceExtensions, extensionName);
        }

        public static ExtensionProperties GetExtensionPropertiesByName(IReadOnlyList<ExtensionProperties> extensions,
            String extensionName)
        {
            return extensions.FirstOrDefault(e => e.ExtensionName == extensionName);
        }

        public LayerProperties GetLayerPropertiesByName(String layerName)
        {
            return AvailableInstanceLayers.FirstOrDefault(e => e.LayerName == layerName);
        }

        public VulkanSurface CreateSurface(INativeWindow vulkanWindow)
        {
            return CreateSurface(this, vulkanWindow);
        }

        public static VulkanSurface CreateSurface(VulkanInstance vulkanInstance, INativeWindow vulkanWindow)
        {
            var createInfo = new VulkanSurfaceCreateInfo
            {
                VulkanInstance = vulkanInstance,
                VulkanWindow = vulkanWindow
            };
            var vulkanSurface = new VulkanSurface();
            vulkanSurface.Create(createInfo);
            return vulkanSurface;
        }

        public void Create(VulkanInstanceCreateInfo vulkanInstanceCreateInfo)
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
                // Получим расширения и слои, которые доступны для использования при создании экземпляра Vulkan

                var availInstExtensions = new List<ExtensionProperties>();
                var availInstLayers = new List<LayerProperties>();

                var availableInstanceExtensions = Commands.EnumerateInstanceExtensionProperties(null);

                if (availableInstanceExtensions != null && availableInstanceExtensions.Length > 0)
                {
                    availInstExtensions.AddRange(availableInstanceExtensions);
                    AvailableInstanceExtensions = availInstExtensions;
                }

                var availableInstanceLayers = Commands.EnumerateInstanceLayerProperties();

                if (availableInstanceLayers != null && availableInstanceLayers.Length > 0)
                {
                    availInstLayers.AddRange(availableInstanceLayers);
                    AvailableInstanceLayers = availInstLayers;
                }

                var requestedLayers = new List<LayerProperties>();
                var requestedExtensions = new List<ExtensionProperties>();

                foreach (var name in vulkanInstanceCreateInfo.RequestedExtensionNames)
                {
                    var extension = GetAvailableInstanceExtensionPropertiesByName(name);
                    if (extension == null)
                    {
                        throw new Exception(
                            "Среди доступных расширений в системе, не обнаружено запрошенное расширение с именем '" +
                            name + "'");
                    }
                    requestedExtensions.Add(extension);
                }

                // Если собираемся отлаживаться и проходить валидацию по слоям, надо подключить расширение отладки и слои валидации
                foreach (var name in vulkanInstanceCreateInfo.RequestedLayerNames)
                {
                    var layer = GetLayerPropertiesByName(name);
                    if (layer == null)
                    {
                        throw new Exception(
                            "Среди доступных слоев в системе, не обнаружено запрошенный слой с именем '" +
                            name + "'");
                    }
                    requestedLayers.Add(layer);
                }

                EnabledInstanceExtensions = requestedExtensions;
                EnabledInstanceLayers = requestedLayers;
                // Заполним необходимые параметры для создания экземпляра Vulkan
                var appInfo = new ApplicationInfo
                {
                    ApiVersion =
                        VulkanSharp.Version.Make((UInt32) vulkanInstanceCreateInfo.VulkanApiVersion.Major,
                            (UInt32) vulkanInstanceCreateInfo.VulkanApiVersion.Minor,
                            (UInt32) vulkanInstanceCreateInfo.VulkanApiVersion.Build),
                    ApplicationVersion = VulkanSharp.Version.Make(
                        (UInt32) vulkanInstanceCreateInfo.ApplicationVersion.Major,
                        (UInt32) vulkanInstanceCreateInfo.ApplicationVersion.Minor,
                        (UInt32) vulkanInstanceCreateInfo.ApplicationVersion.Build),
                    ApplicationName = vulkanInstanceCreateInfo.ApplicationName,
                    EngineName = vulkanInstanceCreateInfo.EngineName,
                    EngineVersion = VulkanSharp.Version.Make((UInt32) vulkanInstanceCreateInfo.EngineVersion.Major,
                        (UInt32) vulkanInstanceCreateInfo.EngineVersion.Minor,
                        (UInt32) vulkanInstanceCreateInfo.EngineVersion.Build),
                };

                var createInfo = new InstanceCreateInfo
                {
                    ApplicationInfo = appInfo,
                    EnabledExtensionCount = (UInt32) EnabledInstanceExtensions.Count,
                    EnabledExtensionNames = EnabledInstanceExtensions.Select(e => e.ExtensionName).ToArray(),
                    EnabledLayerCount = (UInt32) EnabledInstanceLayers.Count,
                    EnabledLayerNames = EnabledInstanceLayers.Select(e => e.LayerName).ToArray()
                };
                // Создадим экземпляр Vulkan
                Instance = new Instance(createInfo);
                // Создадим поверхность (Поверхность будет связана с главным окном программы и пока единственным)
                VulkanSurface = CreateSurface(this, vulkanInstanceCreateInfo.VulkanWindow);
                // Если требуется для отладки, включаем уровни проверки по умолчанию
                if (SettingsManager.IsDebugEnabled)
                {
                    // Указанные флаги отчетности определяют, какие сообщения для слоев следует отображать 
                    // Для проверки (отладки) приложения, битового флага Error и битового флага Warning, должно быть достаточно
                    const DebugReportFlagsExt debugReportFlags =
                        DebugReportFlagsExt.Error
                        | DebugReportFlagsExt.Warning
                        | DebugReportFlagsExt.PerformanceWarning
                        | DebugReportFlagsExt.Debug;
                    // Дополнительные битового флаги включают информацию о производительности, загрузчике и другие отладочные сообщения
                    VulkanDebug.SetupDebugging(Instance, debugReportFlags);
                }
                var physicalDevices = Instance.EnumeratePhysicalDevices();
                if (physicalDevices == null || physicalDevices.Length <= 0)
                {
                    throw new Exception(
                        "В системе не установлен подходящий видеоадаптер поддерживающий работу с Vulkan");
                }

                PhysicalDevices = new List<PhysicalDevice>(physicalDevices);

                if (SettingsManager.IsDebugEnabled)
                {
                    Console.WriteLine("Информация по видеоадаптерам в системе");
                    for (var i = 0; i < PhysicalDevices.Count; i++)
                    {
                        var physicalDevice = PhysicalDevices[i];
                        var deviceProperties = physicalDevice.GetProperties();
                        Console.WriteLine("[" + i + "] Название видеоадаптера: " + deviceProperties.DeviceName);
                        Console.WriteLine("[" + i + "] Тип видеоадаптера: " +
                                          VulkanTools.PhysicalDeviceTypeString(deviceProperties.DeviceType));
                        Console.WriteLine("[" + i + "] Версия графического API Vulkan поддерживаемая видеоадаптером: " +
                                          VulkanTools.GetVersionAsString(deviceProperties.ApiVersion));
                    }
                }
                _isInit = true;
            }
        }

        /// <summary>
        /// Поиск наилучшего видеоадаптера для работы с нашим приложением
        /// </summary>
        public PhysicalDevice FindSuitablePhysicalDevice(VulkanPhysicalDeviceSearchInfo searchInfo)
        {
            var rates = GetVulkanPhysicalDeviceRate(searchInfo);

            PhysicalDevice physicalDevice = null;
            var maxRate = 0U;

            foreach (var rate in rates)
            {
                if (rate.Rate > maxRate)
                {
                    maxRate = rate.Rate;
                    physicalDevice = rate.PhysicalDevice;
                }
            }

            return physicalDevice;
        }

        /// <summary>
        /// Определяет рейтинг устройства на основе поддерживаемой функциональности
        /// Чем выше рейтинг устройства, тем оно предпочтительнее для нас
        /// </summary>
        private IReadOnlyList<VulkanPhysicalDeviceRate> GetVulkanPhysicalDeviceRate(
            VulkanPhysicalDeviceSearchInfo searchInfo)
        {
            var rates = new List<VulkanPhysicalDeviceRate>();

            foreach (var physicalDevice in PhysicalDevices)
            {
                var deviceRate = 0U;
                var supportGraphics = false;
                var supportCompute = false;
                var supportTransfer = false;
                var supportPresentation = false;
                var supportRequestedExtensions = false;
                // ---
                var properties = physicalDevice.GetProperties();
                var supportedApiVersion = VulkanTools.GetDotNetVersion(properties.ApiVersion);
                var features = physicalDevice.GetFeatures();
                //var deviceMemory = physicalDevice.GetMemoryProperties();
                //UInt64 deviceMemoryHeapSize = deviceMemory.MemoryHeaps[0].Size;
                var deviceQueues = physicalDevice.GetQueueFamilyProperties();
                // ---
                if (deviceQueues == null || deviceQueues.Length <= 0)
                {
                    // Если физическое устройство не поддерживает ни одного семейства очередей,
                    // что наверное не возможно, то мы такое устройство даже рассматривать не будем 
                    rates.Add(GetZeroPhysicalDeviceRate(physicalDevice, properties.DeviceType));
                    continue;
                }

                for (var i = 0; i < deviceQueues.Length; i++)
                {
                    var queueFamilyProperties = deviceQueues[i];

                    if ((queueFamilyProperties.QueueFlags & QueueFlags.Graphics) == QueueFlags.Graphics)
                    {
                        supportGraphics = true;
                        if (searchInfo.IsRequestedSupportGraphicsQueue)
                        {
                            deviceRate += 1;
                        }
                    }
                    if ((queueFamilyProperties.QueueFlags & QueueFlags.Compute) == QueueFlags.Compute)
                    {
                        supportCompute = true;
                        if (searchInfo.IsRequestedSupportComputeQueue)
                        {
                            deviceRate += 1;
                        }
                    }
                    if ((queueFamilyProperties.QueueFlags & QueueFlags.Transfer) == QueueFlags.Transfer)
                    {
                        supportTransfer = true;
                        if (searchInfo.IsRequestedSupportTransferQueue)
                        {
                            deviceRate += 1;
                        }
                    }
                    if (searchInfo.IsRequestedSupportPresentationQueue)
                    {
                        if (physicalDevice.GetSurfaceSupportKHR((UInt32) i, searchInfo.VulkanSurface.Surface))
                        {
                            supportPresentation = true;
                            deviceRate += 1;
                        }
                    }
                    if (searchInfo.RequestedExtensionNames != null && searchInfo.RequestedExtensionNames.Any())
                    {
                        var extensions = physicalDevice.EnumerateDeviceExtensionProperties(null);
                        if (extensions != null && extensions.Length > 0)
                        {
                            var allExtensionsSupported = true;
                            foreach (var extensionName in searchInfo.RequestedExtensionNames)
                            {
                                var extension = GetExtensionPropertiesByName(extensions, extensionName);
                                if (extension == null)
                                {
                                    allExtensionsSupported = false;
                                    break;
                                }
                            }
                            supportRequestedExtensions = allExtensionsSupported;
                        }

                        if (!supportRequestedExtensions)
                        {
                            // Если физическое устройство не поддерживает все необходимые нам расширения
                            // то мы такое устройство не рассматриваем
                            rates.Add(GetZeroPhysicalDeviceRate(physicalDevice, properties.DeviceType));
                            continue;
                        }
                        else
                        {
                            deviceRate += 1;
                        }
                    }
                    else
                    {
                        supportRequestedExtensions = true;
                        deviceRate += 1;
                    }

                    if ((searchInfo.IsRequestedSupportGraphicsQueue && !supportGraphics) ||
                        (searchInfo.IsRequestedSupportComputeQueue && !supportCompute) ||
                        (searchInfo.IsRequestedSupportTransferQueue && !supportTransfer) ||
                        (searchInfo.IsRequestedSupportPresentationQueue && !supportPresentation))
                    {
                        // Если физическое устройство не поддерживает все необходимые нам очереди
                        // то мы такое устройство не рассматриваем
                        rates.Add(GetZeroPhysicalDeviceRate(physicalDevice, properties.DeviceType));
                    }
                }

                if (searchInfo.PreferredVulkanApiVersion != null)
                {
                    if ((supportedApiVersion < searchInfo.PreferredVulkanApiVersion) &&
                        (supportedApiVersion < SettingsManager.VulkanApiVersion))
                    {
                        // Если физическое устройство поддерживает версию Vulkan меньше чем предпочитаемая 
                        // и тем более чем минимально допустимая, то мы такое устройство не рассматриваем
                        rates.Add(GetZeroPhysicalDeviceRate(physicalDevice, properties.DeviceType));
                        continue;
                    }
                    else
                    {
                        deviceRate += 1;
                    }
                }

                // TODO: В процессе разработки пополнять необходимым, тем самым уточняя рейтинг устройств

                // Для нас обязательно должны поддержиться и геометричейский шейдер и шейдер тесселяции
                //if (!features.GeometryShader || !features.TessellationShader)
                //{
                //    rates.Add(GetZeroVulkanPhysicalDeviceRate(physicalDevice, properties.DeviceType));
                //    continue;
                //}

                // Максимально возможный размер текстур влияет на качество графики
                //deviceRate += properties.Limits.MaxImageDimension2D;

                rates.Add(new VulkanPhysicalDeviceRate
                {
                    PhysicalDevice = physicalDevice,
                    PhysicalDeviceType = properties.DeviceType,
                    Rate = deviceRate
                });
            }

            return rates.OrderByDescending(e => e.Rate).ToList();
        }

        private static VulkanPhysicalDeviceRate GetZeroPhysicalDeviceRate(PhysicalDevice physicalDevice,
            PhysicalDeviceType physicalDeviceType)
        {
            return new VulkanPhysicalDeviceRate
            {
                PhysicalDevice = physicalDevice,
                PhysicalDeviceType = physicalDeviceType,
                Rate = 0
            };
        }
    }
}