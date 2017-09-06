using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphics.Engine.VulkanDriver.VkDevice.Logical;
using Graphics.Engine.VulkanDriver.VkDevice.Physical;
using Graphics.Engine.VulkanDriver.VkSurface;
using Vulkan;

namespace Graphics.Engine.VulkanDriver.VkSwapchain
{
    internal sealed class VulkanSwapchain
    {
        private Boolean _isInit;

        public VulkanSwapchain()
        {
        }

        /// <summary>
        /// Видеоадаптер, для которого было создано логическое устройство (помещенное в объект обертку).
        /// </summary>
        public VulkanPhysicalDevice VulkanPhysicalDevice { get; private set; }

        public VulkanLogicalDevice VulkanLogicalDevice { get; private set; }

        /// <summary>
        /// Поверхность отрисовки (связана с окном вывода изображения)
        /// </summary>
        public VulkanSurface VulkanSurface { get; set; }

        public SurfaceFormatKhr SurfaceFormat { get; private set; }

        public PresentModeKhr SurfacePresentMode { get; private set; }

        public Extent2D SurfaceExtent2D { get; private set; }

        public SwapchainKhr Swapchain { get; private set; }

        public IReadOnlyList<Image> SwapchainImages { get; private set; }
        public IReadOnlyList<ImageView> SwapchainImageViews { get; private set; }

        /// <summary>
        /// Создает цепочку переключений с указанными пользователем параметрами.
        /// </summary>
        public void Create(VulkanSwapchainCreateInfo vulkanSwapchainCreateInfo)
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

                VulkanPhysicalDevice = vulkanSwapchainCreateInfo.VulkanPhysicalDevice;
                VulkanLogicalDevice = vulkanSwapchainCreateInfo.VulkanLogicalDevice;
                VulkanSurface = vulkanSwapchainCreateInfo.VulkanSurface;

                SurfaceFormat = ChooseSurfaceFormat();
                SurfacePresentMode = ChoosePresentMode();
                SurfaceExtent2D = ChooseSwapExtent();
                Swapchain = CreateSwapChain();
                PrepareSwapchainImages();

                _isInit = true;
            }
        }

        private SurfaceFormatKhr ChooseSurfaceFormat()
        {
            var availableFormats = VulkanPhysicalDevice.AvailableSurfaceFormats;
            if (availableFormats.Count == 1 && availableFormats[0].Format == Format.Undefined)
            {
                return new SurfaceFormatKhr
                {
                    ColorSpace = ColorSpaceKhr.SrgbNonlinear,
                    Format = Format.B8G8R8A8Unorm
                };
            }

            foreach (var surfaceFormatKhr in availableFormats)
            {
                if (surfaceFormatKhr.Format == Format.B8G8R8A8Unorm &&
                    surfaceFormatKhr.ColorSpace == ColorSpaceKhr.SrgbNonlinear)
                {
                    return surfaceFormatKhr;
                }
            }

            return availableFormats[0];
        }

        private PresentModeKhr ChoosePresentMode()
        {
            var availablePresentModes = VulkanPhysicalDevice.AvailableSurfacePresentModes;

            foreach (var availablePresentMode in availablePresentModes)
            {
                if (availablePresentMode == PresentModeKhr.Mailbox)
                {
                    return availablePresentMode;
                }
            }

            return PresentModeKhr.Fifo;
        }

        private Extent2D ChooseSwapExtent()
        {
            var availableCapabilities = VulkanPhysicalDevice.AvailableSurfaceCapabilities;

            if (availableCapabilities.CurrentExtent.Width != UInt32.MaxValue)
            {
                return availableCapabilities.CurrentExtent;
            }
            else
            {
                // TODO: Читать из другого места, ясвно задавать не хорошо
                // TODO: надо предусмотреть константы для приложения
                var actualExtent = new Extent2D
                {
                    Height = 480,
                    Width = 640
                };

                actualExtent.Width = Math.Max(availableCapabilities.MinImageExtent.Width,
                    Math.Min(availableCapabilities.MaxImageExtent.Width, actualExtent.Width));

                actualExtent.Height = Math.Max(availableCapabilities.MinImageExtent.Height,
                    Math.Min(availableCapabilities.MaxImageExtent.Height, actualExtent.Height));

                return actualExtent;
            }
        }

        private SwapchainKhr CreateSwapChain()
        {
            var availableCapabilities = VulkanPhysicalDevice.AvailableSurfaceCapabilities;
            var imageCount = availableCapabilities.MinImageCount + 1;
            if (availableCapabilities.MaxImageCount > 0 && imageCount > availableCapabilities.MaxImageCount)
            {
                imageCount = availableCapabilities.MaxImageCount;
            }
            var createInfo = new SwapchainCreateInfoKhr
            {
                Surface = VulkanSurface.Surface,
                MinImageCount = imageCount,
                ImageFormat = SurfaceFormat.Format,
                ImageColorSpace = SurfaceFormat.ColorSpace,
                ImageExtent = SurfaceExtent2D,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachment,
                PreTransform = SurfaceTransformFlagsKhr.Identity,
                CompositeAlpha = CompositeAlphaFlagsKhr.Opaque,
                Clipped = true,
                //OldSwapchain = null, //При изменении размеров окна надо сюда передавать старый экземпляр 
                PresentMode = SurfacePresentMode,        
            };

            if (VulkanPhysicalDevice.GraphicsQueueIndex != VulkanPhysicalDevice.PresentQueueIndex)
            {
                var queueFamilyIndices = new[]
                {
                    (UInt32) VulkanPhysicalDevice.GraphicsQueueIndex,
                    (UInt32) VulkanPhysicalDevice.PresentQueueIndex
                };
                createInfo.ImageSharingMode = SharingMode.Concurrent;
                createInfo.QueueFamilyIndexCount = (UInt32) queueFamilyIndices.Length;
                createInfo.QueueFamilyIndices = queueFamilyIndices;
            }
            else
            {
                createInfo.ImageSharingMode = SharingMode.Exclusive;
                createInfo.QueueFamilyIndexCount = 0; // Optional
                createInfo.QueueFamilyIndices = null; // Optional
            }

            return VulkanLogicalDevice.Device.CreateSwapchainKHR(createInfo);
        }

        private void PrepareSwapchainImages()
        {
            var images = VulkanLogicalDevice.Device.GetSwapchainImagesKHR(Swapchain);
            SwapchainImages = images.ToList();
            var imageViews = new List<ImageView>();
            foreach (var swapchainImage in SwapchainImages)
            {
                var createInfo = new ImageViewCreateInfo
                {
                    Image = swapchainImage,
                    ViewType = ImageViewType.View2D,
                    Format = SurfaceFormat.Format,
                    Components = new ComponentMapping
                    {
                        A = ComponentSwizzle.Identity,
                        R = ComponentSwizzle.Identity,
                        G = ComponentSwizzle.Identity,
                        B = ComponentSwizzle.Identity
                    },
                    SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = ImageAspectFlags.Color,
                        BaseMipLevel = 0,
                        LevelCount = 1,
                        BaseArrayLayer = 0,
                        LayerCount = 1
                    }
                };
                imageViews.Add(VulkanLogicalDevice.Device.CreateImageView(createInfo));
            }
            SwapchainImageViews = imageViews;
        }
    }
}