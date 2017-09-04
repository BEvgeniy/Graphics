using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using Graphics.Engine.Settings;
using Graphics.Engine.VulkanDriver.VkDevice.Logical;
using Graphics.Engine.VulkanDriver.VkDevice.Physical;
using Graphics.Engine.VulkanDriver.VkInstance;
using Graphics.Engine.VulkanDriver.VkShader;
using Graphics.Engine.VulkanDriver.VkSurface;
using Graphics.Engine.VulkanDriver.VkSwapchain;
using OpenTK;
using VulkanSharp;

namespace Graphics.Engine.VulkanDriver
{
    internal class VulkanManager
    {
        #region .fields

        /// <summary>
        /// Флаг - определяет был ли создан экземпляр Vulkan
        /// </summary>
        private Boolean _isVulkanInit;

        /// <summary>
        /// Обертка над Device и PhysicalDevice (выбранный видеоадаптер)
        /// Device - это логическое устройство, 
        /// и таких устройств можно создать много на основе одного PhysicalDevice,
        /// при этом каждое такое логичесвое устройство может иметь разную доступную функциональность
        /// Скажем так, пусть устройство PhysicalDevice имеет 2 фичи: VkBool32 geometryShader и VkBool32 tessellationShader;
        /// Так вот мы можем создать два логических устройства, одно из которых будет поддерживать одно из фич, а второе оставшуюся фичу
        /// </summary>
        private VulkanDevice _vulkanDevice;

        /// <summary>
        /// Объект для синхронизации выполнения критических участков кода. 
        /// В данном случае, позволяется, помеченный участок кода выполнять только одним потоком.
        /// Если один поток выполняет помеченый участок кода, то другие в это время ожидают.
        /// </summary>
        private readonly Object SyncObject = new Object();

        #endregion

        #region .props

        /// <summary>
        /// Экземпляр(объект или инстанс) Vulkan - хранит все состояния для текущего приложения
        /// Создается один раз при инициализации. 
        /// </summary>
        public VulkanInstance VulkanInstance { get; private set; }

        /// <summary>
        /// Выбранный видеоадаптер. 
        /// Видеоадаптер который был выбран системой автоматически (рекомендуется), либо указанный в настройках.
        /// Видеоадаптер может быть задан явно, через указание в настройках, в случае, когда в системе имеется несколько видеоадаптеров и ведется разработка, 
        /// либо по какой-то причине выбранный системой видаоадаптер не устраивает или не отрабатывает как от него ожидают.
        /// </summary>
        public VulkanPhysicalDevice VulkanPhysicalDevice { get; private set; }

        /// <summary>
        /// Логическое устройство для выбранного видеоадаптера. 
        /// </summary>
        public VulkanLogicalDevice VulkanLogicalDevice { get; private set; }

        /// <summary>
        /// Цепочка переключений (своп). 
        /// </summary>
        public VulkanSwapchain VulkanSwapchain { get; private set; }

        /// <summary>
        /// Шейдер (вершинный + пиксельный)
        /// </summary>
        public VulkanShader VulkanShader { get; private set; }

        public RenderPass RenderPass { get; private set; }

        public Pipeline Pipeline { get; private set; }

        public IReadOnlyList<Framebuffer> Framebuffers { get; private set; }

        public IReadOnlyList<CommandBuffer> CommandBuffers { get; private set; }

        public Semaphore ImageAvailableSemaphore { get; private set; }

        public Semaphore RenderFinishedSemaphore { get; private set; }

        #endregion

        public VulkanManager()
        {
            _isVulkanInit = false;
        }

