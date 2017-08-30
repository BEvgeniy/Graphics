using System;
using System.Collections.Generic;
using Graphics.Engine.Settings;
using Graphics.Engine.VulkanDriver.VkDevice.Logical;
using Graphics.Engine.VulkanDriver.VkDevice.Physical;
using Graphics.Engine.VulkanDriver.VkInstance;
using Graphics.Engine.VulkanDriver.VkSurface;
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

        #endregion

        public VulkanManager()
        {
            _isVulkanInit = false;
        }

        public void Init(INativeWindow vulkanMainWindow)
        {
            if (_isVulkanInit) return;

            lock (SyncObject)
            {
                if (_isVulkanInit) return;
                // Создадим экземпляр Vulkan
                CreateInstance(vulkanMainWindow);
                // Выбираем наилучшее для нас устройство
                CreatePhysicalDevice();
                // Создадим логическое устройство связанное с видеоадаптером
                CreateLogicalDevice();
            }
        }

        #region private sector

        private void CreateInstance(INativeWindow vulkanMainWindow)
        {
            VulkanInstance = new VulkanInstance();

            var createInfo = new VulkanInstanceCreateInfo
            {
                IsDebugEnabled = SettingsManager.IsDebugEnabled,
                VulkanApiVersion = SettingsManager.VulkanApiVersion,
                ApplicationName = SettingsManager.ApplicationName,
                EngineName = SettingsManager.EngineName,
                ApplicationVersion = SettingsManager.ApplicationVersion,
                EngineVersion = SettingsManager.EngineVersion,
                RequestedExtensionNames = SettingsManager.RequestedInstanceExtensionNames,
                RequestedLayerNames = SettingsManager.RequestedInstanceLayerNames,
                VulkanWindow = vulkanMainWindow
            };

            VulkanInstance.Create(createInfo);
        }

        private void CreatePhysicalDevice()
        {
            var searchInfo = new VulkanPhysicalDeviceSearchInfo
            {
                IsRequestedSupportGraphicsQueue = true,
                IsRequestedSupportPresentationQueue = true,
                VulkanSurface = VulkanInstance.VulkanSurface,
                IsRequestedSupportTransferQueue = false, // пока нет нужды
                IsRequestedSupportComputeQueue = false, // пока нет нужды
                RequestedFeatures = new PhysicalDeviceFeatures(), // пока все false
                PreferredType = PhysicalDeviceType.DiscreteGpu,
                PreferredVulkanApiVersion = SettingsManager.VulkanApiVersion,
                RequestedExtensionNames = SettingsManager.RequestedPhysicalDeviceExtensionNames
            };

            var foundPhysicalDevice = VulkanInstance.FindSuitablePhysicalDevice(searchInfo);
            if (foundPhysicalDevice == null)
            {
                throw new Exception("Не найден подходящий видеоадаптер для работы с приложением.");
            }

            var createInfo = new VulkanPhysicalDeviceCreateInfo
            {
                VulkanInstance = VulkanInstance,
                VulkanSurface = VulkanInstance.VulkanSurface,
                PhysicalDevice = foundPhysicalDevice
            };

            VulkanPhysicalDevice = new VulkanPhysicalDevice();
            VulkanPhysicalDevice.Create(createInfo);
        }

        private void CreateLogicalDevice()
        {
            var createInfo = new VulkanLogicalDeviceCreateInfo
            {
                VulkanPhysicalDevice = VulkanPhysicalDevice,
                VulkanSurface = VulkanInstance.VulkanSurface,
                RequestedFeatures = new PhysicalDeviceFeatures(), // пока все false
                RequestedExtensionNames = SettingsManager.RequestedLogicalDeviceExtensionNames,
                IsRequestedCreateGraphicsQueue = true,
                IsRequestedCreateComputeQueue = true,
                IsRequestedCreateTransferQueue = true
            };

            VulkanLogicalDevice = new VulkanLogicalDevice();
            VulkanLogicalDevice.Create(createInfo);
        }

        #endregion

    }
}