using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Graphics.Engine.VulkanDriver;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenGL.CSharp.Engine
{
    internal class GL4Window : GameWindow, INativeWindow
    {
        private readonly VulkanManager _vulkanManager;

        public GL4Window(VulkanManager vulkanManager, int width = 600, int height = 400)
            : base(width, height,
                OpenTK.Graphics.GraphicsMode.Default,
                "Tutorial GL4 Window",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                //Major Minor implicitly assigned to 4.0
                //It's best to set to your version of GL
                //so look at the method below for help.
                //**do not set to a version above your own
                4, 5,
                //Make sure that we are only using 4.0 related stuff.
                OpenTK.Graphics.GraphicsContextFlags.Debug |
                OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible)
        {
            _vulkanManager = vulkanManager;
            VSync = VSyncMode.Off;
            
            #region GL_VERSION

            //this will return your version of opengl
            //int major, minor;
            //GL.GetInteger(GetPName.MajorVersion, out major);
            //GL.GetInteger(GetPName.MinorVersion, out minor);
            //Console.WriteLine("Major {0}\nMinor {1}", major, minor);
            //you can also get your GLSL version, although not sure if it varies from the above
            //Console.WriteLine("GLSL {0}", GL.GetString(StringName.ShadingLanguageVersion));
            //Console.WriteLine("Vendor {0}", GL.GetString(StringName.Vendor));
            //Console.WriteLine("Version {0}", GL.GetString(StringName.Version));
            //Console.WriteLine("Renderer {0}", GL.GetString(StringName.Renderer));
            //Console.WriteLine("Extensions {0}", GL.GetString(StringName.Extensions));

            #endregion
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

        }
        private DateTime _dt = DateTime.Now;
        private Int64 _fps = 0;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            if ((DateTime.Now - _dt).TotalSeconds < 1)
            {
                _fps++;
                _vulkanManager.DrawFrame();
            }
            else
            {
                Title = _fps.ToString();
                _fps = 0;
                _dt = DateTime.Now;
            }

        }
    }
}