using System;
using Vulkan;

namespace Graphics.Engine
{
    internal class Program
    {
        private static void Main(String[] args)
        {
            #region Создадим экземпляр Vulkan'a

            try
            {
                new GraphicsEngine(args).Run();
            }
            catch (ResultException e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }

            #endregion
        }
    }
}