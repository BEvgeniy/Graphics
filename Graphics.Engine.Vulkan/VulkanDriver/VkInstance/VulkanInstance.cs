using System;
using System.Collections.Generic;
using System.Linq;
using Graphics.Engine.Settings;
using Graphics.Engine.VulkanDriver.VkDevice.Physical;
using Graphics.Engine.VulkanDriver.VkSurface;
using OpenTK;
using Vulkan;

namespace Graphics.Engine.VulkanDriver.VkInstance
{
    /// <summary>
    /// Создает объект-обертку над экземпляром Vulkan
    /// </summary>
    internal sealed class VulkanInstance
    {
        #region .props

        /// <summary>
        /// Экземпляр(объект или инстанс) Vulkan - хранит все состояния для текущего приложения
        /// Создается один раз при инициализации. 
        /// </summary>
        public Instance Instance { get; private set; }

        /// <summary>
        /// Поверхность для отрисовки заданная по умолчанию (создается после создания экземпляра Vulkan)
        /// Текущая поверхность используется при создании цепочки свопинга или полчении
        /// дополнительных сведений о имеющихся в системе форматов отображения, цветовых(ой) схем(ы)
        /// </summary>
        public VulkanSurface VulkanSurface { get; private set; }

        /// <summary>
        /// Названия расширений, которые были затребованы (переданы в коструктор) при создании экземпяра Vulkan
        /// </summary>
        public IReadOnlyList<ExtensionProperties> EnabledInstanceExtensions { get; private set; }

        /// <summary>
        /// Названия слоев, которые были затребованы (переданы в коструктор) при создании экземпяра Vulkan
        /// </summary>
        public IReadOnlyList<LayerProperties> EnabledInstanceLayers { get; private set; }

        #endregion

        #region .public.sector

        #region .static

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

        /// <summary>
        /// Возвращает свойства слоя Vulkan по указанному имени или null
        /// </summary>
        public static LayerProperties GetLayerPropertiesByName(IReadOnlyList<LayerProperties> layers,
            String layerName)
        {
            return layers.FirstOrDefault(e => e.LayerName == layerName);
        }

        /// <summary>
        /// Возвращает свойства расширения Vulkan по указанному имени или null
        /// </summary>
        public static ExtensionProperties GetExtensionPropertiesByName(IReadOnlyList<ExtensionProperties> extensions,
            String extensionName)
        {
            return extensions.FirstOrDefault(e => e.ExtensionName == extensionName);
        }

        /// <summary>
        /// Предустановленные расширения в системе, которые доступны для задания при создании экземпляра Vulkan 
        /// </summary>
        public static IReadOnlyList<ExtensionProperties> GetAvailableInstanceExtensions()
        {
            var availInstExtensions = new List<ExtensionProperties>();
            var availableInstanceExtensions = Commands.EnumerateInstanceExtensionProperties(null);
            if (availableInstanceExtensions != null && availableInstanceExtensions.Length > 0)
            {
                availInstExtensions.AddRange(availableInstanceExtensions);
            }
            return availInstExtensions;
        }

        /// <summary>
        /// Предустановленные слои в системе, экземпляра которые доступны для задания при создании экземпляра Vulkan 
        /// Слои используются для подключения разных функций отладки работы Vulkan
        /// Наример можно влючить вывод отладочной информации по переданным парметрам в функции и последовательность их вызова,
        /// корректность передаваемых параметров, корректность создания и уничтожения объектов (утечки памяти)
        /// корректность вызовов при использовании многопоточности итд.
        /// </summary>
        public static IReadOnlyList<LayerProperties> GetAvailableInstanceLayers()
        {
            var availInstLayers = new List<LayerProperties>();
            var availableInstanceLayers = Commands.EnumerateInstanceLayerProperties();
            if (availableInstanceLayers != null && availableInstanceLayers.Length > 0)
            {
                availInstLayers.AddRange(availableInstanceLayers);
            }
            return availInstLayers;
        }

        #endregion

        #region .instance

        /// <summary>
        /// Список физических устройств (видеоадаптеров) доступных в системе (интегрированная или внешняя или тандем (sli))
        /// </summary>
        public IReadOnlyList<PhysicalDevice> GetPhysicalDevices()
        {
            if (Instance == null)
            {
                throw new Exception("Не создан экземпляр Vulkan");
            }
            var physicalDevices = Instance.EnumeratePhysicalDevices();
            if (physicalDevices == null || physicalDevices.Length <= 0)
            {
                throw new Exception(
                    "В системе не установлен подходящий видеоадаптер поддерживающий работу с Vulkan");
            }
            return physicalDevices;
        }

