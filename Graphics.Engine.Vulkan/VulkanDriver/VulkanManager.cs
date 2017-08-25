using System;
using System.Collections.Generic;
using Graphics.Engine.Settings;
using VulkanSharp;

namespace Graphics.Engine.VulkanDriver
{
    internal class VulkanManager
    {
        #region .fields
        /// <summary>
        /// Флаг - определяет был ли создан экземпляр Vulkan
        /// </summary>
        private Boolean _isVulkanInit;
        
        /// <summary>
        /// Обертка над Device и PhysicalDevice (выбранный видеоадаптер)
        /// Device - это логическое устройство, 
        /// и таких устройств можно создать много на основе одного PhysicalDevice,
        /// при этом каждое такое логичесвое устройство может иметь разную доступную функциональность
        /// Скажем так, пусть устройство PhysicalDevice имеет 2 фичи: VkBool32 geometryShader и VkBool32 tessellationShader;
        /// Так вот мы можем создать два логических устройства, одно из которых будет поддерживать одно из фич, а второе оставшуюся фичу
        /// </summary>
        private VulkanDevice _vulkanDevice;
        /// <summary>
        /// Объект для синхронизации выполнения критических участков кода. 
        /// В данном случае, позволяется, помеченный участок кода выполнять только одним потоком.
        /// Если один поток выполняет помеченый участок кода, то другие в это время ожидают.
        /// </summary>
        private readonly Object SyncObject = new Object();

        #endregion

        #region .props

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
        public VulkanPhysicalDevice VulkanPhysicalDevice { get; private set; }
        
        /// <summary>
        /// Логическое устройство для выбранного видеоадаптера. 
        /// </summary>
        public VulkanLogicalDevice VulkanLogicalDevice { get; private set; }

        /// <summary>
        /// Поверхности отрисовки Vulkan. 
        /// </summary>
        public VulkanSurface VulkanSurface { get; private set; }

        #endregion

        public VulkanManager()
        {
            _isVulkanInit = false;
        }

        public void Init(INativeWindow vulkanWindow)
        {
            if (_isVulkanInit) return;

            lock (SyncObject)
            {
                if (_isVulkanInit) return;
                // Создадим экземпляр Vulkan
                CreateInstance();
                // Создадим поверхность
                CreateSurface(vulkanWindow);
                // Выбираем наилучшее для нас устройство
                CreatePhysicalDevice();
                // Создадим логическое устройство связанное с видеоадаптером
                CreateLogicalDevice();
            }
        }

        private void CreateSurface(INativeWindow vulkanWindow)
        {
            VulkanSurface = new VulkanSurface(VulkanInstance, vulkanWindow);
            VulkanSurface.Create();
        }

        #region private sector

        private void CreateInstance()
        {
            VulkanInstance = new VulkanInstance();
            // Так как мы собираемся рендерить в окно, то надо подключить расширения WSI (Window System Integration)
            var requestedLayers = new List<LayerProperties>();
            var requestedExtentions = new List<ExtensionProperties>();

            var extentionNames = new List<String>
            {
                "VK_KHR_surface",
                "VK_KHR_win32_surface"
            };

            if (SettingsManager.IsDebugEnabled)
            {
                extentionNames.Add("VK_EXT_debug_report");
            }

            foreach (var name in extentionNames)
            {
                var extention = VulkanInstance.GetExtensionPropertiesByName(name);
                if (extention == null)
                {
                    throw new Exception("Среди доступных расширений в системе, не обнаружено расширение с именем '" + name + "'");
                }
                requestedExtentions.Add(extention);
            }
          
            // Если собираемся отлаживаться и проходить валидацию по слоям, надо подключить расширение отладки и слои валидации
            if (SettingsManager.IsDebugEnabled)
            {  
                foreach (var layer in VulkanInstance.VulkanAvailableInstanceLayers)
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
                    requestedLayers.Add(layer);
                }
            }
            VulkanInstance.Create(requestedExtentions, requestedLayers);
        }

        private void CreatePhysicalDevice()
        {
            // Возможно стоит передавать параметры для поиска подходящего видеоадаптера
            var vulkanPhysicalDevice = VulkanInstance.GetVulkanPhysicalDevice();
            if (vulkanPhysicalDevice == null)
            {
                throw new Exception("Не найден подходящий видеоадаптер для работы с приложением.");
            }
            VulkanPhysicalDevice = new VulkanPhysicalDevice(VulkanInstance, vulkanPhysicalDevice);
            VulkanPhysicalDevice.Create();
        }

        private void CreateLogicalDevice()
        {
            var requestedFeatures = new PhysicalDeviceFeatures();
            var requestedExtentions = new List<ExtensionProperties>();
            var extentionNames = new List<String>
            {
                "VK_KHR_swapchain",
                "VK_KHR_sampler_mirror_clamp_to_edge"
            };

            //var requestedQueueTypes = QueueFlags.Graphics | QueueFlags.Compute;
            //_vulkanDevice = new VulkanDevice(VulkanPhysicalDevice, requestedFeatures,
            //    requestedExtensions, useSwapChain, requestedQueueTypes);
            
            if (SettingsManager.IsDebugEnabled)
            {
                extentionNames.Add("VK_EXT_debug_marker");
            }

            foreach (var name in extentionNames)
            {
                var extention = VulkanPhysicalDevice.GetExtensionPropertiesByName(name);
                if (extention == null)
                {
                    throw new Exception("Среди доступных расширений видеоадаптера не обнаружено расширение с именем '" + name + "'");
                }
                requestedExtentions.Add(extention);
            }

            VulkanLogicalDevice = new VulkanLogicalDevice(VulkanPhysicalDevice);
            VulkanLogicalDevice.Create(requestedFeatures, requestedExtentions);
        }

        #endregion

    }
}