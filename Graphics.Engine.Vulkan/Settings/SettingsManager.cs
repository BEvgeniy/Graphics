using System;

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
                    ApplicationVersion = Vulkan.Version.Make(1, 0, 0);
                    EngineName = "Atlas Engine";
                    EngineVersion = Vulkan.Version.Make(1, 0, 0);
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

        /// <summary>
        /// Версия API Vulkan (Поддерживаемая версия)
        /// </summary>
        public static UInt32 VulkanApiVersion => Vulkan.Version.Make(1, 0, 0);

        /// <summary>
        /// Свойство определяет разрешено ли включение отладки и слоев валидации Vulkan'а.
        /// Если флаг установлен отладка разрешена, иначе запрещена.
        /// </summary>
        public static Boolean IsDebugEnabled { get; private set; }

        /// <summary>
        /// Имя приложения. Используется при создании экземпляра объекта (инстанса) Vulkan. 
        /// Имя можно указывать любое. Носит для Vulkan только информационный характер.
        /// Полезно при отладке, кроме прочей информации Vulkan добавляет эту информацию в информацию отладочную.
        /// </summary>
        public static String ApplicationName { get; private set; }

        /// <summary>
        /// Версия приложения. Используется при создании экземпляра объекта (инстанса) Vulkan. 
        /// Версию можно указывать любую. Носит для Vulkan только информационный характер.
        /// Полезно при отладке, кроме прочей информации Vulkan добавляет эту информацию в информацию отладочную.
        /// </summary>
        public static UInt32 ApplicationVersion { get; private set; }

        /// <summary>
        /// Имя движка. Используется при создании экземпляра объекта (инстанса) Vulkan. 
        /// Имя можно указывать любое. Носит для Vulkan только информационный характер.
        /// Полезно при отладке, кроме прочей информации Vulkan добавляет эту информацию в информацию отладочную.
        /// </summary>
        public static String EngineName { get; private set; }

        /// <summary>
        /// Версия движка. Используется при создании экземпляра объекта (инстанса) Vulkan. 
        /// Версию можно указывать любую. Носит для Vulkan только информационный характер.
        /// Полезно при отладке, кроме прочей информации Vulkan добавляет эту информацию в информацию отладочную.
        /// </summary>
        public static UInt32 EngineVersion { get; private set; }
    }
}