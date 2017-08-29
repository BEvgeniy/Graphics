using System;
using System.Collections.Generic;
using Graphics.Engine.VulkanDriver;

namespace Graphics.Engine.Settings
{
    public class SettingsManager
    {
        // Установлен, если загрузка и инициализация была проведена успешно
        private static Boolean _isLoaded;

        // Объект для синхронизации выполнения критических участков кода. 
        // В данном случае позволяется помеченный участок кода выполнять только одним потоком.
        // Если один поток выполняет помеченый участок кода, то другие в это время ожидают.
        private static readonly Object SyncObject = new Object();

        public static void LoadSettings()
        {
            if (_isLoaded) return;

            lock (SyncObject)
            {
                if (_isLoaded) return;
                try
                {
                    // TODO: Загружаить настройки из файла(ов)
                    // а пока заполняю статичной информацией
                    IsDebugEnabled = true;
                    ApplicationName = "Atlas";
                    ApplicationVersion = new Version(1, 0, 0);
                    EngineName = "Atlas Engine";
                    EngineVersion = new Version(1, 0, 0);
                    // Задание необходимых расширений и дополнительных слоев
                    InitExtentionsAndLayers();
                }
                catch (Exception ex)
                {
                    // TODO: Что-то делать если не удалось загружить настройки
                }
                finally
                {
                    _isLoaded = false;
                }
            }
        }

        private static void InitExtentionsAndLayers()
        {
            // Отрисовка в окно требуется всегда
            var extentionNames = new List<String>
            {
                "VK_KHR_surface",
                "VK_KHR_win32_surface"
            };

            // Отрисовка в окно требуется всегда, следовательно и цепочка переключений тоже требуется всегда
            var physicalDeviceExtentionNames = new List<String>
            {
                "VK_KHR_swapchain",
               // "VK_KHR_sampler_mirror_clamp_to_edge"
            };

            var logicalDeviceExtentionNames = physicalDeviceExtentionNames;

            var layerNames = new List<String>();

            if (IsDebugEnabled)
            {
                // Если собираемся отлаживаться и проходить валидацию по слоям, надо подключить расширение отладки и слои валидации
                extentionNames.Add("VK_EXT_debug_report");

               // physicalDeviceExtentionNames.Add("VK_EXT_debug_marker");
               // logicalDeviceExtentionNames.Add("VK_EXT_debug_marker");

                layerNames.Add(
                    "VK_LAYER_LUNARG_standard_validation"
                );

                //foreach (var layer in VulkanAvailableInstanceLayers)
                //{
                //    if (
                //        layer.LayerName == "VK_LAYER_LUNARG_vktrace"
                //        || layer.LayerName == "VK_LAYER_LUNARG_api_dump"
                //        || layer.LayerName == "VK_LAYER_LUNARG_device_simulation"
                //        // || layer.LayerName == "VK_LAYER_GOOGLE_threading" 
                //        // || layer.LayerName == "VK_LAYER_GOOGLE_unique_objects" 
                //        // ||layer.LayerName == "VK_LAYER_VALVE_steam_overlay"
                //    )
                //    {
                //        continue;
                //    }
                //    requestedLayers.Add(layer);
                //}

            }

            RequestedInstanceExtentionNames = extentionNames;
            RequestedInstanceLayerNames = layerNames;
            RequestedPhysicalDeviceExtentionNames = physicalDeviceExtentionNames;
            RequestedLogicalDeviceExtentionNames = logicalDeviceExtentionNames;
        }

        /// <summary>
        /// Версия API Vulkan. Поддерживаемая версия - хардкор, так как не уверен, 
        /// что после изменения стандарта и как следствие версии, что-нибудь не устареет (будет помечено obsolete).
        /// В Vulkan используется версия трех компонент: Major, Minor, Patch
        /// Так в c# версию можно представить как: Major, Minor, Build (Patch)
        /// Четвертый компонент Revision не используется
        /// ----------------------------------------------------------------------
        /// На текущий момент используется первая версия Vulkan без указания Minor и Patch
        /// Реально поддерживаемую версию Vulkan можно получить конкретно для каждого физического устройства
        /// Данное свойство используется при создании экземпляра Vulkan, 
        /// а также планируется использовать в будущем, при проверке использования необходимой функциональности
        /// </summary>
        public static Version VulkanApiVersion => new Version(1, 0, 0);

        /// <summary>
        /// Свойство определяет разрешено ли включение отладки и слоев валидации Vulkan'а.
        /// Если флаг установлен отладка разрешена, иначе запрещена.
        /// </summary>
        public static Boolean IsDebugEnabled { get; private set; }

        /// <summary>
        /// Имя приложения. Используется при создании экземпляра объекта (инстанса)  
        /// Имя можно указывать любое. Носит для Vulkan только информационный характер.
        /// Полезно при отладке, кроме прочей информации Vulkan добавляет эту информацию в информацию отладочную.
        /// </summary>
        public static String ApplicationName { get; private set; }

        /// <summary>
        /// Версия приложения. Используется при создании экземпляра объекта (инстанса)  
        /// Версию можно указывать любую. Носит для Vulkan только информационный характер.
        /// Полезно при отладке, кроме прочей информации Vulkan добавляет эту информацию в информацию отладочную.
        /// </summary>
        public static Version ApplicationVersion { get; private set; }

        /// <summary>
        /// Имя движка. Используется при создании экземпляра объекта (инстанса)  
        /// Имя можно указывать любое. Носит для Vulkan только информационный характер.
        /// Полезно при отладке, кроме прочей информации Vulkan добавляет эту информацию в информацию отладочную.
        /// </summary>
        public static String EngineName { get; private set; }

        /// <summary>
        /// Версия движка. Используется при создании экземпляра объекта (инстанса)  
        /// Версию можно указывать любую. Носит для Vulkan только информационный характер.
        /// Полезно при отладке, кроме прочей информации Vulkan добавляет эту информацию в информацию отладочную.
        /// </summary>
        public static Version EngineVersion { get; private set; }

        /// <summary>
        /// Имена расширений, указанные в файле конфигурации, 
        /// которые требуется иметь предустановленными в системе (это как я ни текущий момент это понимаю, но могу ошибаться)
        /// при создании экземпляра Vulkan.
        /// </summary>
        public static IEnumerable<String> RequestedInstanceExtentionNames { get; private set; }

        /// <summary>
        /// Имена слоев, указанные в файле конфигурации, 
        /// которые требуется иметь предустановленными в системе (это как я ни текущий момент это понимаю, но могу ошибаться)
        /// при создании экземпляра Vulkan.
        /// </summary>
        public static IEnumerable<String> RequestedInstanceLayerNames { get; private set; }

        /// <summary>
        /// Имена расширений, которые должны поддерживаться физическим устройством, 
        /// при поиске подходящего.
        /// </summary>
        public static IEnumerable<String> RequestedPhysicalDeviceExtentionNames { get; private set; }

        /// <summary>
        /// Имена расширений, которые должны поддерживаться физическим устройством, 
        /// иначе логическое устройство создать не удасться
        /// Если рассматривать расширения физического устройства как множество
        /// то расширения логического устройства должно быть либо подмножеством физического, 
        /// либо целым множеством, но никак иначе
        /// </summary>
        public static IEnumerable<String> RequestedLogicalDeviceExtentionNames { get; private set; }
    }
}