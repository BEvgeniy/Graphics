using System;

namespace Graphics.Engine.VulkanDriver
{
    internal sealed class VulkanPhysicalDeviceQueueFamiliesParams
    {
        public VulkanPhysicalDeviceQueueFamiliesParams()
        {
            IsSupportGraphics = false;
            IsSupportCompute = false;
            IsSupportTransfer = false;
            GraphicsIndex = -1;
            ComputeIndex = -1;
            TransferIndex = -1;
        }

        /// <summary>
        /// Устанавливается, если физическое устройство поддерживает работу с графическими командами (т.е. имеет симейство очередей, которое может работать с ними)
        /// </summary>
        public Boolean IsSupportGraphics { get; set; }
        /// <summary>
        /// Устанавливается, если физическое устройство поддерживает работу с командами вычисления (т.е. имеет симейство очередей, которое может работать с ними)
        /// </summary>
        public Boolean IsSupportCompute { get; set; }
        /// <summary>
        /// Устанавливается, если физическое устройство поддерживает работу с командами передачи, такие как для работы с памятью, напрмер, копирование (т.е. имеет симейство очередей, которое может работать с ними)
        /// </summary>
        public Boolean IsSupportTransfer { get; set; }
        /// <summary>
        /// Если физическое устройство поддерживает работу с графическими командами, 
        /// то данное свойство содержит индекс указывающий на это семейство очередей
        /// </summary>
        public Int32 GraphicsIndex { get; set; }
        /// <summary>
        /// Если физическое устройство поддерживает работу с командами вычисления, 
        /// то данное свойство содержит индекс указывающий на это семейство очередей
        /// </summary>
        public Int32 ComputeIndex { get; set; }
        /// <summary>
        /// Если физическое устройство поддерживает работу с командами передачи, 
        /// то данное свойство содержит индекс указывающий на это семейство очередей
        /// </summary>
        public Int32 TransferIndex { get; set; }

    }
}
