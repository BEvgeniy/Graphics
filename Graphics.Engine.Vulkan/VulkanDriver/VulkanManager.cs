using System;
using System.Collections.Generic;
using System.Linq;
using Graphic.Engine.VulkanDriver;
using Graphics.Engine.Settings;
using Vulkan;

namespace Graphics.Engine.VulkanDriver
{
    internal static class VulkanManager
    {
        #region .fields

        // Флаг - определяет был ли создан экземпляр Vulkan
        private static Boolean _isVulkanInit;
        
        // Предустановленные расширения в системе, которые доступны для задания при создании экземпляра Vulkan 
        private static IReadOnlyList<Vulkan.ExtensionProperties> _vulkanAvailableInstanceExtensions;

        // Предустановленные слои в системе, экземпляра которые доступны для задания при создании экземпляра Vulkan 
        // TODO: (надо разобраться какие слои для чего нужны - очень слабое понимание на сегодняшний день)
        private static IReadOnlyList<Vulkan.LayerProperties> _vulkanAvailableInstanceLayers;

        // Расширения, которые подключены к созданному экземпляру Vulkan
        private static IReadOnlyList<String> _vulkanEnabledInstanceExtentionNames;

        // Слои, которые подключены к созданному экземпляру Vulkan
        // TODO: (надо разобраться для чего нужны слои - очень слабое понимание на сегодняшний день)
        private static IReadOnlyList<String> _vulkanEnabledInstanceLayerNames;

        // Обертка над Vulkan.Device и Vulkan.PhysicalDevice (выбранный видеоадаптер)
        // Vulkan.Device - это логическое устройство, 
        // и таких устройств можно создать много на основе одного Vulkan.PhysicalDevice,
        // при этом каждое такое логичесвое устройство может иметь разную доступную функциональность
        // Скажем так, пусть устройство Vulkan.PhysicalDevice имеет 2 фичи: VkBool32 geometryShader и VkBool32 tessellationShader;
        // Так вот мы можем создать два логических устройства, одно из которых будет поддерживать одно из фич, а второе оставшуюся фичу
        private static VulkanDevice _vulkanDevice;

        // Объект для синхронизации выполнения критических участков кода. 
        // В данном случае позволяется помеченный участок кода выполнять только одним потоком.
        // Если один поток выполняет помеченый участок кода, то другие в это время ожидают.
        private static readonly Object SyncObject = new Object();

        #endregion

        #region .props

        /// <summary>
        /// Экземпляр(объект или инстанс) Vulkan - хранит все состояния для текущего приложения
        /// Создается один раз при инициализации. 
        /// </summary>
        public static Vulkan.Instance VulkanInstance { get; private set; }

        /// <summary>
        /// Список видеоадаптеров доступных в системе (интегрированная или внешняя или тандем (sli))
        /// </summary>
        public static IReadOnlyList<Vulkan.PhysicalDevice> VulkanPhysicalDevices { get; private set; }

        /// <summary>
        /// Выбранный видеоадаптер. 
        /// Видеоадаптер который был выбран системой автоматически (рекомендуется), либо указанный в настройках.
        /// Видеоадаптер может быть задан явно, через указание в настройках, в случае, когда в системе имеется несколько видеоадаптеров и ведется разработка, 
        /// либо по какой-то причине выбранный системой видаоадаптер не устраивает или не отрабатывает как от него ожидают.
        /// </summary>
        public static Vulkan.PhysicalDevice VulkanPhysicalDevice { get; private set; }
        /// <summary>
        /// Логическое устройство для выбранного видеоадаптера. 
        /// </summary>
        public static Vulkan.Device VulkanLogicalDevice { get; private set; }

        #endregion

        static VulkanManager()
        {
            _isVulkanInit = false;
        }

        public static void Init()
        {
            if (_isVulkanInit) return;

            lock (SyncObject)
            {
                if (_isVulkanInit) return;

                // Создадим экземпляр Vulkan
                CreateInstance();
                // Если требуется для отладки, включаем уровни проверки по умолчанию
                if (SettingsManager.IsDebugEnabled)
                {
                    // Указанные флаги отчетности определяют, какие сообщения для слоев следует отображать 
                    // Для проверки (отладки) приложения, битового флага Error и битового флага Warning, должно быть достаточно
                    const Vulkan.DebugReportFlagsExt debugReportFlags =
                        Vulkan.DebugReportFlagsExt.Error | Vulkan.DebugReportFlagsExt.Warning;
                    // Дополнительные битового флаги включают информацию о производительности, загрузчике и другие отладочные сообщения
                    VulkanDebug.SetupDebugging(VulkanInstance, debugReportFlags);
                }
                // Получаем все физические устройства (здесь и далее видеоадаптеры) доступные в системе
                GetPhysicalDevices();
                // Создадим логическое устройство связанное с видеоадаптером
                CreateLogicalDevice();
            }
        }

