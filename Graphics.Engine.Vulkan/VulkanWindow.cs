using System;
using Graphics.Engine.VulkanDriver;
using OpenTK;

namespace Graphics.Engine
{
    internal class VulkanWindow : GameWindow, INativeWindow
    {
        private readonly Action _onUpdate;
        private readonly Action _onRender;

        public VulkanWindow(Action onUpdate, Action onRender, Int32 width = 600, Int32 height = 400)
            : base(width, height,
                OpenTK.Graphics.GraphicsMode.Default,
                "Tutorial Vulkan Window",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                //Major Minor implicitly assigned to 4.0
                //It's best to set to your version of GL
                //so look at the method below for help.
                //**do not set to a version above your own
                3, 1,
                //Make sure that we are only using 4.0 related stuff.
                OpenTK.Graphics.GraphicsContextFlags.Default)
        {
            _onUpdate = onUpdate;
            _onRender = onRender;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            _onUpdate?.Invoke();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            _onRender?.Invoke();
        }
    }
}