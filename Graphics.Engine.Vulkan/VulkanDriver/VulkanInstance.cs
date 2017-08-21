using System;
using System.Collections.Generic;
using System.Linq;
using Graphic.Engine.VulkanDriver;
using Graphics.Engine.Settings;

namespace Graphics.Engine.VulkanDriver
{
    /// <summary>
    /// Создает объект-обертку над экземпляром Vulkan
    /// </summary>
    internal sealed class VulkanInstance
    {
        public VulkanInstance()
        {
            VulkanAvailableInstanceExtensions = new List<Vulkan.ExtensionProperties>();
            VulkanAvailableInstanceLayers = new List<Vulkan.LayerProperties>();
            VulkanPhysicalDevices = new List<Vulkan.PhysicalDevice>();
            VulkanEnabledInstanceExtentions = new List<Vulkan.ExtensionProperties>();
            VulkanEnabledInstanceLayers = new List<Vulkan.LayerProperties>();

            // Получим расширения и слои, которые доступны для использования при создании экземпляра Vulkan

            var availInstExtensions = new List<Vulkan.ExtensionProperties>();
            var availInstLayers = new List<Vulkan.LayerProperties>();

            var availableInstanceExtensions = Vulkan.Commands.EnumerateInstanceExtensionProperties();

            if (availableInstanceExtensions != null && availableInstanceExtensions.Length > 0)
            {
                availInstExtensions.AddRange(availableInstanceExtensions);
                VulkanAvailableInstanceExtensions = availInstExtensions;
            }

            var availableInstanceLayers = Vulkan.Commands.EnumerateInstanceLayerProperties();

            if (availableInstanceLayers != null && availableInstanceLayers.Length > 0)
            {
                availInstLayers.AddRange(availableInstanceLayers);
                VulkanAvailableInstanceLayers = availInstLayers;
            }
        }

        /// <summary>
        /// Предустановленные расширения в системе, которые доступны для задания при создании экземпляра Vulkan 
        /// </summary>
        public IReadOnlyList<Vulkan.ExtensionProperties> VulkanAvailableInstanceExtensions { get; }

        /// <summary>
        /// Предустановленные слои в системе, экземпляра которые доступны для задания при создании экземпляра Vulkan 
        /// </summary>
        /// TODO: (надо разобраться какие слои для чего нужны - очень слабое понимание на сегодняшний день)
        public IReadOnlyList<Vulkan.LayerProperties> VulkanAvailableInstanceLayers { get; }

        /// <summary>
        /// Экземпляр(объект или инстанс) Vulkan - хранит все состояния для текущего приложения
        /// Создается один раз при инициализации. 
        /// </summary>
        public Vulkan.Instance Instance { get; private set; }

        /// <summary>
        /// Названия расширений, которые подключены к созданному экземпляру Vulkan
        /// </summary>
        public IReadOnlyList<Vulkan.ExtensionProperties> VulkanEnabledInstanceExtentions { get; private set; }

        /// <summary>
        /// Названия слоев, которые подключены к созданному экземпляру Vulkan
        /// </summary>
        public IReadOnlyList<Vulkan.LayerProperties> VulkanEnabledInstanceLayers { get; private set; }

        /// <summary>
        /// Список видеоадаптеров доступных в системе (интегрированная или внешняя или тандем (sli))
        /// </summary>
        public IReadOnlyList<Vulkan.PhysicalDevice> VulkanPhysicalDevices { get; private set; }

        public Vulkan.ExtensionProperties GetExtensionPropertiesByName(String extentionName)
        {
            return VulkanAvailableInstanceExtensions.FirstOrDefault(e=> e.ExtensionName == extentionName);
        }

        public Vulkan.LayerProperties GetLayerPropertiesByName(String layerName)
        {
            return VulkanAvailableInstanceLayers.FirstOrDefault(e => e.LayerName == layerName);
        }

