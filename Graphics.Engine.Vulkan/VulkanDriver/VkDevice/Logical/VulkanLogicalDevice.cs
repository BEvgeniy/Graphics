using System;
using System.Collections.Generic;
using System.Linq;
using Graphics.Engine.VulkanDriver.VkDevice.Physical;
using VulkanSharp;

namespace Graphics.Engine.VulkanDriver.VkDevice.Logical
{
    /// <summary>
    /// Создает объект-обертку над логическим устройством (видеоадаптером).
    /// </summary>
    internal sealed class VulkanLogicalDevice
    {
        private Boolean _isInit;

        public VulkanLogicalDevice()
        {
            VulkanEnabledLogicalDeviceExtensions = new List<ExtensionProperties>();
        }

        /// <summary>
        /// Видеоадаптер, для которого было создано логическое устройство (помещенное в объект обертку).
        /// </summary>
        public VulkanPhysicalDevice VulkanPhysicalDevice { get; private set; }

        /// <summary>
        /// Логическое устройство созданное поверх физического (видеоадаптер). 
        /// При создании логического устройства указываются необходимые пользователю расширения и другие требуемые возможности, 
        /// которые доступны у физического устройства. При создании логического устройства не обязательно указывать все-возможные 
        /// свойства (фичи), которые предлагает нам физическое устройство. Поэтому мы можем создать несколько логических устройств, у которых
        /// для создания использовался один и тот же видеоадаптер.
        /// </summary>
        public Device Device { get; private set; }

        /// <summary>
        /// Очередь поддерживающая работу с графическими командами
        /// </summary>
        public Queue GraphicsQueue { get; private set; }

        /// <summary>
        /// Очередь поддерживающая вывод изображения на экран
        /// Идеальный вариант, когда <see cref="GraphicsQueue"/> и эта очередь
        /// являются одним и тем же семейством очередей с одним и тем же индексом
        /// </summary>
        public Queue PresentQueue { get; private set; }

        /// <summary>
        /// Очередь поддерживающая работу с командами вычислений
        /// </summary>
        public Queue ComputeQueue { get; private set; }

        /// <summary>
        /// Очередь поддерживающая работу с командами работы с памятью
        /// </summary>
        public Queue TransferQueue { get; private set; }

        /// <summary>
        /// Пул команд для семейства очередей поддерживающих работу с графическими командами (по умолчанию)
        /// </summary>
        public CommandPool GraphicsCommandPool { get; private set; }

        /// <summary>
        /// Пул команд для семейства очередей поддерживающих работу с командами вычислений (по умолчанию)
        /// </summary>
        public CommandPool ComputeCommandPool { get; private set; }

        /// <summary>
        /// Пул команд для семейства очередей поддерживающих работу с командами работы с памятью (по умолчанию)
        /// </summary>
        public CommandPool TransferCommandPool { get; private set; }

        /// <summary>
        /// Названия расширений, которые подключены к созданному логическому устройству.
        /// </summary>
        public IReadOnlyList<ExtensionProperties> VulkanEnabledLogicalDeviceExtensions { get; private set; }

        /// <summary>
        /// Возможности физического устройства, которые используются логическим устройством.
        /// </summary>
        public PhysicalDeviceFeatures VulkanEnabledLogicalDeviceFeatures { get; private set; }

        /// <summary>
        /// Создает логическое устройство с указанными пользователем расширениями и свойствами (фичами).
        /// </summary>
        public void Create(VulkanLogicalDeviceCreateInfo vulkanLogicalDeviceCreateInfo)
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

                VulkanPhysicalDevice = vulkanLogicalDeviceCreateInfo.VulkanPhysicalDevice;

