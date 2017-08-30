using System;
using System.Collections.Generic;
using Graphics.Engine.VulkanDriver.VkDevice.Physical;
using Graphics.Engine.VulkanDriver.VkSurface;
using VulkanSharp;

namespace Graphics.Engine.VulkanDriver.VkDevice.Logical
{
    internal sealed class VulkanLogicalDeviceCreateInfo
    {
        /// <summary>
        /// Выбранный видеоадаптер. 
        /// Видеоадаптер который был выбран системой автоматически (рекомендуется), либо указанный в настройках.
        /// Видеоадаптер может быть задан явно, через указание в настройках, в случае, 
        /// когда в системе имеется несколько видеоадаптеров и ведется разработка, 
        /// либо по какой-то причине выбранный системой видаоадаптер не устраивает 
        /// или не отрабатывает как от него ожидают.
        /// </summary>
        public VulkanPhysicalDevice VulkanPhysicalDevice { get; set; }

        /// <summary>
        /// Поверхность отрисовки (связана с окном вывода изображения)
        /// </summary>
        public VulkanSurface VulkanSurface { get; set; }

        /// <summary>
        /// Возможности, которые должны обязательно являться подмножеством физического устройства
        /// </summary>
        public PhysicalDeviceFeatures RequestedFeatures { get; set; }

        /// <summary>
        /// Расширения, которые должны обязательно являться подмножеством физического устройства
        /// </summary>
        public IEnumerable<String> RequestedExtensionNames { get; set; }

        public Boolean IsRequestedCreatePresentationQueue { get; set; }

        /// <summary>
        /// Установлен, в случае, необходимо создать очередь поддерживающую работу с графическими командами
        /// </summary>
        public Boolean IsRequestedCreateGraphicsQueue { get; set; }

        /// <summary>
        /// Установлен, в случае, необходимо создать очередь поддерживающую работу с командами вычислений
        /// </summary>
        public Boolean IsRequestedCreateComputeQueue { get; set; }

        /// <summary>
        /// Установлен, в случае, необходимо создать очередь поддерживающую работу с командами работы с памятью
        /// </summary>
        public Boolean IsRequestedCreateTransferQueue { get; set; }
    }
}