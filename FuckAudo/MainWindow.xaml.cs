using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;

using System.Drawing;
using AudioSwitcher.AudioApi.CoreAudio;




namespace FuckAudo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();
    CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;

    public MainWindow()
    {
        InitializeComponent();


        this.Loaded += (o, r) =>
        {
           

            TrayTaskbarIcon.Icon = SystemIcons.Application;

            TrayTaskbarIcon.TrayMouseDoubleClick += (s, e) => this.Show();

            System.Windows.Application.Current.Exit += (s, e) => TrayTaskbarIcon.Dispose();

            SettingsMenuItem.Click += (s, e) => this.Show();

            CloseMenuItem.Click += (s, e) => System.Windows.Application.Current.Shutdown();

            ButtonTray.Click += (s, e) => this.Hide();
        };


    }




    void AudioFunc() {

        if (defaultPlaybackDevice.Volume == 100)
        {
            defaultPlaybackDevice.Mute(true);
        }
        else
        {
            defaultPlaybackDevice.Mute(false);
            defaultPlaybackDevice.Volume = 100;
        }

        System.Threading.Thread.Sleep(1000); // 매 1초 확인
    }

}