                var extensions = new List<ExtensionProperties>();
                foreach (var name in vulkanLogicalDeviceCreateInfo.RequestedExtensionNames)
                {
                    var extension = VulkanPhysicalDevice.GetExtensionPropertiesByName(name);
                    if (extension == null)
                    {
                        throw new Exception(
                            "Среди доступных расширений поддерживаемых физическим устройством, не обнаружено запрошенное расширение с именем '" +
                            name + "'");
                    }
                    extensions.Add(extension);
                }

                VulkanEnabledLogicalDeviceExtensions = extensions;
                // TODO: Как только определимся с поддерживаемой функциональностью, тут же сделать проверки на соответствие
                VulkanEnabledLogicalDeviceFeatures = vulkanLogicalDeviceCreateInfo.RequestedFeatures;

                // Необходимые типы очередей должны задаваться на этапе создания логического устройства

                var queueCreateInfos = new List<DeviceQueueCreateInfo>();

                // Get queue family indices for the requested queue family types
                // Note that the indices may overlap depending on the implementation

                const Single defaultQueuePriority = 0.0f;

                // Очередь для отрисовки графики
                if (vulkanLogicalDeviceCreateInfo.IsRequestedCreateGraphicsQueue)
                {
                    var queueInfo = new DeviceQueueCreateInfo
                    {
                        QueueFamilyIndex = (UInt32) VulkanPhysicalDevice.GraphicsQueueIndex,
                        QueueCount = 1,
                        QueuePriorities = new[] {defaultQueuePriority}
                    };
                    queueCreateInfos.Add(queueInfo);
                }

                if (vulkanLogicalDeviceCreateInfo.IsRequestedCreateComputeQueue)
                {
                    if ((UInt32) VulkanPhysicalDevice.ComputeQueueIndex !=
                        (UInt32) VulkanPhysicalDevice.GraphicsQueueIndex)
                    {
                        var queueInfo = new DeviceQueueCreateInfo
                        {
                            QueueFamilyIndex = (UInt32) VulkanPhysicalDevice.ComputeQueueIndex,
                            QueueCount = 1,
                            QueuePriorities = new[] {defaultQueuePriority}
                        };
                        queueCreateInfos.Add(queueInfo);
                    }
                }

                if (vulkanLogicalDeviceCreateInfo.IsRequestedCreateTransferQueue)
                {
                    if ((UInt32) VulkanPhysicalDevice.TransferQueueIndex !=
                        (UInt32) VulkanPhysicalDevice.GraphicsQueueIndex &&
                        (UInt32) VulkanPhysicalDevice.TransferQueueIndex !=
                        (UInt32) VulkanPhysicalDevice.ComputeQueueIndex)
                    {
                        var queueInfo = new DeviceQueueCreateInfo
                        {
                            QueueFamilyIndex = (UInt32) VulkanPhysicalDevice.TransferQueueIndex,
                            QueueCount = 1,
                            QueuePriorities = new[] {defaultQueuePriority}
                        };
                        queueCreateInfos.Add(queueInfo);
                    }
                }

                if (vulkanLogicalDeviceCreateInfo.IsRequestedCreatePresentationQueue)
                {
                    if ((UInt32) VulkanPhysicalDevice.PresentQueueIndex !=
                        (UInt32) VulkanPhysicalDevice.GraphicsQueueIndex &&
                        (UInt32) VulkanPhysicalDevice.PresentQueueIndex !=
                        (UInt32) VulkanPhysicalDevice.ComputeQueueIndex &&
                        (UInt32) VulkanPhysicalDevice.PresentQueueIndex !=
                        (UInt32) VulkanPhysicalDevice.TransferQueueIndex)
                    {
                        var queueInfo = new DeviceQueueCreateInfo
                        {
                            QueueFamilyIndex = (UInt32) VulkanPhysicalDevice.PresentQueueIndex,
                            QueueCount = 1,
                            QueuePriorities = new[] {defaultQueuePriority}
                        };
                        queueCreateInfos.Add(queueInfo);
                    }
                }

                var deviceCreateInfo = new DeviceCreateInfo {EnabledFeatures = VulkanEnabledLogicalDeviceFeatures};

