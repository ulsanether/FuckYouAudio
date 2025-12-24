using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NAudio.CoreAudioApi;
using MaterialDesignThemes.Wpf;

namespace FuckAuido
{

    public partial class MainWindow : Window
    {
        // NAudio - CoreAudioApi
        private MMDeviceEnumerator deviceEnumerator;
        private MMDevice defaultDevice;
        private DispatcherTimer volumeCheckTimer;

        private bool isMuted = false;
        private double previousVolume = 70;
        private bool isUpdatingSlider = false;

        public MainWindow()
        {
            InitializeComponent();

     
            deviceEnumerator = new MMDeviceEnumerator();
            defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

  
            volumeCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            volumeCheckTimer.Tick += VolumeCheckTimer_Tick;

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
    
            SetupHelper.InitializeResources();

       
            try
            {
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "volume-icon.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    TrayTaskbarIcon.Icon = new System.Drawing.Icon(iconPath);
                }
                else
                {
                 
                    TrayTaskbarIcon.Icon = IconHelper.CreateVolumeIcon(70, false);
                }
            }
            catch
            {
              
                TrayTaskbarIcon.Icon = System.Drawing.SystemIcons.Application;
            }

        
            TrayTaskbarIcon.TrayMouseDoubleClick += (s, e) => ShowWindow();
            System.Windows.Application.Current.Exit += (s, e) =>
            {
                volumeCheckTimer?.Stop();
                TrayTaskbarIcon?.Dispose();
                defaultDevice?.Dispose();
                deviceEnumerator?.Dispose();
            };

           
            var trayMenu = (ContextMenu)this.Resources["TrayMenu"];
            var informationMenuItem = (MenuItem)trayMenu.Items[0];
            var settingsMenuItem = (MenuItem)trayMenu.Items[1];
            var closeMenuItem = (MenuItem)trayMenu.Items[3]; 

            informationMenuItem.Click += (s, e) =>
            {
                MessageBox.Show("FuckAudio",
                    "정보", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            settingsMenuItem.Click += (s, e) =>
            {
                ShowWindow();
                SettingsTab_Click(null, null);
            };

            closeMenuItem.Click += (s, e) => System.Windows.Application.Current.Shutdown();

            AutoStartCheckBox.IsChecked = SetupHelper.IsStartupRegistered();
            AutoStartCheckBox.Checked += (s, e) => SetupHelper.RegisterStartup(true);
            AutoStartCheckBox.Unchecked += (s, e) => SetupHelper.RegisterStartup(false);

            LoadCurrentVolume();

            LoadAudioDevices();

            volumeCheckTimer.Start();

            PositionWindowToBottomRight();

          
            this.Hide();
        }

        private void VolumeCheckTimer_Tick(object sender, EventArgs e)
        {
          
            if (defaultDevice == null || isUpdatingSlider) return;

            try
            {
                float currentVolume = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                bool currentMuted = defaultDevice.AudioEndpointVolume.Mute;

         
                if (Math.Abs(VolumeSlider.Value - currentVolume) > 0.5)
                {
                    isUpdatingSlider = true;
                    VolumeSlider.Value = currentVolume;
                    VolumeText.Text = ((int)currentVolume).ToString();
                    isUpdatingSlider = false;
                }

                if (isMuted != currentMuted)
                {
                    isMuted = currentMuted;
                    UpdateVolumeIcon();
                }
            }
            catch
            {
                // 장치 접근 실패 시 무시
            }
        }

        private void LoadCurrentVolume()
        {
            if (defaultDevice != null)
            {
                try
                {
                    isUpdatingSlider = true;
                    float volume = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                    VolumeSlider.Value = volume;
                    VolumeText.Text = ((int)volume).ToString();
                    isMuted = defaultDevice.AudioEndpointVolume.Mute;
                    isUpdatingSlider = false;

                    UpdateVolumeIcon();
                }
                catch
                {
                    VolumeSlider.Value = 70;
                    VolumeText.Text = "70";
                }
            }
        }

        private void LoadAudioDevices()
        {
            if (deviceEnumerator == null) return;

            DeviceComboBox.Items.Clear();

            try
            {
                var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                foreach (var device in devices)
                {
                    var item = new ComboBoxItem
                    {
                        Content = device.FriendlyName,
                        Tag = device
                    };

                    DeviceComboBox.Items.Add(item);

                 
                    if (device.ID == defaultDevice.ID)
                    {
                        DeviceComboBox.SelectedItem = item;
                    }
                }
            }
            catch
            {
                // 장치 열거 실패 시 무시
            }
        }

        private void ShowWindow()
        {
            this.Show();
            this.Activate();
            PositionWindowToBottomRight();
            LoadCurrentVolume();
        }

        private void PositionWindowToBottomRight()
        {
            var workArea = SystemParameters.WorkArea;
            this.Left = workArea.Right - this.Width - 10;
            this.Top = workArea.Bottom - this.Height - 10;
        }

        private void VolumeTab_Click(object sender, RoutedEventArgs e)
        {
            VolumePanel.Visibility = Visibility.Visible;
            SettingsPanel.Visibility = Visibility.Collapsed;

            // 탭 버튼 스타일 변경
            VolumeTabButton.Opacity = 1.0;
            SettingsTabButton.Opacity = 0.6;
        }

        private void SettingsTab_Click(object sender, RoutedEventArgs e)
        {
            VolumePanel.Visibility = Visibility.Collapsed;
            SettingsPanel.Visibility = Visibility.Visible;

            // 탭 버튼 스타일 변경
            VolumeTabButton.Opacity = 0.6;
            SettingsTabButton.Opacity = 1.0;
        }

        private void VolumeSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeText == null || defaultDevice == null || isUpdatingSlider) return;

