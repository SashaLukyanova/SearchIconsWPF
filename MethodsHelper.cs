using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace SearchIcons
{
    internal class MethodsHelper
    {
        const string TargetFolder = @"[Common]\.NET\Images\";
        const string GenerateFile = @"[Common]\.NET\Images\obj\Debug\GeneratedImagesPalettes\";
        const string ImagesFolder = "Images";
        const string KzDreamImagesFolder = "KzDreamImages";
        const string FileName = "ImagesType.g.cs";

        public List<string> _iconNames;
        public IconViewModel _iconViewModel;

        public void CopyTargetFile()
        {
            var baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var basePath = baseDirectory.Split(System.AppDomain.CurrentDomain.FriendlyName)[0];

            var sourceFile = Path.Combine(basePath, GenerateFile, FileName);
            var destFile = Path.Combine(basePath, TargetFolder, FileName);

            if (!File.Exists(destFile))
            {
                File.Copy(sourceFile, destFile);
            }
        }

        public void DeleteTargetFile()
        {
            var baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var basePath = baseDirectory.Split(System.AppDomain.CurrentDomain.FriendlyName)[0];

            var targetPath = Path.Combine(basePath, TargetFolder, FileName);

            if (File.Exists(targetPath))
            {
                try
                {
                    File.Delete(targetPath);
                }
                catch (IOException e)
                {
                    MessageBox.Show($"Error while deleting file: {e}");
                    return;
                }
            }
        }

        private string SetFilesPath()
        {
            var baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var basePath = baseDirectory.Split(System.AppDomain.CurrentDomain.FriendlyName)[0];

            return basePath;
        }

        public List<string> GetXamlFiles()
        {
            var targetPath = SetFilesPath();

            var files = Directory.GetFiles(targetPath, "*.*", SearchOption.AllDirectories);

            var xamlFiles = new List<string>();
            foreach (var filename in files)
            {
                if (Regex.IsMatch(filename, @".xaml$"))
                {
                    xamlFiles.Add(filename);
                }
            }

            return xamlFiles;
        }       

        /////////////////////////////////////////////

        //поиск неиспользуемых иконок в xaml файлах
        public List<string> GetUnusedXAMLIcons(List<string> iconNames)
        {
            var xamlFiles = GetXamlFiles();

            var fileXamlText = new StringBuilder();

            foreach (var fileItem in xamlFiles)
            {
                var textFileStream = File.OpenText(fileItem);
                fileXamlText.Append(textFileStream.ReadToEnd());
                textFileStream.Close();
            }

            var unusedResources = new List<string>();
            var searchXamlText = fileXamlText.ToString();

            foreach (var resourceString in iconNames)
            {
                var resourceCall = "ImagesType." + resourceString; // find code reference to the resource name
                if (!searchXamlText.Contains(resourceCall))
                {
                    unusedResources.Add(resourceString);
                }
            }

            return unusedResources;
        }

        public List<string> ConfigureName(string valueText)
        {
            var result = new List<string>();

            var sizes = new string[] { "16", "24", "32" };
            var temp = valueText.Split('_').Last();

            if (sizes.Contains(temp))
            {
                foreach (var size in sizes)
                {
                    if (size == temp)
                    {
                        continue;
                    }
                    result.Add(valueText.Replace(temp, size));
                }
            }
                
            return result;
        }
    }
}
