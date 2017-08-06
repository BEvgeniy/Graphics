using System;
using Graphic.Engine.VulkanDriver;
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
                VulkanManager.Init();
            }
            catch (Vulkan1.ResultException e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                return;
            }

            #endregion
        }
    }
}