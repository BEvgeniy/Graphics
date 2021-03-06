﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Graphics.Engine.Settings;
using Graphics.Engine.VulkanDriver;

namespace Graphics.Engine
{
    internal class GraphicsEngine
    {
        private String[] _commandLineArguments;

        // https://blogs.msdn.microsoft.com/rickhos/2005/03/30/the-ideal-system-windows-forms-3d-gameloop-take-15/
        // https://gamedev.stackexchange.com/questions/67651/what-is-the-standard-c-windows-forms-game-loop
        private VulkanWindow _vulkanMainWindow;

        private VulkanManager _vulkanManager;

        private DateTime _dt = DateTime.Now;
        private Int64 _fps = 0;

        public GraphicsEngine(String[] args)
        {
            _commandLineArguments = args;
        }

        private void Init()
        {
            // Подгрузим настройки в наше приложение для использования. Этот шаг должен быть свегда первым.
            // Т.к. менеджер типа VulkanManager, также как и другие менеджеры используют загруженные 
            // настройки из файлов конфигураций
            SettingsManager.LoadSettings();
            _vulkanManager = new VulkanManager();
            _vulkanMainWindow = new VulkanWindow(Update, Render);
            _vulkanManager.Init(_vulkanMainWindow);
        }

        private void Load()
        {

        }

        public void Run()
        {
            Init();
            Load();
            _vulkanMainWindow.VSync = OpenTK.VSyncMode.Off;
            _vulkanMainWindow.Run(0, 0);
            _vulkanManager.WaitIdle();
            UnLoad();
            DeInit();
        }

        public void Render()
        {
            _vulkanManager.DrawFrame();
        }

        public void Update()
        {
            if ((DateTime.Now - _dt).TotalSeconds < 1)
            {
                _fps++;
            }
            else
            {
                _vulkanMainWindow.Title = _fps.ToString();
               
                _fps = 0;
                _dt = DateTime.Now;
            }
        }

        public void UnLoad()
        {

        }

        public void DeInit()
        {

        }
    }
}