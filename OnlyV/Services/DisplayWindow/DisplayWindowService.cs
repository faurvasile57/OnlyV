﻿namespace OnlyV.Services.DisplayWindow
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;
    using Behaviours;
    using GalaSoft.MvvmLight.Messaging;
    using Helpers.JwLib;
    using Monitors;
    using OnlyV.Themes.Common.Services.WindowPositioning;
    using Options;
    using PubSubMessages;
    using Serilog;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DisplayWindowService : IDisplayWindowService
    {
        private readonly IMonitorsService _monitorsService;
        private readonly IOptionsService _optionsService;

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
        private readonly (int dpiX, int dpiY) _systemDpi;
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly

        private Windows.DisplayWindow _displayWindow;

        public DisplayWindowService(
            IMonitorsService monitorsService,
            IOptionsService optionsService)
        {
            _monitorsService = monitorsService;
            _optionsService = optionsService;

            _systemDpi = WindowPlacement.GetDpiSettings();

            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        }

        public bool IsWindowVisible => _displayWindow != null && _displayWindow.IsVisible;

        public void ShowWindow()
        {
            EnsureWindowCreated();
            _displayWindow.Show();
        }

        public void CloseWindow()
        {
            _displayWindow?.Close();
            _displayWindow = null;
            
            Task.Delay(FadeCloseBehaviour.FadeTime).ContinueWith(t =>
            {
                BringJwlToFront(); 
            });
        }

        public void ToggleWindow()
        {
            if (IsWindowVisible)
            {
                CloseWindow();
            }
            else
            {
                ShowWindow();
            }
        }

        public void ClearImage()
        {
            if (_displayWindow != null)
            {
                _displayWindow.TheImage.Source = null;
            }
        }

        public void ChangeTargetMonitor()
        {
            if (IsWindowVisible)
            {
                var originalImage = _displayWindow.TheImage.Source;

                CloseWindow();

                if (_optionsService.MediaMonitorId != null)
                {
                    ShowWindow();
                }

                SetImage(originalImage);
            }
        }

        public void SetImage(ImageSource image)
        {
            if (_displayWindow != null)
            {
                _displayWindow.TheImage.Source = image;
            }
        }
        
        private void EnsureWindowCreated()
        {
            if (_displayWindow == null)
            {
                _displayWindow = new Windows.DisplayWindow();

                var targetMonitor = _monitorsService.GetSystemMonitor(_optionsService.MediaMonitorId);
                if (targetMonitor != null)
                {
                    Log.Logger.Information("Opening display window");

                    LocateWindowAtOrigin(_displayWindow, targetMonitor.Monitor);

                    _displayWindow.Topmost = true;
                }
            }
        }

        private void LocateWindowAtOrigin(Window window, Screen monitor)
        {
            var area = monitor.WorkingArea;

            var left = (area.Left * 96) / _systemDpi.dpiX;
            var top = (area.Top * 96) / _systemDpi.dpiY;

            Log.Logger.Verbose($"Monitor = {monitor.DeviceName} Left = {left}, top = {top}");

            // these seemingly redundant sizing statements are required!
            window.Left = 0;
            window.Top = 0;
            window.Width = 0;
            window.Height = 0;

            window.Left = left;
            window.Top = top;
        }

        private void OnShutDown(ShutDownMessage msg)
        {
            CloseWindow();
        }

        private void BringJwlToFront()
        {
            if (_optionsService.JwLibraryCompatibilityMode)
            {
                JwLibHelper.BringToFront();
                Thread.Sleep(100);
            }
        }
    }
}
