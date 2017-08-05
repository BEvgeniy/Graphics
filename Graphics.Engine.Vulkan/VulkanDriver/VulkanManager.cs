using System;
using System.Collections.Generic;

namespace Graphic.Engine.VulkanDriver
{
    internal static class VulkanManager
    {
        #region .fields

        // Разрешена отладка true, иначе false
        private static Boolean _isDebugAndValidation;

        // Версия API Vulkan
        private static UInt32 _vulkanApiVersion;

        // Имя приложения 
        private static String _applicationName;

        // Версия приложения
        private static UInt32 _applicationVersion;

        // Имя движка 
        private static String _engineName;

        // Версия движка
        private static UInt32 _engineVersion;

        // Флаг - определяет был ли создан экземпляр Vulkan
        private static Boolean _isVulkanInit;

        // Экземпляр Vulkan - хранит все состояния для текущего приложения
        // Может быть создан только 1 раз
        private static Vulkan.Instance _vulkanInstance;

        // Расширения экземпляра
        private static List<String> _vulkanEnabledInstanceExtentionNames;

        // Слои экземпляра TODO: (надо разобраться для чего нужны слои - очень слабое понимание на сегодняшний день)
        private static List<String> _vulkanEnabledInstanceLayerNames;

        // Список видеокарт доступных в системе (интегрированная или внешняя или тандем (sli))
        private static List<Vulkan.PhysicalDevice> _vulkanPhysicalDevices;

        // Обертка над Vulkan.Device и Vulkan.PhysicalDevice (выбранный видеоадаптер)
        // Vulkan.Device - это логическое устройство, 
        // и таких устройств можно создать много на основе одного Vulkan.PhysicalDevice,
        // при этом каждое такое логичесвое устройство может иметь разную доступную функциональность
        // Скажем так, пусть устройство Vulkan.PhysicalDevice имеет 2 фичи: VkBool32 geometryShader и VkBool32 tessellationShader;
        // Так вот мы можем создать два логических устройства, одно из которых будет поддерживать одно из фич, а второе оставшуюся фичу
        private static VulkanDevice _vulkanDevice;

        // Объект для блокировки критических участков кода
        private static readonly Object Locker;

        #endregion

        static VulkanManager()
        {
            Locker = new Object();
            _isVulkanInit = false;
            _isDebugAndValidation = true;
            _vulkanApiVersion = Vulkan.Version.Make(1, 0, 0);
            _applicationName = "Atlas";
            _applicationVersion = Vulkan.Version.Make(1, 0, 0);
            _engineName = "Atlas Engine";
            _engineVersion = Vulkan.Version.Make(1, 0, 0);
            _vulkanEnabledInstanceExtentionNames = new List<String>
            {
                "VK_KHR_surface",
                "VK_KHR_win32_surface"
            };
            if (!_isDebugAndValidation) return;
            _vulkanEnabledInstanceExtentionNames.Add("VK_EXT_debug_report");
            _vulkanEnabledInstanceLayerNames = new List<String> {"VK_LAYER_LUNARG_standard_validation"};
            _vulkanPhysicalDevices = new List<Vulkan.PhysicalDevice>();
        }

        public static void Init()
        {
            if (_isVulkanInit) return;
            lock (Locker)
            {
                if (_isVulkanInit) return;
                // Создадим экземпляр Vulkan
                CreateInstance();
                // Если требуется для отладки, включаем уровни проверки по умолчанию
                if (_isDebugAndValidation)
                {
                    // Указанные флаги отчетности определяют, какие сообщения для слоев следует отображать 
                    // Для проверки (отладки) приложения, битового флага ошибка и битового флага предупреждение, должно быть достаточно
                    const Vulkan.DebugReportFlagsExt debugReportFlags =
                        Vulkan.DebugReportFlagsExt.Error | Vulkan.DebugReportFlagsExt.Warning;
                    // Дополнительные битового флаги включают информацию о производительности, загрузчик и другие отладочные сообщения
                    VulkanDebug.SetupDebugging(_vulkanInstance, debugReportFlags);
                }
                // Получаем все видеоадаптеры доступные в системе
                GetPhysicalDevices();
                // Создадим логическое устройство связанное с видеоадаптером
                CreateLogicalDevice();
            }
        }

        private static void CreateInstance()
        {
            var appInfo = new Vulkan.ApplicationInfo
            {
                ApiVersion = _vulkanApiVersion,
                ApplicationVersion = _applicationVersion,
                ApplicationName = _applicationName,
                EngineName = _engineName,
                EngineVersion = _engineVersion
            };
            var createInfo = new Vulkan.InstanceCreateInfo
            {
                ApplicationInfo = appInfo,
                EnabledExtensionNames = _vulkanEnabledInstanceExtentionNames.ToArray(),
                EnabledExtensionCount = (UInt32) _vulkanEnabledInstanceExtentionNames.Count,
            };
            if (_isDebugAndValidation)
            {
                createInfo.EnabledLayerCount = (UInt32) _vulkanEnabledInstanceLayerNames.Count;
                createInfo.EnabledLayerNames = _vulkanEnabledInstanceLayerNames.ToArray();
            }
            _vulkanInstance = new Vulkan.Instance(createInfo);
        }

        private static void GetPhysicalDevices()
        {
            var physicalDevices = _vulkanInstance.EnumeratePhysicalDevices();
            if (physicalDevices.Length <= 0)
            {
                throw new Exception("В системе не установлено подходящее устройство для вывода изображения на экран");
            }
            _vulkanPhysicalDevices.AddRange(physicalDevices);
            for (var i = 0; i < _vulkanPhysicalDevices.Count; i++)
            {
                var physicalDevice = _vulkanPhysicalDevices[i];
                var deviceProperties = physicalDevice.GetProperties();
                Console.WriteLine("Название видеоадаптера [" + i + "] : " + deviceProperties.DeviceName);
                Console.WriteLine("Тип видеоадаптера: " + VulkanTools.PhysicalDeviceTypeString(deviceProperties.DeviceType));
                Console.WriteLine("Версия графического API Vulkan: " + (deviceProperties.ApiVersion >> 22) + "." +
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
            _vulkanDevice = new VulkanDevice(_vulkanPhysicalDevices[0], requestedFeatures, 
                requestedExtensions, useSwapChain, requestedQueueTypes);
        }


    }
}