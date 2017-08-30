using System;
using System.Collections.Generic;
using Graphics.Engine.Settings;
using Graphics.Engine.VulkanDriver.VkSurface;
using VulkanSharp;
using Version = System.Version;

namespace Graphics.Engine.VulkanDriver.VkDevice.Physical
{
    /// <summary>
    /// Задает информацию для поиска физического устройства (видеоадаптера)
    /// </summary>
    internal sealed class VulkanPhysicalDeviceSearchInfo
    {
        /// <summary>
        /// Предпочтительная версия Vulkan, которую должно поддерживать физическое устройство.
        /// 1. Если версия указана, будет искать физическое устройство с версией точно подходящей с данной версией, 
        ///    если устройство не будет найдено, то будет поиск по пункту 2.
        /// 2. Если версия не указана или не удалось найти по указанной версии подходящего физического устройства, 
        ///    будет осуществляться поиск версии согласно указанной в <see cref="SettingsManager.VulkanApiVersion"/>
        ///    но с максимально возможными Minor и Patch
        /// </summary>
        public Version PreferredVulkanApiVersion { get; set; }

        /// <summary>
        /// Предпочтительное физическое устройство:
        /// Внешнее (например внешняя видеокарта)
        /// Встроенное (например интегрированная видеокарта)
        /// Виртульное (например исполняемое на VMWare - эмуляция реальной видеокарты)
        /// ЦПУ (центральный процессор)
        /// ------------------------------------------------------
        /// Принцип поика физического устройства по типу простой с приоритетом:
        /// Самый высший приоритет у внешнего
        /// Далее удет встроенное
        /// За ним виртульное
        /// и затом только ЦПУ
        /// </summary>
        public PhysicalDeviceType PreferredType { get; set; }

        /// <summary>
        /// Установлен, в случае, если физическое устройство должно поддерживать представление
        /// При поиске такой очереди предпочтительно использовать индекс очереди равный индексу графической очереди 
        /// в этом случае не будет надобности заботиться о синхронизации доступа к ресурсу при создании цепочки своппинга (swap chain, цепочка переключений)
        /// </summary>
        public Boolean IsRequestedSupportPresentationQueue { get; set; }

        /// <summary>
        /// Поверхность для отрисовки. Необходимо указывать в случае если, 
        /// требуется наличие семейства очередей с поддержкой представления <see cref="IsRequestedSupportPresentationQueue"/>
        /// </summary>
        public VulkanSurface VulkanSurface { get; set; }

        /// <summary>
        /// Установлен, в случае, если физическое устройство должно поддерживать графические команды
        /// </summary>
        public Boolean IsRequestedSupportGraphicsQueue { get; set; }

        /// <summary>
        /// Установлен, в случае, если физическое устройство должно поддерживать команды вычислений
        /// </summary>
        public Boolean IsRequestedSupportComputeQueue { get; set; }

        /// <summary>
        /// Установлен, в случае, если физическое устройство должно поддерживать команды работы с памятью (передача, копирование)
        /// </summary>
        public Boolean IsRequestedSupportTransferQueue { get; set; }

        /// <summary>
        /// Возможности, которые должны обязательно поддерживаться физическим устройством
        /// </summary>
        public PhysicalDeviceFeatures RequestedFeatures { get; set; }

        /// <summary>
        /// Расширения, которые должны обязательно поддерживаться физическим устройством
        /// </summary>
        public IEnumerable<String> RequestedExtensionNames { get; set; }
    }
}