        public void Init(INativeWindow vulkanMainWindow)
        {
            if (_isVulkanInit) return;

            lock (SyncObject)
            {
                if (_isVulkanInit) return;
                // Создадим экземпляр Vulkan
                CreateInstance(vulkanMainWindow);
                // Выбираем наилучшее для нас устройство
                CreatePhysicalDevice();
                // Создадим логическое устройство связанное с видеоадаптером
                CreateLogicalDevice();
                // Создадим цепочку переключений
                CreateSwapchain();
                // Создадим шейдеры
                CreateShaders();
                // Создадим проход рендеринга
                CreateRenderPass();
                // Создадим конвеер
                CreatePipeline();
                // Создадим буферы для фреймов (сколько было создано в цепочке переключений, столько будет и буферов)
                CreateFramebuffers();
                // Создадим командные буферы
                CreateCommandBuffers();
                // Запишем команды в командные буферы
                WriteCommandBuffers();
                // Создадим объекты синхронизации видеокарты и хоста 
                СreateSemaphores();
                
            }
        }

        public void DrawFrame()
        {
            WaitIdle();

            var imageIndex = VulkanLogicalDevice.Device.AcquireNextImageKHR(VulkanSwapchain.Swapchain, UInt64.MaxValue,
                ImageAvailableSemaphore);
            
            var submitInfo = new SubmitInfo();

            Semaphore[] waitSemaphores = {ImageAvailableSemaphore};
            PipelineStageFlags[] waitStages = {PipelineStageFlags.ColorAttachmentOutput};
            submitInfo.WaitSemaphoreCount = 1;
            submitInfo.WaitSemaphores = waitSemaphores;
            submitInfo.WaitDstStageMask = waitStages;

            submitInfo.CommandBufferCount = 1;
            submitInfo.CommandBuffers = new[] {CommandBuffers.ElementAt((Int32) imageIndex)};

            Semaphore[] signalSemaphores = {RenderFinishedSemaphore};
            submitInfo.SignalSemaphoreCount = 1;
            submitInfo.SignalSemaphores = signalSemaphores;

            VulkanLogicalDevice.GraphicsQueue.Submit(submitInfo);

            var presentInfo = new PresentInfoKhr
            {
                WaitSemaphoreCount = 1,
                WaitSemaphores = signalSemaphores
            };

            SwapchainKhr[] swapChains = {VulkanSwapchain.Swapchain};
            presentInfo.SwapchainCount = 1;
            presentInfo.Swapchains = swapChains;

            presentInfo.ImageIndices = new[] {imageIndex};

            VulkanLogicalDevice.GraphicsQueue.PresentKHR(presentInfo);
        }

        private void СreateSemaphores()
        {
            var semaphoreInfo = new SemaphoreCreateInfo();
            ImageAvailableSemaphore = VulkanLogicalDevice.Device.CreateSemaphore(semaphoreInfo);
            RenderFinishedSemaphore = VulkanLogicalDevice.Device.CreateSemaphore(semaphoreInfo);
        }

        private void WriteCommandBuffers()
        {
            for (var i = 0; i < CommandBuffers.Count; i++)
            {
                var beginInfo = new CommandBufferBeginInfo
                {
                    Flags = CommandBufferUsageFlags.SimultaneousUse,
                    //InheritanceInfo = null // Optional
                };
                
                CommandBuffers[i].Begin(beginInfo);

                var clearColor = new ClearValue
                {
                    Color = new ClearColorValue(0.2f, 0.2f, 0.2f, 1.0f)
                };

                var renderPassInfo = new RenderPassBeginInfo
                {
                    RenderPass = RenderPass,
                    ClearValueCount = 1,
                    ClearValues = new[] {clearColor},
                    Framebuffer = Framebuffers[i],
                    RenderArea = new Rect2D
                    {
                        Offset = new Offset2D(),
                        Extent = VulkanSwapchain.SurfaceExtent2D
                    }
                };
                
                CommandBuffers[i].CmdBeginRenderPass(renderPassInfo, SubpassContents.Inline);
                CommandBuffers[i].CmdBindPipeline(PipelineBindPoint.Graphics, Pipeline);
                CommandBuffers[i].CmdDraw(3, 1, 0, 0);
                CommandBuffers[i].CmdEndRenderPass();

                CommandBuffers[i].End();
            }
        }

