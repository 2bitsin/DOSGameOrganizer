using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DosGameOrganizer
{
    public class Launcher
    {
        public Launcher()
        {
        }

        public Task Run(GameMetadataModel _gameMetadataModel, string _executable)
        {
           return Task.Run(() =>
           {
               var _archive = _gameMetadataModel.Archive;
               var _targetPath = $"{Properties.Settings.Default.ExtractionPath}\\{Path.GetFileNameWithoutExtension(_gameMetadataModel.Path)}";

               if (!Directory.Exists(_targetPath))
               {
                   Directory.CreateDirectory(_targetPath);
                   _archive.ExtractTo(_targetPath).Wait();
               }

               var _confDir = Path.GetDirectoryName(Properties.Settings.Default.DOSBoxPath) + "\\DOSBox.conf";
               var _process = Process.Start(Properties.Settings.Default.DOSBoxPath, $"\"{_targetPath}\\{_executable}\" -conf \"{_confDir}\"");
           });
        }
    }
}