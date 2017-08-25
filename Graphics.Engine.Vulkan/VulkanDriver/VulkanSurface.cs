using VulkanSharp;
using VulkanSharp.Windows;

namespace Graphics.Engine.VulkanDriver
{
    /// <summary>
    /// Создает объект-обертку над экземпляром поверхности отрисовки Vulkan
    /// </summary>
    internal sealed class VulkanSurface
    {
        public VulkanSurface(VulkanInstance vulkanInstance, INativeWindow vulkanWindow)
        {
            VulkanInstance = vulkanInstance;
            VulkanWindow = vulkanWindow;
        }

        /// <summary>
        /// Экземпляр(объект или инстанс) Vulkan - хранит все состояния для текущего приложения
        /// Создается один раз при инициализации. 
        /// </summary>
        public VulkanInstance VulkanInstance { get; }

        /// <summary>
        /// Экземпляр окна, к которому привязана текущая поверхность
        /// </summary>
        public INativeWindow VulkanWindow { get; }

        /// <summary>
        /// Экземпляр поверхности отрисовки связанный с окном
        /// </summary>
        public SurfaceKhr Surface { get; private set; }

        public void Create()
        {
            var createInfo = new Win32SurfaceCreateInfoKhr
            {
                Hinstance = VulkanWindow.ProcessHandle,
                Hwnd = VulkanWindow.WindowHandle
            };
            Surface = VulkanInstance.Instance.CreateWin32SurfaceKHR(createInfo);
        }
    }
}