        /// <summary>
        /// Позволяет узнать поддерживает ли Vulkan расширение по указанному имени
        /// </summary>
        public Boolean IsExtensionSupportedByInstance(String extensionName)
        {
            return GetExtensionPropertiesByName(GetAvailableInstanceExtensions(), extensionName) != null;
        }

        public Boolean IsLayerSupportedByInstance(String layerName)
        {
            return GetAvailableInstanceLayers().FirstOrDefault(e => e.LayerName == layerName) != null;
        }

        public VulkanSurface CreateSurface(INativeWindow vulkanWindow)
        {
            return CreateSurface(this, vulkanWindow);
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

                var requestedLayers = new List<LayerProperties>();
                var requestedExtensions = new List<ExtensionProperties>();

                var availableInstanceExtensions = GetAvailableInstanceExtensions();

                foreach (var extensionName in vulkanInstanceCreateInfo.RequestedExtensionNames)
                {
                    var extension = availableInstanceExtensions.FirstOrDefault(e => e.ExtensionName == extensionName);
                    if (extension == null)
                    {
                        throw new Exception(
                            "Среди доступных расширений в системе, не обнаружено запрошенное расширение с именем '" +
                            extensionName + "'");
                    }
                    requestedExtensions.Add(extension);
                }

                var availableInstanceLayers = GetAvailableInstanceLayers();

                // Если собираемся отлаживаться и проходить валидацию по слоям, надо подключить расширение отладки и слои валидации
                foreach (var layerName in vulkanInstanceCreateInfo.RequestedLayerNames)
                {
                    var layer = availableInstanceLayers.FirstOrDefault(e => e.LayerName == layerName);
                    if (layer == null)
                    {
                        throw new Exception(
                            "Среди доступных слоев в системе, не обнаружен запрошенный слой с именем '" +
                            layerName + "'");
                    }
                    requestedLayers.Add(layer);
                }

                EnabledInstanceExtensions = requestedExtensions;
                EnabledInstanceLayers = requestedLayers;

                // Заполним необходимые параметры для создания экземпляра Vulkan

                var appInfo = new ApplicationInfo
                {
                    ApiVersion =
                        Vulkan.Version.Make((UInt32) vulkanInstanceCreateInfo.VulkanApiVersion.Major,
                            (UInt32) vulkanInstanceCreateInfo.VulkanApiVersion.Minor,
                            (UInt32) vulkanInstanceCreateInfo.VulkanApiVersion.Build),
                    ApplicationVersion = Vulkan.Version.Make(
                        (UInt32) vulkanInstanceCreateInfo.ApplicationVersion.Major,
                        (UInt32) vulkanInstanceCreateInfo.ApplicationVersion.Minor,
                        (UInt32) vulkanInstanceCreateInfo.ApplicationVersion.Build),
                    ApplicationName = vulkanInstanceCreateInfo.ApplicationName,
                    EngineName = vulkanInstanceCreateInfo.EngineName,
                    EngineVersion = Vulkan.Version.Make((UInt32) vulkanInstanceCreateInfo.EngineVersion.Major,
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
                VulkanSurface = CreateSurface(vulkanInstanceCreateInfo.VulkanWindow);
                // Если требуется для отладки, включаем уровни проверки по умолчанию
                if (SettingsManager.IsDebugEnabled)
                {
                    // Указанные флаги отчетности определяют, какие сообщения для слоев следует отображать 
                    // Для проверки (отладки) приложения, битового флага Error и битового флага Warning, должно быть достаточно
                    // но я пока подключу все
                    const DebugReportFlagsExt debugReportFlags =
                        DebugReportFlagsExt.Error
                        | DebugReportFlagsExt.Warning
                        | DebugReportFlagsExt.PerformanceWarning
                        | DebugReportFlagsExt.Debug;
                    // Дополнительные битового флаги включают информацию о производительности, загрузчике и другие отладочные сообщения
                    VulkanDebug.SetupDebugging(Instance, debugReportFlags);
                }

                if (SettingsManager.IsDebugEnabled)
                {
                    var physicalDevices = GetPhysicalDevices();
                    Console.WriteLine("Информация по видеоадаптерам в системе");
                    for (var i = 0; i < physicalDevices.Count; i++)
                    {
                        var physicalDevice = physicalDevices[i];
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
            var deviceType = PhysicalDeviceType.Other;

            foreach (var rate in rates)
            {
                if (rate.Rate >= maxRate)
                {
                    maxRate = rate.Rate;
                    physicalDevice = rate.PhysicalDevice;
                    deviceType = rate.PhysicalDeviceType;
                }
            }

            if (deviceType != searchInfo.PreferredType)
            {
                // Попытаемся найти желаемый тип устройства с тем же рейтингом
                var preferred = rates.FirstOrDefault(e => e.PhysicalDeviceType == searchInfo.PreferredType &&
                                                          e.Rate == maxRate);
                if (preferred != null)
                {
                    return preferred.PhysicalDevice;
                }
            }

            return physicalDevice;
        }

        #endregion

        #endregion

        #region .private.sector

        #region .static

        private static UInt32 GetFeaturesRate(UInt32 deviceRate, PhysicalDeviceFeatures physicalDeviceFeatures,
            PhysicalDeviceFeatures requestedFeatures)
        {
            var rate = deviceRate;
            if (requestedFeatures.FillModeNonSolid)
            {
                if (physicalDeviceFeatures.FillModeNonSolid)
                {
                    rate++;
                }
                else
                {
                    return 0;
                }
            }
            if (requestedFeatures.GeometryShader)
            {
                if (physicalDeviceFeatures.GeometryShader)
                {
                    rate++;
                }
                else
                {
                    return 0;
                }
            }
            if (requestedFeatures.TessellationShader)
            {
                if (physicalDeviceFeatures.TessellationShader)
                {
                    rate++;
                }
                else
                {
                    return 0;
                }
            }
            return rate;
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

        #endregion

        #region .instance

        /// <summary>
        /// Определяет рейтинг устройства на основе поддерживаемой функциональности
        /// Чем выше рейтинг устройства, тем оно предпочтительнее для нас
        /// </summary>
        private IReadOnlyList<VulkanPhysicalDeviceRate> GetVulkanPhysicalDeviceRate(
            VulkanPhysicalDeviceSearchInfo searchInfo)
        {
            var rates = new List<VulkanPhysicalDeviceRate>();
            var physicalDevices = GetPhysicalDevices();
            foreach (var physicalDevice in physicalDevices)
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
                            deviceRate++;
                        }
                    }
                    if ((queueFamilyProperties.QueueFlags & QueueFlags.Compute) == QueueFlags.Compute)
                    {
                        supportCompute = true;
                        if (searchInfo.IsRequestedSupportComputeQueue)
                        {
                            deviceRate++;
                        }
                    }
                    if ((queueFamilyProperties.QueueFlags & QueueFlags.Transfer) == QueueFlags.Transfer)
                    {
                        supportTransfer = true;
                        if (searchInfo.IsRequestedSupportTransferQueue)
                        {
                            deviceRate++;
                        }
                    }
                    if (searchInfo.IsRequestedSupportPresentationQueue)
                    {
                        if (physicalDevice.GetSurfaceSupportKHR((UInt32) i, searchInfo.VulkanSurface.Surface))
                        {
                            supportPresentation = true;
                            deviceRate++;
                        }
                    }
                    if (searchInfo.RequestedExtensionNames != null && searchInfo.RequestedExtensionNames.Any())
                    {
                        var extensions = physicalDevice.EnumerateDeviceExtensionProperties();
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
                            deviceRate++;
                        }
                    }
                    else
                    {
                        supportRequestedExtensions = true;
                        deviceRate++;
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
                        deviceRate++;
                    }
                }

                // TODO: В процессе разработки пополнять необходимым, тем самым уточняя рейтинг устройств

                deviceRate = GetFeaturesRate(deviceRate, features, searchInfo.RequestedFeatures);

                if (deviceRate == 0)
                {
                    rates.Add(GetZeroPhysicalDeviceRate(physicalDevice, properties.DeviceType));
                    continue;
                }

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

        #endregion

        #endregion

        #region .fields

        private Boolean _isInit;

        #endregion

        #region .ctors

        public VulkanInstance()
        {
            EnabledInstanceExtensions = new List<ExtensionProperties>();
            EnabledInstanceLayers = new List<LayerProperties>();
        }

        #endregion
    }
}