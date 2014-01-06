using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using AxAXVLC;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace vlcplayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AxVLCPlugin2 vlc;
        private List<string> _playList;
        private Server _server;
        private int _curIndex;

        private List<PortableDevice.PortableDeviceObject> _portableList;

        private event EventHandler LoadedCompleted;
        private PortableDevice.PortableDevice _device;

        public MainWindow()
        {
            InitializeComponent();

            _curIndex = 0;

            _playList = new List<string>();
//            _playList.Add(@"D:\Videos\FTISLAND - Severely.mp4");
//            _playList.Add(@"D:\Videos\20130907162142.m2ts");
            _playList.Add(@"D:\Videos\Lady Antebellum - Just A Kiss.mp4");
//            _playList.Add(@"D:\Videos\miss A Bad Girl, Good Girl.mp4");
            _playList.Add(@"D:\Videos\T-ARA(티아라) _ Sexy Love (Dance Ver. MV).mp4");
            _playList.Add(@"D:\Temp\o1928.mp3");
            _playList.Add(@"D:\Temp\o115.mp3");

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Closed += new EventHandler(MainWindow_Closed);

            LoadedCompleted += new EventHandler(MainWindow_LoadedCompleted);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _server = new Server();
            Start();

            vlc = new AxVLCPlugin2();
            windowsFormsHost1.Child = vlc;

//            foreach (string item in _playList)
//                listBox1.Items.Add(item.ToString());

            Thread loadItems = new Thread(new ThreadStart(LoadPortableDevice));
            loadItems.Name = "LoadItems";
            loadItems.Start();
        }


        void MainWindow_Closed(object sender, EventArgs e)
        {
            _server.Stop();
            _server.Dispose();
        }

        private void Start()
        {
            Thread startServer = new Thread(new ThreadStart(StartServer));
            startServer.Name = "StartServer";
            startServer.Start();
        }

        private void StartServer()
        {
            _server.Start();
        }

        private void UpdateContent()
        {
            if (vlc.playlist.isPlaying)
                vlc.playlist.stop();

            if (_server.IsStarted)
                _server.Stop();

            if (_curIndex >= _portableList.Count)
                _curIndex = 0;
            if (_curIndex < 0)
                _curIndex = _portableList.Count - 1;

            var item = _portableList[_curIndex] as PortableDevice.PortableDeviceFile;

            MemoryStream ms = _device.GetMemoryStream(item);

            ms.Position = 0;

            _server.memoryStream = ms;

//            string filename = _playList[_curIndex];
/*
            if (filename != string.Empty)
            {
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                MemoryStream memoryStream = new MemoryStream();
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
                memoryStream.Write(bytes, 0, (int)fs.Length);
//                byte[] bytes = new byte[fs.Length];
//                fs.Read(bytes, 0, (int)fs.Length);
//                ms.Write(bytes, 0, (int)fs.Length);
                memoryStream.Position = 0;

                _server.memoryStream = memoryStream;
//                _server.fileStream = fs;

//                _server.FileName = filename;
//                _server.ms = 
            }
*/
            try
            {
                vlc.playlist.add("http://localhost:7896/");
                vlc.playlist.play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            listBox1.SelectedIndex = _curIndex;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            _curIndex = 0;
            UpdateContent();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            _curIndex++;
            UpdateContent();
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            _curIndex--;
            UpdateContent();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _curIndex = listBox1.SelectedIndex;
            System.Diagnostics.Debug.WriteLine(_curIndex);
            UpdateContent();
        }

        void MainWindow_LoadedCompleted(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("LoadedCompleted");

            Dispatcher.BeginInvoke(new EventHandler(SAFE_MainWindow_LoadedCompleted), sender, e);
        }

        void SAFE_MainWindow_LoadedCompleted(object sender, EventArgs e)
        {
            foreach (var item in _portableList)
            {
                listBox1.Items.Add(item.Name.ToString());
            }        
        }

        public void DisplayObject(PortableDevice.PortableDeviceObject portableDeviceObject)
        {
            if (portableDeviceObject is PortableDevice.PortableDeviceFolder)
                DisplayFolderContents((PortableDevice.PortableDeviceFolder)portableDeviceObject);
        }

        public void DisplayFolderContents(PortableDevice.PortableDeviceFolder folder)
        {
            foreach (var item in folder.Files)
            {
                if (item is PortableDevice.PortableDeviceFolder)
                    DisplayFolderContents((PortableDevice.PortableDeviceFolder)item);
                else if (item is PortableDevice.PortableDeviceFile)
                {
                    if (item.Name.Contains("mp3"))
                        _portableList.Add(item as PortableDevice.PortableDeviceObject);
                    if (item.Name.Contains("mp4"))
                        _portableList.Add(item as PortableDevice.PortableDeviceObject);
                }
            }
        }

        private void LoadPortableDevice()
        {
            var devices = new PortableDevice.PortableDeviceCollection();
            devices.Refresh();
            _device = devices.First();
            _device.Connect();

            _portableList = new List<PortableDevice.PortableDeviceObject>();

            var folder = _device.GetContents();

            foreach (var item in folder.Files)
            {
                DisplayObject(item);
            }

            LoadedCompleted(this, new EventArgs());
        }
    }
}