                if (queueCreateInfos.Count > 0)
                {
                    deviceCreateInfo.QueueCreateInfoCount = (UInt32) queueCreateInfos.Count;
                    deviceCreateInfo.QueueCreateInfos = queueCreateInfos.ToArray();
                }

                if (VulkanEnabledLogicalDeviceExtensions.Count > 0)
                {
                    deviceCreateInfo.EnabledExtensionCount = (UInt32) VulkanEnabledLogicalDeviceExtensions.Count;
                    deviceCreateInfo.EnabledExtensionNames = VulkanEnabledLogicalDeviceExtensions
                        .Select(e => e.ExtensionName)
                        .ToArray();
                }

                Device = VulkanPhysicalDevice.PhysicalDevice.CreateDevice(deviceCreateInfo);

                if (vulkanLogicalDeviceCreateInfo.IsRequestedCreateGraphicsQueue)
                {
                    GraphicsQueue = Device.GetQueue((UInt32) VulkanPhysicalDevice.GraphicsQueueIndex, 0);
                    GraphicsCommandPool = CreateCommandPool((UInt32) VulkanPhysicalDevice.GraphicsQueueIndex);
                }

                if (vulkanLogicalDeviceCreateInfo.IsRequestedCreateComputeQueue)
                {
                    ComputeQueue = Device.GetQueue((UInt32) VulkanPhysicalDevice.ComputeQueueIndex, 0);
                    if (vulkanLogicalDeviceCreateInfo.IsRequestedCreateGraphicsQueue)
                    {
                        if (VulkanPhysicalDevice.GraphicsQueueIndex == VulkanPhysicalDevice.ComputeQueueIndex)
                        {
                            ComputeCommandPool = GraphicsCommandPool;
                        }
                    }
                    if (ComputeCommandPool == null)
                    {
                        ComputeCommandPool = CreateCommandPool((UInt32) VulkanPhysicalDevice.ComputeQueueIndex);
                    }
                }

                if (vulkanLogicalDeviceCreateInfo.IsRequestedCreateTransferQueue)
                {
                    TransferQueue = Device.GetQueue((UInt32) VulkanPhysicalDevice.TransferQueueIndex, 0);

                    if (vulkanLogicalDeviceCreateInfo.IsRequestedCreateGraphicsQueue)
                    {
                        if (VulkanPhysicalDevice.GraphicsQueueIndex == VulkanPhysicalDevice.TransferQueueIndex)
                        {
                            TransferCommandPool = GraphicsCommandPool;
                        }
                    }
                    if (TransferCommandPool == null)
                    {
                        if (vulkanLogicalDeviceCreateInfo.IsRequestedCreateComputeQueue)
                        {
                            if (VulkanPhysicalDevice.ComputeQueueIndex == VulkanPhysicalDevice.TransferQueueIndex)
                            {
                                TransferCommandPool = ComputeCommandPool;
                            }
                        }
                    }
                    if (TransferCommandPool == null)
                    {
                        TransferCommandPool = CreateCommandPool((UInt32) VulkanPhysicalDevice.TransferQueueIndex);
                    }
                }

                if (vulkanLogicalDeviceCreateInfo.IsRequestedCreatePresentationQueue)
                {
                    TransferQueue = Device.GetQueue((UInt32) VulkanPhysicalDevice.PresentQueueIndex, 0);
                }

                _isInit = true;
            }
        }

        public CommandPool CreateCommandPool(UInt32 queueFamilyIndex,
            CommandPoolCreateFlags createFlags = CommandPoolCreateFlags.ResetCommandBuffer)
        {
            var cmdPoolInfo = new CommandPoolCreateInfo
            {
                QueueFamilyIndex = queueFamilyIndex,
                Flags = createFlags
            };
            var cmdPool = Device.CreateCommandPool(cmdPoolInfo);
            return cmdPool;
        }

    }
}