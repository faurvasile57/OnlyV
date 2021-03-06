﻿namespace OnlyV.Services.Images
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows.Media.Imaging;
    using Helpers;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using OnlyV.Themes.Common.Services;

    internal class ImageSavingService : IImageSavingService
    {
        private readonly IReadOnlyCollection<BitmapSource> _images;
        private readonly string _folder;
        private readonly string _scriptureText;
        private string _currentDestinationFolder;

        public ImageSavingService(
            IReadOnlyCollection<BitmapSource> images, 
            string folder, 
            string scriptureText)
        {
            _images = images;
            _folder = folder;
            _scriptureText = scriptureText;
        }

        public static string GetSuitableFilenameWithoutExtension(string bibleBookChapterAndVersesString)
        {
            var s = bibleBookChapterAndVersesString.Replace(":", " v ").Replace(".", string.Empty);
            return FileUtils.CleanFileName(s);
        }

        public string GetFolder()
        {
            string result = null;

            var dlg = new CommonOpenFileDialog
            {
                Title = Properties.Resources.DEST_FOLDER,
                IsFolderPicker = true,
                InitialDirectory = _currentDestinationFolder ?? FileUtils.GetDefaultSaveToFolder(),
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _currentDestinationFolder = dlg.FileName;
                result = _currentDestinationFolder;
            }

            return result;
        }

        public string Execute()
        {
            string result = string.Empty;

            if (_images.Any())
            {
                var baseFileName = GetSuitableFilenameWithoutExtension(_scriptureText);

                if (_images.Count == 1)
                {
                    var path = Path.Combine(_folder, Path.ChangeExtension(baseFileName, ".png"));
                    BitmapWriter.WritePng(path, _images.First());
                    result = path;
                }
                else
                {
                    string folder = Path.Combine(_folder, baseFileName);
                    if (Directory.Exists(folder))
                    {
                        ClearFiles(folder);
                    }
                    else
                    {
                        Directory.CreateDirectory(folder);
                    }

                    if (Directory.Exists(folder))
                    {
                        if (Directory.EnumerateFiles(folder).Any())
                        {
                            throw new Exception("Could not clear folder!");
                        }

                        result = folder;

                        int count = 1;
                        foreach (var image in _images)
                        {
                            var baseNameWithDigitPrefix = $"{count:D3} {baseFileName}";
                            var path = Path.Combine(folder, Path.ChangeExtension(baseNameWithDigitPrefix, ".png"));
                            BitmapWriter.WritePng(path, image);

                            ++count;
                        }
                    }
                }
            }

            return result;
        }

        private void ClearFiles(string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*.png");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
