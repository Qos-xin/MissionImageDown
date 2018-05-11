using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace MissionImageDown
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            SelectFileCommand = new DelegateCommand(OnSelectFile);
            StartDownImage = new DelegateCommand(OnDownImage);
        }
        public DelegateCommand SelectFileCommand { get; }
        public DelegateCommand StartDownImage { get; }

        public string fileName;
        private List<RecordInfo> recordInfo;

        public string FileName
        {
            get { return fileName; }
            set { SetProperty(ref fileName, value); }
        }

        public List<RecordInfo> RecordInfo
        {
            get => recordInfo;
            set => SetProperty(ref recordInfo, value);
        }
        private void OnSelectFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "csv文件|*.csv";
            if (openFileDialog.ShowDialog().Value)
            {
                FileName = openFileDialog.FileName;
                RecordInfo = File.ReadAllLines(FileName).Select(k =>
                {
                    var t = k.Split(',');
                    if (t.Count() > 3)
                    {
                        return new RecordInfo() { Phone = t[0], CTime = DateTime.Parse(t[1]), UniqueKey = t[2], Fileds = t };
                    }
                    else
                    {
                        return null;
                    }
                }).ToList();
            }
        }
        private void OnDownImage()
        {
            var filePath = System.IO.Path.GetDirectoryName(FileName);
            var newPath = System.IO.Path.Combine(filePath, System.IO.Path.GetFileNameWithoutExtension(FileName));
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            Random random = new Random(124);

            foreach (var item in RecordInfo)
            {
                if (item.Fileds.Count() > 3 && !string.IsNullOrWhiteSpace(item.UniqueKey))
                {
                    var randomExt = random.Next(1234, 9876);
                    var i = 1;
                    var file = System.IO.Path.Combine(newPath, $"{item.UniqueKey}_{randomExt}_{i++}");
                    foreach (var str in item.Fileds.Where(k => k.ToLower().StartsWith("http")))
                    {
                        DownFile(str, file + System.IO.Path.GetExtension(str));
                        file = System.IO.Path.Combine(newPath, $"{item.UniqueKey}_{randomExt}_{i++}");
                    }
                }
                item.State = true;
            }
        }
        public void DownFile(string url, string fileName)
        {
            WebClient wc = new WebClient();
            wc.DownloadFileAsync(new Uri(url), fileName);

        }






        private void SetProperty<T>(ref T fileName, T value, [CallerMemberName] string propertyName = null)
        {
            fileName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;      

    }
    public class RecordInfo : BindableBase
    {
        private string phone;
        private DateTime cTime;
        private string uniqueKey;
        private bool state;
        private string[] fileds;

        public string Phone { get => phone; set => SetProperty(ref phone, value); }
        public DateTime CTime { get => cTime; set => SetProperty(ref cTime, value); }
        public string UniqueKey { get => uniqueKey; set => SetProperty(ref uniqueKey, value); }
        public bool State { get => state; set => SetProperty(ref state, value); }
        public string[] Fileds { get => fileds; set => SetProperty(ref fileds, value); }
    }
}