        private void CreateCommandBuffers()
        {
            var allocateInfo = new CommandBufferAllocateInfo
            {
                CommandPool = VulkanLogicalDevice.GraphicsCommandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = (UInt32) VulkanSwapchain.SwapchainImageViews.Count
            };

            CommandBuffers = VulkanLogicalDevice.Device.AllocateCommandBuffers(allocateInfo);
        }

        private void CreateFramebuffers()
        {
            var framebuffers = new List<Framebuffer>();

            for (var i = 0; i < VulkanSwapchain.SwapchainImageViews.Count; i++)
            {
                ImageView[] attachments =
                {
                    VulkanSwapchain.SwapchainImageViews[i]
                };

                var framebufferCreateInfo = new FramebufferCreateInfo
                {
                    RenderPass = RenderPass,
                    AttachmentCount = 1,
                    Attachments = attachments,
                    Width = VulkanSwapchain.SurfaceExtent2D.Width,
                    Height = VulkanSwapchain.SurfaceExtent2D.Height,
                    Layers = 1
                };

                var framebuffer = VulkanLogicalDevice.Device.CreateFramebuffer(framebufferCreateInfo);

                framebuffers.Add(framebuffer);
            }

            Framebuffers = framebuffers;
        }

        private void CreateRenderPass()
        {
            // Описываем вложение цвета (для цепочки рендеринга)
            var colorAttachment = new AttachmentDescription
            {
                Format = VulkanSwapchain.SurfaceFormat.Format,
                Samples = SampleCountFlags.Count1,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr
            };
            // layout(location = 0) out vec4 outColor (в fragment shader)
            var colorAttachmentRef = new AttachmentReference
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal
            };

            var subPass = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                ColorAttachments = new[] {colorAttachmentRef},
                //InputAttachmentCount = 0,
                //InputAttachments = null,
                //PreserveAttachmentCount = 0,
                //PreserveAttachments = null,
                //DepthStencilAttachment = null,
                //ResolveAttachments = null
            };

            var dependency = new SubpassDependency
            {
                SrcSubpass = 0,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutput,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.ColorAttachmentOutput,
                DstAccessMask = AccessFlags.ColorAttachmentRead | AccessFlags.ColorAttachmentWrite
            };

            var renderPassInfo = new RenderPassCreateInfo
            {
                AttachmentCount = 1,
                Attachments = new[] {colorAttachment},
                SubpassCount = 1,
                Subpasses = new[] {subPass},
                DependencyCount = 1,
                Dependencies = new[] {dependency}
            };

