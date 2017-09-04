using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphics.Engine.VulkanDriver.VkDevice.Logical;

namespace Graphics.Engine.VulkanDriver.VkShader
{
    internal sealed class VulkanShaderCreateInfo
    {
        public VulkanLogicalDevice VulkanLogicalDevice { get; set; }
        public String VertexFileName { get; set; }
        public String FragmentFileName { get; set; }
    }
}