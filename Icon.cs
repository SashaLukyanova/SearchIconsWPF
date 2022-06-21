using System;
using System.Linq;
using System.Windows.Media.Imaging;

namespace SearchIcons
{
    public class Icon
    {
        //private readonly Uri _uri;

        public Icon(string path)
        {
            Name = GetIconName(path);
            Path = path;
            //_uri = new Uri(@"pack://application:,,,/Falcongaze.SecureTower.Images;component/Images/128/AllUnknownUsers.png", UriKind.RelativeOrAbsolute);
            //uri = new Uri(path);
            Image = BitmapFrame.Create(new Uri(path));
        }

        public string Path { get; }
        public string Name { get; }
        public BitmapFrame Image { get; set; }
        
        private string GetIconName(string path)
        {
            var shortName = path.Split('\\').Last();
            var temp = path.Substring(0, path.Length - shortName.Length - 1);
            var tempFullName = shortName.Replace(shortName.Split('.').Last(), temp.Split('\\').Last());
            var fullName = tempFullName.Replace('.', '_');
            return fullName;
        }
    }
    
}
