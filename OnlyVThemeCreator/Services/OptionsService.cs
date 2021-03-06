﻿namespace OnlyVThemeCreator.Services
{
    using System;
    using System.IO;
    using Helpers;
    using Newtonsoft.Json;
    using Serilog;

    internal class OptionsService : IOptionsService
    {
        private readonly int _optionsVersion = 1;
        private AppOptions.Options _options;
        private string _optionsFilePath;
        private string _originalOptionsSignature;

        public OptionsService()
        {
            Init();
        }

        public event EventHandler EpubPathChangedEvent;

        public string AppWindowPlacement
        {
            get => _options.AppWindowPlacement;
            set
            {
                if (_options.AppWindowPlacement == null || _options.AppWindowPlacement != value)
                {
                    _options.AppWindowPlacement = value;
                }
            }
        }

        public string EpubPath
        {
            get => _options.EpubPath;
            set
            {
                if (_options.EpubPath == null || _options.EpubPath != value)
                {
                    _options.EpubPath = value;
                    EpubPathChangedEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        
        public void Save()
        {
            try
            {
                var newSignature = GetOptionsSignature(_options);
                if (_originalOptionsSignature != newSignature)
                {
                    // changed...
                    WriteOptions();
                    Log.Logger.Information("Settings changed and saved");
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not save settings");
            }
        }

        private string GetOptionsSignature(AppOptions.Options options)
        {
            return JsonConvert.SerializeObject(options);
        }

        private void Init()
        {
            if (_options == null)
            {
                try
                {
                    _optionsFilePath = FileUtils.GetUserOptionsFilePath(_optionsVersion);
                    var path = Path.GetDirectoryName(_optionsFilePath);
                    if (path != null)
                    {
                        FileUtils.CreateDirectory(path);
                        ReadOptions();
                    }

                    if (_options == null)
                    {
                        _options = new AppOptions.Options();
                    }

                    // store the original settings so that we can determine if they have changed
                    // when we come to save them
                    _originalOptionsSignature = GetOptionsSignature(_options);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Could not read options file");
                    _options = new AppOptions.Options();
                }
            }
        }

        private void ReadOptions()
        {
            if (!File.Exists(_optionsFilePath))
            {
                WriteDefaultOptions();
            }
            else
            {
                using (var file = File.OpenText(_optionsFilePath))
                {
                    var serializer = new JsonSerializer();
                    _options = (AppOptions.Options)serializer.Deserialize(file, typeof(AppOptions.Options));

                    _options.Sanitize();
                }
            }
        }

        private void WriteDefaultOptions()
        {
            _options = new AppOptions.Options();
            WriteOptions();
        }

        private void WriteOptions()
        {
            if (_options != null)
            {
                using (var file = File.CreateText(_optionsFilePath))
                {
                    var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                    serializer.Serialize(file, _options);
                    _originalOptionsSignature = GetOptionsSignature(_options);
                }
            }
        }
    }
}
