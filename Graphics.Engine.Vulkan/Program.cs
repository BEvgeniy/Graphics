using System;
using Graphic.Engine.VulkanDriver;
using Graphics.Engine.Settings;
using Graphics.Engine.VulkanDriver;
using Vulkan1 = Vulkan;

namespace Graphics.Engine
{
    internal class Program
    {
        private static void Main(String[] args)
        {
            //var hWnd = new System.Windows.Interop.WindowInteropHelper(this).EnsureHandle();
            //var hInstance = System.Runtime.InteropServices.Marshal.GetHINSTANCE(typeof(App).Module);

            #region Создадим экземпляр Vulkan'a

            try
            {
                // Подгрузим настройки в наше приложение для использования. Этот шаг должен быть свегда первым.
                // Т.к. менеджер типа VulkanManager, также как и другие менеджеры используют загруженные настройки из файлов конфигураций
                SettingsManager.LoadSettings();
                // Теперь проинициализируем Vulkan
                VulkanManager.Init();
            }
            catch (Vulkan1.ResultException e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }

            #endregion
        }
    }
}