            RenderPass = VulkanLogicalDevice.Device.CreateRenderPass(renderPassInfo);
        }

        private void CreateShaders()
        {
            var vulkanShaderCreateInfo = new VulkanShaderCreateInfo
            {
                VulkanLogicalDevice = VulkanLogicalDevice,
                VertexFileName = "VS_Color.vert",
                FragmentFileName = "PS_Color.frag"
            };
            var vulkanShader = new VulkanShader();
            vulkanShader.Create(vulkanShaderCreateInfo);
            VulkanShader = vulkanShader;
        }

        private void CreatePipeline()
        {
            // Fixes pipeline stages settings
            var vertexInputState = CreatePipelineVertexInputState();
            var inputAssemblyState = CreatePipelineInputAssemblyState();
            var viewportState = CreatePipelineViewportState();
            var rasterizer = CreatePipelineRasterizationState();
            var multisampling = CreatePipelineMultisampleState();
            var depthStencil = CreatePipelineDepthStencilState();
            var colorBlending = CreatePipelineColorBlendState();
            var dynamicState = CreatePipelineDynamicState();
            var pipelineLayoutState = CreatePipelineLayoutState();

            var pipelineLayout = VulkanLogicalDevice.Device.CreatePipelineLayout(pipelineLayoutState);

            var vertexShaderStage = CreatePipelineShaderStage(ShaderStageFlags.Vertex, VulkanShader.VertexModule);
            var fragmentShaderStage = CreatePipelineShaderStage(ShaderStageFlags.Fragment, VulkanShader.FragmentModule);

            // Имея все - теперь можно создать сам графический конвеер

            var pipelineInfo = new GraphicsPipelineCreateInfo();
            {
                pipelineInfo.StageCount = 2;
                // Количество шейдерных программ - 2 (vertex shader & fragment shader)
                pipelineInfo.Stages = new[] {vertexShaderStage, fragmentShaderStage};
                pipelineInfo.VertexInputState = vertexInputState;
                pipelineInfo.InputAssemblyState = inputAssemblyState;
                pipelineInfo.ViewportState = viewportState;
                pipelineInfo.RasterizationState = rasterizer;
                pipelineInfo.MultisampleState = multisampling;
                //pipelineInfo.DepthStencilState = depthStencil; // Optional
                pipelineInfo.ColorBlendState = colorBlending;
                //pipelineInfo.DynamicState = dynamicState;// Optional
                pipelineInfo.Layout = pipelineLayout;
                pipelineInfo.RenderPass = RenderPass;
                pipelineInfo.Subpass = 0;
                //pipelineInfo.BasePipelineHandle = new Pipeline();
                //pipelineInfo.BasePipelineIndex = -1; // Optional
            };

           Pipeline = VulkanLogicalDevice.Device.CreateGraphicsPipelines(new PipelineCache(), 1, pipelineInfo)[0];
        }

        private PipelineInputAssemblyStateCreateInfo CreatePipelineInputAssemblyState()
        {
            var inputAssemblyState = new PipelineInputAssemblyStateCreateInfo
            {
                Topology = PrimitiveTopology.TriangleList,
                PrimitiveRestartEnable = false
            };
            return inputAssemblyState;
        }

        private PipelineVertexInputStateCreateInfo CreatePipelineVertexInputState()
        {
            var vertexInputState = new PipelineVertexInputStateCreateInfo
            {
                VertexBindingDescriptionCount = 0,
                VertexBindingDescriptions = null, // Optional
                VertexAttributeDescriptionCount = 0,
                VertexAttributeDescriptions = null // Optional
            };
            return vertexInputState;
        }

        private PipelineShaderStageCreateInfo CreatePipelineShaderStage(ShaderStageFlags shaderType, ShaderModule module)
        {
            var createInfo = new PipelineShaderStageCreateInfo
            {
                Stage = shaderType,
                Module = module,
                // Имя точки входа в программу шейдера (функция с которой начинается выполнение программы)
                Name = "main"
            };
            return createInfo;
        }

        private PipelineViewportStateCreateInfo CreatePipelineViewportState()
        {
            // Область просмотра изображения в окне
            var viewport = new Viewport
            {
                X = 0.0f,
                Y = 0.0f,
                Width = VulkanSwapchain.SurfaceExtent2D.Width,
                Height = VulkanSwapchain.SurfaceExtent2D.Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
            };
            var scissor = new Rect2D
            {
                Offset = new Offset2D
                {
                    X = 0,
                    Y = 0,
                },
                Extent = new Extent2D
                {
                    Width = VulkanSwapchain.SurfaceExtent2D.Width,
                    Height = VulkanSwapchain.SurfaceExtent2D.Height
                }
            };
            var viewportState = new PipelineViewportStateCreateInfo
            {
                ViewportCount = 1,
                Viewports = new[] {viewport},
                ScissorCount = 1,
                Scissors = new[] {scissor}
            };
            return viewportState;
        }

        private PipelineRasterizationStateCreateInfo CreatePipelineRasterizationState()
        {
            var rasterizer = new PipelineRasterizationStateCreateInfo
            {
                DepthClampEnable = false,
                RasterizerDiscardEnable = false,
                PolygonMode = PolygonMode.Fill,
                LineWidth = 1.0f,
                CullMode = CullModeFlags.Back,
                FrontFace = FrontFace.Clockwise,
                //DepthBiasEnable = false,
                //DepthBiasConstantFactor = 0.0f, // Optional
                //DepthBiasClamp = 0.0f, // Optional
                //DepthBiasSlopeFactor = 0.0f // Optional
            };
            return rasterizer;
        }

        private PipelineMultisampleStateCreateInfo CreatePipelineMultisampleState()
        {
            var multisampling = new PipelineMultisampleStateCreateInfo
            {
                SampleShadingEnable = false,
                RasterizationSamples = SampleCountFlags.Count1,
                //MinSampleShading = 1.0f,// Optional
                //SampleMask = null,// Optional
                //AlphaToCoverageEnable = false,// Optional
                //AlphaToOneEnable = false// Optional
            };
            return multisampling;
        }

        private PipelineDepthStencilStateCreateInfo CreatePipelineDepthStencilState()
        {
            var depthStencil = new PipelineDepthStencilStateCreateInfo
            {
                DepthTestEnable = true,
                DepthWriteEnable = true,
                DepthCompareOp = CompareOp.Less,
                DepthBoundsTestEnable = false,
                MinDepthBounds = 0.0f, // Optional
                MaxDepthBounds = 1.0f, // Optional
                StencilTestEnable = false,
                Front = new StencilOpState(), // Optional
                Back = new StencilOpState() // Optional
            };

            return new PipelineDepthStencilStateCreateInfo();
            // TODO: при отображении трехмерных моделей разблокировать - сверху удалить
            // return depthStencil;
        }

        private PipelineColorBlendStateCreateInfo CreatePipelineColorBlendState()
        {
            // opaque
            var colorBlendAttachment = new PipelineColorBlendAttachmentState
            {
                BlendEnable = false,
                SrcColorBlendFactor = BlendFactor.One, // Optional
                DstColorBlendFactor = BlendFactor.Zero, // Optional
                ColorBlendOp = BlendOp.Add,
                SrcAlphaBlendFactor = BlendFactor.One, // Optional
                DstAlphaBlendFactor = BlendFactor.Zero, // Optional
                AlphaBlendOp = BlendOp.Add,
                ColorWriteMask = ColorComponentFlags.R | ColorComponentFlags.G |
                                 ColorComponentFlags.B | ColorComponentFlags.A
            };

            // not opaque (transparent)
            //var colorBlendAttachment = new PipelineColorBlendAttachmentState
            //{
            //    BlendEnable = true,
            //    SrcColorBlendFactor = BlendFactor.SrcAlpha, // Optional
            //    DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha, // Optional
            //    ColorBlendOp = BlendOp.Add,
            //    SrcAlphaBlendFactor = BlendFactor.One, // Optional
            //    DstAlphaBlendFactor = BlendFactor.Zero, // Optional
            //    AlphaBlendOp = BlendOp.Add,
            //    ColorWriteMask = ColorComponentFlags.R | ColorComponentFlags.G |
            //                     ColorComponentFlags.B | ColorComponentFlags.A
            //};

            var colorBlending = new PipelineColorBlendStateCreateInfo
            {
                LogicOpEnable = false,
                LogicOp = LogicOp.Copy, // Optional
                AttachmentCount = 1,
                Attachments = new[] {colorBlendAttachment},
                BlendConstants =
                {
                    [0] = 0.0f, // Optional
                    [1] = 0.0f, // Optional
                    [2] = 0.0f, // Optional
                    [3] = 0.0f, // Optional
                }
            };

            return colorBlending;
        }

        private PipelineDynamicStateCreateInfo CreatePipelineDynamicState()
        {
            DynamicState[] dynamicStates =
            {
                DynamicState.Viewport,
                DynamicState.LineWidth,
            };

            var dynamicState = new PipelineDynamicStateCreateInfo
            {
                DynamicStateCount = (UInt32) dynamicStates.Length,
                DynamicStates = dynamicStates
            };
            //return new PipelineDynamicStateCreateInfo();
            // TODO: при необходимости разблокировать - сверху удалить
            return dynamicState;
        }

        private PipelineLayoutCreateInfo CreatePipelineLayoutState()
        {
            var layout = new PipelineLayoutCreateInfo
            {
                SetLayoutCount = 0, // Optional
                SetLayouts = null, // Optional
                PushConstantRangeCount = 0, // Optional
                PushConstantRanges = null // Optional
            };

            return layout;
        }

        private void CreateSwapchain()
        {
            var createInfo = new VulkanSwapchainCreateInfo
            {
                VulkanPhysicalDevice = VulkanPhysicalDevice,
                VulkanLogicalDevice = VulkanLogicalDevice,
                VulkanSurface = VulkanInstance.VulkanSurface
            };

            VulkanSwapchain = new VulkanSwapchain();
            VulkanSwapchain.Create(createInfo);
        }

        #region private sector

        private void CreateInstance(INativeWindow vulkanMainWindow)
        {
            VulkanInstance = new VulkanInstance();

            var createInfo = new VulkanInstanceCreateInfo
            {
                IsDebugEnabled = SettingsManager.IsDebugEnabled,
                VulkanApiVersion = SettingsManager.VulkanApiVersion,
                ApplicationName = SettingsManager.ApplicationName,
                EngineName = SettingsManager.EngineName,
                ApplicationVersion = SettingsManager.ApplicationVersion,
                EngineVersion = SettingsManager.EngineVersion,
                RequestedExtensionNames = SettingsManager.RequestedInstanceExtensionNames,
                RequestedLayerNames = SettingsManager.RequestedInstanceLayerNames,
                VulkanWindow = vulkanMainWindow
            };

            VulkanInstance.Create(createInfo);
        }

        private void CreatePhysicalDevice()
        {
            var searchInfo = new VulkanPhysicalDeviceSearchInfo
            {
                IsRequestedSupportGraphicsQueue = true,
                IsRequestedSupportPresentationQueue = true,
                VulkanSurface = VulkanInstance.VulkanSurface,
                IsRequestedSupportTransferQueue = false, // пока нет нужды
                IsRequestedSupportComputeQueue = false, // пока нет нужды
                RequestedFeatures = new PhysicalDeviceFeatures(), // пока все false
                PreferredType = PhysicalDeviceType.DiscreteGpu,
                PreferredVulkanApiVersion = SettingsManager.VulkanApiVersion,
                RequestedExtensionNames = SettingsManager.RequestedPhysicalDeviceExtensionNames
            };

            var foundPhysicalDevice = VulkanInstance.FindSuitablePhysicalDevice(searchInfo);
            if (foundPhysicalDevice == null)
            {
                throw new Exception("Не найден подходящий видеоадаптер для работы с приложением.");
            }

            var createInfo = new VulkanPhysicalDeviceCreateInfo
            {
                VulkanInstance = VulkanInstance,
                VulkanSurface = VulkanInstance.VulkanSurface,
                PhysicalDevice = foundPhysicalDevice
            };

            VulkanPhysicalDevice = new VulkanPhysicalDevice();
            VulkanPhysicalDevice.Create(createInfo);
        }

        private void CreateLogicalDevice()
        {
            var createInfo = new VulkanLogicalDeviceCreateInfo
            {
                VulkanPhysicalDevice = VulkanPhysicalDevice,
                VulkanSurface = VulkanInstance.VulkanSurface,
                RequestedFeatures = new PhysicalDeviceFeatures(), // пока все false
                RequestedExtensionNames = SettingsManager.RequestedLogicalDeviceExtensionNames,
                IsRequestedCreateGraphicsQueue = true,
                IsRequestedCreateComputeQueue = true,
                IsRequestedCreateTransferQueue = true
            };

            VulkanLogicalDevice = new VulkanLogicalDevice();
            VulkanLogicalDevice.Create(createInfo);
        }

        #endregion

        public void WaitIdle()
        {
           VulkanLogicalDevice.Device.WaitIdle();
        }
    }
}