        public void Create(List<Vulkan.ExtensionProperties> requestedExtentions, List<Vulkan.LayerProperties> requestedLayers)
        {         
            VulkanEnabledInstanceExtentions = requestedExtentions;
            VulkanEnabledInstanceLayers = requestedLayers;
            // Заполним необходимые параметры для создания экземпляра Vulkan
            var appInfo = new Vulkan.ApplicationInfo
            {
                ApiVersion = SettingsManager.VulkanApiVersion,
                ApplicationVersion = SettingsManager.ApplicationVersion,
                ApplicationName = SettingsManager.ApplicationName,
                EngineName = SettingsManager.EngineName,
                EngineVersion = SettingsManager.EngineVersion
            };

            var createInfo = new Vulkan.InstanceCreateInfo
            {
                ApplicationInfo = appInfo,
                EnabledExtensionCount = (UInt32)VulkanEnabledInstanceExtentions.Count,
                EnabledExtensionNames = VulkanEnabledInstanceExtentions.Select(e => e.ExtensionName).ToArray(),
            };

            if (SettingsManager.IsDebugEnabled)
            {
                createInfo.EnabledLayerCount = (UInt32)VulkanEnabledInstanceLayers.Count;
                createInfo.EnabledLayerNames = VulkanEnabledInstanceLayers.Select(e => e.LayerName).ToArray();
            }
            // Создадим экземпляр Vulkan
            Instance = new Vulkan.Instance(createInfo);
            // Если требуется для отладки, включаем уровни проверки по умолчанию
            if (SettingsManager.IsDebugEnabled)
            {
                // Указанные флаги отчетности определяют, какие сообщения для слоев следует отображать 
                // Для проверки (отладки) приложения, битового флага Error и битового флага Warning, должно быть достаточно
                const Vulkan.DebugReportFlagsExt debugReportFlags =
                    Vulkan.DebugReportFlagsExt.Error
                    | Vulkan.DebugReportFlagsExt.Warning
                    | Vulkan.DebugReportFlagsExt.PerformanceWarning
                    | Vulkan.DebugReportFlagsExt.Debug;
                // Дополнительные битового флаги включают информацию о производительности, загрузчике и другие отладочные сообщения
                VulkanDebug.SetupDebugging(Instance, debugReportFlags);
            }
            var physicalDevices = Instance.EnumeratePhysicalDevices();
            if (physicalDevices == null || physicalDevices.Length <= 0)
            {
                throw new Exception("В системе не установлен подходящий видеоадаптер поддерживающий работу с Vulkan");
            }

            VulkanPhysicalDevices = new List<Vulkan.PhysicalDevice>(physicalDevices);
        
            if (SettingsManager.IsDebugEnabled)
            {
                Console.WriteLine("Информация по видеоадаптерам в системе");
                for (var i = 0; i < VulkanPhysicalDevices.Count; i++)
                {
                    var physicalDevice = VulkanPhysicalDevices[i];
                    var deviceProperties = physicalDevice.GetProperties();
                    Console.WriteLine("[" + i + "] Название видеоадаптера: " + deviceProperties.DeviceName);
                    Console.WriteLine("[" + i + "] Тип видеоадаптера: " +
                                      VulkanTools.PhysicalDeviceTypeString(deviceProperties.DeviceType));
                    Console.WriteLine("[" + i + "] Версия графического API Vulkan поддерживаемая видеоадаптером: " +
                                      VulkanTools.GetVersionAsString(deviceProperties.ApiVersion));
                }
            }
        }

        /// <summary>
        /// Поиск наилучшего видеоадаптера для работы с нашим приложением
        /// </summary>
        public Vulkan.PhysicalDevice GetVulkanPhysicalDevice()
        {
            var rates = GetVulkanPhysicalDeviceRate();
            Vulkan.PhysicalDevice physicalDevice = null;
            var maxRate = 0U;
            foreach (var rate in rates)
            {
                if (rate.Value > maxRate)
                {
                    maxRate = rate.Value;
                    physicalDevice = rate.Key;
                }
            }

            return physicalDevice;
        }