            try
            {
                int volumeValue = (int)e.NewValue;
                VolumeText.Text = volumeValue.ToString();

                // NAudio로 시스템 볼륨 설정
                defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = volumeValue / 100f;

                // 볼륨이 0보다 크면 음소거 해제
                if (volumeValue > 0 && isMuted)
                {
                    isMuted = false;
                    defaultDevice.AudioEndpointVolume.Mute = false;
                }

                UpdateVolumeIcon();

                // 알림 표시
                if (NotificationCheckBox?.IsChecked == true)
                {
                    TrayTaskbarIcon.ShowBalloonTip("볼륨 변경", $"볼륨: {volumeValue}%", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                }
            }
            catch
            {
                // 볼륨 설정 실패 시 무시
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (defaultDevice == null) return;

            try
            {
                if (!isMuted)
                {
                    // 음소거
                    previousVolume = VolumeSlider.Value;
                    isMuted = true;
                    defaultDevice.AudioEndpointVolume.Mute = true;

                    isUpdatingSlider = true;
                    VolumeSlider.Value = 0;
                    VolumeText.Text = "0";
                    isUpdatingSlider = false;
                }
                else
                {
                    // 음소거 해제
                    isMuted = false;
                    defaultDevice.AudioEndpointVolume.Mute = false;

                    isUpdatingSlider = true;
                    VolumeSlider.Value = previousVolume;
                    VolumeText.Text = ((int)previousVolume).ToString();
                    isUpdatingSlider = false;
                }

                UpdateVolumeIcon();

                // 알림 표시
                if (NotificationCheckBox?.IsChecked == true)
                {
                    TrayTaskbarIcon.ShowBalloonTip("음소거",
                        isMuted ? "음소거됨" : "음소거 해제됨",
                        Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                }
            }
            catch
            {
                // 음소거 실패 시 무시
            }
        }

        private void UpdateVolumeIcon()
        {
            if (VolumeIcon == null) return;

            double volume = isMuted ? 0 : VolumeSlider.Value;

   
            if (volume == 0 || isMuted)
            {
                VolumeIcon.Kind = PackIconKind.VolumeMute;
            }
            else if (volume < 33)
            {
                VolumeIcon.Kind = PackIconKind.VolumeLow;
            }
            else if (volume < 66)
            {
                VolumeIcon.Kind = PackIconKind.VolumeMedium;
            }
            else
            {
                VolumeIcon.Kind = PackIconKind.VolumeHigh;
            }

  
            UpdateTrayIcon();
        }

        private void UpdateTrayIcon()
        {
            try
            {
                double volume = isMuted ? 0 : VolumeSlider.Value;
                var newIcon = IconHelper.CreateVolumeIcon((int)volume, isMuted);
                TrayTaskbarIcon.Icon = newIcon;
            }
            catch
            {
                // 아이콘 업데이트 실패 시 무시
            }
        }

        private void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeviceComboBox.SelectedItem is ComboBoxItem item && item.Tag is MMDevice device)
            {
                try
                {
                    // 새 장치 설정
                    defaultDevice?.Dispose();
                    defaultDevice = device;

                    // 볼륨 다시 로드
                    LoadCurrentVolume();
                }
                catch
                {
                    // 장치 변경 실패 시 무시
                }
            }
        }

        private void ButtonTray_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 창을 닫지 않고 숨기기
            if (MinimizeToTrayCheckBox?.IsChecked == true)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
           
            volumeCheckTimer?.Stop();


            TrayTaskbarIcon?.Dispose();
            defaultDevice?.Dispose();
            deviceEnumerator?.Dispose();

            base.OnClosed(e);
        }
    }
}
