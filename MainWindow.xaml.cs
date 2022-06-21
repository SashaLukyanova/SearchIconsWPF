using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SearchIcons
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IconViewModel IconViewModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            
            var test = new ReferenceFinder();
            var mh = new MethodsHelper();
            var newIcons = new IconViewModel();            
            Task.Run(() => newIcons.Init());
            IconViewModel = newIcons;
            DataContext = IconViewModel;            
        }

        private void SaveFiles_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Icons";
            dlg.DefaultExt = ".docx";
            dlg.Filter = "Text documents (.doc)|*.doc";

            var result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                var i = 1;
                var list = new List<string>();
                foreach (var iconPath in IconViewModel.Icons)
                {
                    list.Add($"{i}) " + iconPath.Path);
                    i++;
                }
                
                File.WriteAllLines(filename, list);
                 

            }
        }
    }

}
