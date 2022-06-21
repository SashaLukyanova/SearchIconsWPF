using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SearchIcons
{
    internal class IconViewModel
    {
        public List<string> _iconNames;
        public ObservableCollection<Icon> Icons { get; set; }// = new ObservableCollection<Icon>();

        private const string ApplicationDirectory = @"\SearchIcons\bin\Debug";
        private const string TargetFolder = @"\[Common]\.NET\Images\";

        public IconViewModel()
        {
            
        }

        public async Task Init()
        {
            var icons = await GetUnusedIcons();
            Icons = new ObservableCollection<Icon>(icons);
        }

        public async Task<ObservableCollection<Icon>> GetUnusedIcons()
        {
            var methods = new MethodsHelper();
            var roslyn = new ReferenceFinder();
            var temp = new ObservableCollection<Icon>();

            var collection = GetIcons();

            var unusedList = methods.GetUnusedXAMLIcons(_iconNames); //поиск неиспользуемых иконок в xaml файлах
            var usedListRoslyn = await roslyn.FindUsingIconsAsync();

            foreach (var icon in collection)
            {
                foreach (var iconList in unusedList)
                {
                    if (icon.Name == iconList)
                    {
                        foreach (var iconRoslyn in usedListRoslyn)
                        {
                            if (icon.Name == iconRoslyn)
                            {
                                temp.Add(icon);
                            }
                        }
                    }
                }

            }
            return temp;
        }

        //коллекция иконок, которые находятся в папках изображений проектов
        public ObservableCollection<Icon> GetIcons()
        {
            var icons = new ObservableCollection<Icon>();
            try
            {                
                _iconNames = new List<string>();
                var iconsFolder = GetFiles("Images");
                var kzDreamIconsFolder = GetFiles("KzDreamImages");

                foreach (var item in iconsFolder)
                {
                    var icon = new Icon(item);
                    icons.Add(icon);
                    _iconNames.Add(icon.Name);
                }

                foreach (var item in kzDreamIconsFolder)
                {
                    var icon = new Icon(item);
                    icons.Add(icon);
                    _iconNames.Add(icon.Name);
                }

            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("No Such Directory");
            }
            return icons;
        }

        public string[] GetFiles(string postfix)
        {
            var files = Directory.GetFiles(SetFilesPath() + postfix, "*.*", SearchOption.AllDirectories);

            var imageFiles = new List<string>();
            foreach (var filename in files)
            {
                if (Regex.IsMatch(filename, @".jpg|.gif|.PNG|.png$"))
                {
                    imageFiles.Add(filename);
                }
            }

            return imageFiles.ToArray();
        }

        private string SetFilesPath()
        {
            var fullPath = Directory.GetCurrentDirectory();
            var targetPath = fullPath.Replace(ApplicationDirectory, TargetFolder);

            return targetPath;
        }
       
    }
}