        /// <summary>
        /// Определяет рейтинг устройства на основе поддерживаемой функциональности
        /// Чем выше рейтинг устройства, тем оно предпочтительнее для нас
        /// </summary>
        private IReadOnlyDictionary<Vulkan.PhysicalDevice, UInt32> GetVulkanPhysicalDeviceRate()
        {
            var rates = new Dictionary<Vulkan.PhysicalDevice, UInt32>();

            foreach (var physicalDevice in VulkanPhysicalDevices)
            {
                var deviceRate = 0U;
                var deviceProperties = physicalDevice.GetProperties();
                var deviceFeatures = physicalDevice.GetFeatures();
                //var deviceMemory = physicalDevice.GetMemoryProperties();
                //UInt64 deviceMemoryHeapSize = deviceMemory.MemoryHeaps[0].Size;

                switch (deviceProperties.DeviceType)
                {
                    case Vulkan.PhysicalDeviceType.Other:
                        // Тип устройства не известен и нам не интересен
                        rates.Add(physicalDevice, deviceRate);
                        continue;
                    case Vulkan.PhysicalDeviceType.DiscreteGpu:
                        deviceRate = 10000000U;
                        break;
                    case Vulkan.PhysicalDeviceType.IntegratedGpu:
                        deviceRate = 100000U;
                        break;
                    case Vulkan.PhysicalDeviceType.VirtualGpu:
                        // Тип устройства пока нам не интересен
                        rates.Add(physicalDevice, deviceRate);
                        continue;
                    case Vulkan.PhysicalDeviceType.Cpu:
                        // Тип устройства пока нам не интересен
                        rates.Add(physicalDevice, deviceRate);
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // Для нас обязательно должны поддержиться и геометричейский шейдер и шейдер тесселяции
                if (!deviceFeatures.GeometryShader || !deviceFeatures.TessellationShader)
                {
                    deviceRate = 0;
                    rates.Add(physicalDevice, deviceRate);
                    continue;
                }

                var extensions = physicalDevice.EnumerateDeviceExtensionProperties();
                if (extensions != null && extensions.Length > 0)
                {
                    var supportSwapchain = false;
                    foreach (var extensionProperties in extensions)
                    {
                        if (extensionProperties.ExtensionName == "VK_KHR_swapchain")
                        {
                            supportSwapchain = true;
                            break;
                        }
                    }
                    // Для нас обязательно должна быть поддержка возможности вывода на экран физическим устройством
                    if (!supportSwapchain)
                    {
                        deviceRate = 0;
                        rates.Add(physicalDevice, deviceRate);
                        continue;
                    }
                    deviceRate += 1000;
                }

                var queues = physicalDevice.GetQueueFamilyProperties();
                if (queues != null && queues.Length > 0)
                {
                    var supportGraphics = false;
                    var supportCompute = false;
                    var supportTransfer = false;
                    foreach (var queueFamilyProperties in queues)
                    {
                        if ((queueFamilyProperties.QueueFlags & Vulkan.QueueFlags.Graphics) == Vulkan.QueueFlags.Graphics)
                        {
                            supportGraphics = true;
                        }
                        if ((queueFamilyProperties.QueueFlags & Vulkan.QueueFlags.Compute) == Vulkan.QueueFlags.Compute)
                        {
                            supportCompute = true;
                        }
                        if ((queueFamilyProperties.QueueFlags & Vulkan.QueueFlags.Transfer) == Vulkan.QueueFlags.Transfer)
                        {
                            supportTransfer = true;
                        }
                    }
                    // Для нас обязательно должна быть поддержка возможностей:
                    // 1. Очередь, которая коддерживает графические команды
                    // 2. Очередь, которая коддерживает команды вычислений
                    // 3. Очередь, которая коддерживает команды передачи данных
                    if (!supportGraphics || !supportCompute || !supportTransfer)
                    {
                        deviceRate = 0;
                        rates.Add(physicalDevice, deviceRate);
                        continue;
                    }
                    deviceRate += 2000;
                }

                // Максимально возможный размер текстур влияет на качество графики
                deviceRate += deviceProperties.Limits.MaxImageDimension2D;

                // TODO: В процессе разработки пополнять необходимым, тем самым уточняя рейтинг устройств

                rates.Add(physicalDevice, deviceRate);
            }

            return rates;
        }
    }
}
