using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Graphics.Engine.Settings;
using Graphics.Engine.VulkanDriver;

namespace Graphics.Engine
{
    public class GraphicsEngine
    {
        private String[] _commandLineArguments;
        // https://blogs.msdn.microsoft.com/rickhos/2005/03/30/the-ideal-system-windows-forms-3d-gameloop-take-15/
        // https://gamedev.stackexchange.com/questions/67651/what-is-the-standard-c-windows-forms-game-loop
        private VulkanWindow _vulkanMainWindow;

        private DateTime _dt = DateTime.Now;
        private Int64 _fps = 0;

        public GraphicsEngine(String[] args)
        {
            _commandLineArguments = args;
        }

        private void Init()
        {
            //var hInstance = System.Runtime.InteropServices.Marshal.GetHINSTANCE(typeof(App).Module);
            // Подгрузим настройки в наше приложение для использования. Этот шаг должен быть свегда первым.
            // Т.к. менеджер типа VulkanManager, также как и другие менеджеры используют загруженные настройки из файлов конфигураций
            SettingsManager.LoadSettings();
            // Создаем экземпляр окна 
            _vulkanMainWindow = new VulkanWindow(OnFrame);
            // Теперь проинициализируем Vulkan
            var vulkanManager = new VulkanManager();
            vulkanManager.Init(_vulkanMainWindow);
        }

        private void Load()
        {
            
        }

        public void Run()
        {
            Init();
            Load();
            _vulkanMainWindow.Show();
             Application.Run(_vulkanMainWindow);
            _vulkanMainWindow.Hide();
            UnLoad();
            DeInit();
        }
        
        private void OnFrame()
        {
            if ((DateTime.Now - _dt).TotalSeconds < 1)
            {
                Update();
                Render();
                _fps++;
            }
            else
            {
                _vulkanMainWindow.Text = _fps.ToString();
                _vulkanMainWindow.Update();
                _fps = 0;
                _dt = DateTime.Now;
            }   
        }

        private void Render()
        {
           
        }

        private void Update()
        {
            
        }

        private void UnLoad()
        {

        }

        private void DeInit()
        {

        }
    }
}
