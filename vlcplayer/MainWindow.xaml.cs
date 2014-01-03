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

        public MainWindow()
        {
            InitializeComponent();

            _curIndex = 0;

            _playList = new List<string>();
//            _playList.Add(@"D:\Videos\FTISLAND - Severely.mp4");
//            _playList.Add(@"D:\Videos\miss A Bad Girl, Good Girl.mp4");
//            _playList.Add(@"D:\Videos\T-ARA(티아라) _ Sexy Love (Dance Ver. MV).mp4");
            _playList.Add(@"D:\Temp\o1928.mp3");
            _playList.Add(@"D:\Temp\o115.mp3");

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _server = new Server();
            Start();

            vlc = new AxVLCPlugin2();
            windowsFormsHost1.Child = vlc;
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
            if (_server.IsStarted)
                _server.Stop();

            if (_curIndex >= _playList.Count)
                _curIndex = 0;
            if (_curIndex < 0)
                _curIndex = _playList.Count - 1;

            string filename = _playList[_curIndex];

            if (filename != string.Empty)
            {
                _server.FileName = filename;
            }

            vlc.playlist.add("http://localhost:7896/");
            vlc.playlist.play();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            _curIndex = 0;
            UpdateContent();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            vlc.playlist.stop();
            _curIndex++;
            UpdateContent();
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            vlc.playlist.stop();
            _curIndex--;
            UpdateContent();
        }
    }
}
