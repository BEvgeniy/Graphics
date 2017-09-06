using System;
using OpenTK;

namespace Graphics.Engine
{
    /// <summary>
    /// Данное окно является частью библиотеки OpenTk - работает с OpenGL,
    /// но, так как окно является очень удобным для использования с Vulkan
    /// и не требует специальных усилий по внедрению кода и работы с ним,
    /// решено использовать иммено его. 
    /// При создании окна, также создается контекст OpenGL,
    /// но на это мы подействовать не можем поэтому просто игнорируем этот факт.
    /// </summary>
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