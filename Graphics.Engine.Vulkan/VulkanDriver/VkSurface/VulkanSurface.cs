using System;
using System.Diagnostics;
using Graphics.Engine.VulkanDriver.VkInstance;
using OpenTK;
using Vulkan;
using Vulkan.Windows;

namespace Graphics.Engine.VulkanDriver.VkSurface
{
    /// <summary>
    /// Создает объект-обертку над экземпляром поверхности отрисовки Vulkan
    /// </summary>
    internal sealed class VulkanSurface
    {
        private Boolean _isInit;

        public VulkanSurface()
        {
        }

        /// <summary>
        /// Экземпляр(объект или инстанс) Vulkan - хранит все состояния для текущего приложения
        /// Создается один раз при инициализации. 
        /// </summary>
        public VulkanInstance VulkanInstance { get; private set; }

        /// <summary>
        /// Экземпляр окна, к которому привязана текущая поверхность
        /// </summary>
        public INativeWindow VulkanWindow { get; private set; }

        /// <summary>
        /// Экземпляр поверхности отрисовки связанный с окном
        /// </summary>
        public SurfaceKhr Surface { get; private set; }

        public void Create(VulkanSurfaceCreateInfo vulkanSurfaceCreateInfo)
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

                VulkanInstance = vulkanSurfaceCreateInfo.VulkanInstance;
                VulkanWindow = vulkanSurfaceCreateInfo.VulkanWindow;
                
                var createInfo = new Win32SurfaceCreateInfoKhr
                {
                    Hinstance = Process.GetCurrentProcess().Handle,
                    Hwnd = VulkanWindow.WindowInfo.Handle
                };

                Surface = VulkanInstance.Instance.CreateWin32SurfaceKHR(createInfo);

                _isInit = true;
            }
        }
    }
}