        private static void CreateInstance()
        {
            // Действие 1: Получим расширения и слои, которые доступны для использования при создании экземпляра Vulkan

            var availInstExtensions = new List<Vulkan.ExtensionProperties>();
            var availInstLayers = new List<Vulkan.LayerProperties>();

            var availableInstanceExtensions = Vulkan.Commands.EnumerateInstanceExtensionProperties();

            if (availableInstanceExtensions != null && availableInstanceExtensions.Length > 0)
            {
                availInstExtensions.AddRange(availableInstanceExtensions);
                _vulkanAvailableInstanceExtensions = availInstExtensions;
            }

            var availableInstanceLayers = Vulkan.Commands.EnumerateInstanceLayerProperties();

            if (availableInstanceLayers != null && availableInstanceLayers.Length > 0)
            {
                availInstLayers.AddRange(availableInstanceLayers);
                _vulkanAvailableInstanceLayers = availInstLayers;
            }

            // Действие 2: Так как мы собираемся рендерить на экран, то надо подключить расширения WSI (Window System Integration)

            var vulkanEnabledInstExtNames = new List<String>
            {
                "VK_KHR_surface",
                "VK_KHR_win32_surface"
            };
            
            // Действие 3: Если собираемся отлаживаться и проходить валидацию по слоям, надо подключить расширение отладки и слои валидации

            if (SettingsManager.IsDebugEnabled)
            {
                vulkanEnabledInstExtNames.Add("VK_EXT_debug_report");
                //_vulkanEnabledInstanceLayerNames = new List<String> { "VK_LAYER_LUNARG_standard_validation" };
                var vulkanEnabledInstLayerNames = new List<String>();

                foreach (var layer in _vulkanAvailableInstanceLayers)
                {
                    if (
                        layer.LayerName == "VK_LAYER_LUNARG_vktrace"
                        || layer.LayerName == "VK_LAYER_LUNARG_api_dump"
                        // || layer.LayerName == "VK_LAYER_GOOGLE_threading" 
                        // || layer.LayerName == "VK_LAYER_GOOGLE_unique_objects" 
                        // ||layer.LayerName == "VK_LAYER_VALVE_steam_overlay"
                        )
                    {
                        continue;
                    }

                    vulkanEnabledInstLayerNames.Add(layer.LayerName);
                }

                _vulkanEnabledInstanceLayerNames = vulkanEnabledInstLayerNames;
            }

            _vulkanEnabledInstanceExtentionNames = vulkanEnabledInstExtNames;

            // Действие 4: Заполним необходимые параметры для создания экземпляра Vulkan

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
                EnabledExtensionNames = _vulkanEnabledInstanceExtentionNames.ToArray(),
                EnabledExtensionCount = (UInt32) _vulkanEnabledInstanceExtentionNames.Count,
            };

            if (SettingsManager.IsDebugEnabled)
            {
                createInfo.EnabledLayerCount = (UInt32) _vulkanEnabledInstanceLayerNames.Count;
                createInfo.EnabledLayerNames = _vulkanEnabledInstanceLayerNames.ToArray();
            }

            // Действие 5: Создадим экземпляр Vulkan

            VulkanInstance = new Vulkan.Instance(createInfo);
        }

        private static void GetPhysicalDevices()
        {
            var physicalDevices = VulkanInstance.EnumeratePhysicalDevices();
            if (physicalDevices.Length <= 0)
            {
                throw new Exception("В системе не установлен подходящий видеоадаптер для вывода изображения на экран");
            }
            VulkanPhysicalDevices = new List<PhysicalDevice>(physicalDevices);
            Console.WriteLine("Информация по видеоадаптерам в системе");
            for (var i = 0; i < VulkanPhysicalDevices.Count; i++)
            {
                var physicalDevice = VulkanPhysicalDevices[i];
                var deviceProperties = physicalDevice.GetProperties();
                Console.WriteLine("Название видеоадаптера [" + i + "] : " + deviceProperties.DeviceName);
                Console.WriteLine("Тип видеоадаптера: " + VulkanTools.PhysicalDeviceTypeString(deviceProperties.DeviceType));
                Console.WriteLine("Версия графического API Vulkan поддерживаемая видеоадаптером: " + (deviceProperties.ApiVersion >> 22) + "." +
                                  ((deviceProperties.ApiVersion >> 12) & 0x3ff) + "." +
                                  (deviceProperties.ApiVersion & 0xfff));
            }
        }

        private static void CreateLogicalDevice()
        {
            var requestedFeatures = new Vulkan.PhysicalDeviceFeatures();
            var requestedExtensions = new List<String>();
            var useSwapChain = true;
            var requestedQueueTypes = Vulkan.QueueFlags.Graphics | Vulkan.QueueFlags.Compute;
            // TODO: Выбираю по умолчанию первый, но в реальности надо анализировать какой выбирать (приоритет 'внешний видеоадаптер')
            _vulkanDevice = new VulkanDevice(VulkanPhysicalDevices[0], requestedFeatures, 
                requestedExtensions, useSwapChain, requestedQueueTypes);
        }


    }
}