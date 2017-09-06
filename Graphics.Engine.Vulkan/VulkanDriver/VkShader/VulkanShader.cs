using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Graphics.Engine.Settings;
using Graphics.Engine.VulkanDriver.VkDevice.Logical;
using Vulkan;

namespace Graphics.Engine.VulkanDriver.VkShader
{
    //This will hold our shader code in a nice clean class
    //this example only uses a shader with position and color
    //but didnt want to leave out the other bits for the shader
    //so you could practice writing a shader on your own :P
    internal class VulkanShader
    {
        /// <summary>
        /// Логическое устройство для выбранного видеоадаптера. 
        /// </summary>
        public VulkanLogicalDevice VulkanLogicalDevice { get; private set; }

        public Byte[] VertexSource { get; private set; }
        public Byte[] FragmentSource { get; private set; }

        public String VertexFileName { get; private set; }
        public String FragmentFileName { get; private set; }

        public ShaderModule VertexModule { get; private set; }
        public ShaderModule FragmentModule { get; private set; }

        //public Int32 VertexID { get; private set; }
        //public Int32 FragmentID { get; private set; }

        //public Int32 ProgramID { get; private set; }

        //public Int32 PositionLocation { get; set; }
        //public Int32 NormalLocation { get; set; }
        //public Int32 TexCoordLocation { get; set; }
        //public Int32 ColorLocation { get; set; }

        public VulkanShader()
        {
        }

        public void Create(VulkanShaderCreateInfo vulkanShaderCreateInfo)
        {
            VulkanLogicalDevice = vulkanShaderCreateInfo.VulkanLogicalDevice;
            VertexSource = CompileShader(vulkanShaderCreateInfo.VertexFileName);
            FragmentSource = CompileShader(vulkanShaderCreateInfo.FragmentFileName);

            var createVertexInfo = new ShaderModuleCreateInfo {CodeBytes = VertexSource};
            VertexModule = VulkanLogicalDevice.Device.CreateShaderModule(createVertexInfo);

            var createFragmentInfo = new ShaderModuleCreateInfo {CodeBytes = FragmentSource};
            FragmentModule = VulkanLogicalDevice.Device.CreateShaderModule(createFragmentInfo);
        }

        private Byte[] CompileShader(String shaderFileNameWithoutPathWithExtention)
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var shaderFileWithPath = Path.Combine(path,
                Path.Combine("Shaders", shaderFileNameWithoutPathWithExtention));
            var outShaderFileWithPath = Path.Combine(path, Path.Combine(Path.Combine("Shaders", "Compiled"),
                Path.GetFileNameWithoutExtension(shaderFileNameWithoutPathWithExtention) + ".spv"));

            var cmd = "-V \"" + shaderFileWithPath + "\" -o \"" + outShaderFileWithPath + "\"";
            var processVS = new Process
            {
                StartInfo =
                {
                    FileName = "glslangValidator.exe",
                    WorkingDirectory = path,
                    Arguments = cmd,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };

            processVS.Start();

            if (SettingsManager.IsDebugEnabled)
            {
                var output = processVS.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            processVS.WaitForExit();

            return processVS.ExitCode != 0 ? null : File.ReadAllBytes(outShaderFileWithPath);
        }

        public void Dispose()
        {
          
        }
    }
}
