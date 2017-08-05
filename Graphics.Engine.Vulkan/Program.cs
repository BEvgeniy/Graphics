using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphic.Engine.VulkanDriver;
using Vulkan1 = Vulkan;

namespace Graphic.Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            //var hWnd = new System.Windows.Interop.WindowInteropHelper(this).EnsureHandle();
            //var hInstance = System.Runtime.InteropServices.Marshal.GetHINSTANCE(typeof(App).Module);

            #region Создадим экземпляр Vulkan'a

            try
            {
                VulkanManager.Init();
            }
            catch (Vulkan1.ResultException e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                return;
            }

            #endregion

           
            //var physicalDevices = instance.EnumeratePhysicalDevices();
            
            //if (physicalDevices.Length <= 0)
            //{
            //    throw new Exception("В системе не установлено подходящее устройство для отрисовки графики");
            //}
            //var videoCard = physicalDevices[0];

            ////var physicalDevicePropsArray = videoCard.GetProperties();
            //var queueFamilyPropsArray = videoCard.GetQueueFamilyProperties();
            //var graphicFamilyPropertiesIndex = (UInt32?)null;
            //for (UInt32 i = 0; i < queueFamilyPropsArray.Length; i++)
            //{
            //    if ((queueFamilyPropsArray[i].QueueFlags & Vulkan1.QueueFlags.Graphics) !=
            //        Vulkan1.QueueFlags.Graphics)
            //    {
            //        continue;
            //    }
            //    graphicFamilyPropertiesIndex = i;
            //    break;
            //}
            //if (!graphicFamilyPropertiesIndex.HasValue)
            //{
            //    throw new Exception("Не удалось найти семейство для отрисовки графики");
            //}
            //var deviceQueueInfo = new Vulkan1.DeviceQueueCreateInfo
            //{
            //    QueueCount = 1,
            //    QueueFamilyIndex = graphicFamilyPropertiesIndex.Value,
            //    QueuePriorities = new[] {1.0f}
            //};
            //var deviceInfo = new Vulkan1.DeviceCreateInfo
            //{
            //    QueueCreateInfoCount = 1,
            //    QueueCreateInfos = new[] {deviceQueueInfo}
            //};
            //Vulkan1.Device device = null;
            //try
            //{
            //    device = videoCard.CreateDevice(deviceInfo);
            //}
            //catch (Vulkan1.ResultException e)
            //{
            //    Console.WriteLine(e);
            //    Console.ReadKey();
            //    return;
            //}

        }
    }
}