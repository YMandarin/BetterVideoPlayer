using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace BetterVideoPlayer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] allowedFileTypes = { ".mp4", ".mkv",".mov","wmv",".avi"};

        private bool maximized = false;
        private bool fullscreen = false;
        private bool fullscreenFromMaximized = false;

        private bool controlbarSliderClicked = false;
        private bool controlbarVolumeSliderClicked = false;

        
        private bool titleBarVisible = true;

        private DoubleAnimation titlebarFadeOutAnimation;
        private DoubleAnimation titlebarFadeInAnimation;
        
        
        private bool controlbarVisible = true;
        
        private DispatcherTimer controlbarTimer;
        private DoubleAnimation controlbarFadeOutAnimation;
        private DoubleAnimation controlbarFadeInAnimation;

        private double controlbar_slider_thumbWidth = 16;
        private double controlbar_slider_horizontalMargin = 5;
        private double controlbar_volume_slider_horizontalMargin = 5;

        private bool isVideoPlaying = false;
        private DispatcherTimer videoUpdater;
        private double videoLength;

        private bool isVideoPassed = false;
        private bool isPositionSetByMouseMove = false;

        public MainWindow()
        {
            InitializeComponent();
            controlbar_volume_slider.Value = 1;

            #region setupVideoPlayer
            string[] arguments = ((App)Application.Current).arguments;
            
            if (arguments.Length > 0)
            {
                if (allowedFileTypes.Contains(System.IO.Path.GetExtension(arguments[0]))){
                    if (!System.IO.Path.IsPathRooted(arguments[0]))
                    {
                        arguments[0] =  System.IO.Path.Combine(Directory.GetCurrentDirectory(), arguments[0]);
                    }
                    if (File.Exists(arguments[0]))
                    {
                        Uri uri = new Uri(arguments[0]);
                        isVideoPassed = true;
                        videoElement.Source = uri;
                        videoElement.Play();
                        IndexWindow.Title = System.IO.Path.GetFileNameWithoutExtension(uri.LocalPath);
                        titlebar_label.Content = IndexWindow.Title;
                    }
                    else
                    {
                        MessageBox.Show("File does not exist!", "File Error", MessageBoxButton.OK);
                    }
                }
                else
                {
                    MessageBox.Show("Wrong file type!", "File Error", MessageBoxButton.OK);
                }
            }
            #endregion

            #region setupAnimations

            titlebarFadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200), FillBehavior.Stop);
            titlebarFadeOutAnimation.Completed += (object sender, EventArgs e) => { titleBar.Opacity = 0; };
            titlebarFadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(100), FillBehavior.Stop);
            titlebarFadeInAnimation.Completed += (object sender, EventArgs e) => { titleBar.Opacity = 1; };

            controlbarFadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200), FillBehavior.Stop);
            controlbarFadeOutAnimation.Completed += (object sender, EventArgs e) => { ControlBar.Opacity = 0; Cursor = Cursors.None; };
            controlbarFadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200), FillBehavior.Stop);
            controlbarFadeInAnimation.Completed += (object sender, EventArgs e) => { ControlBar.Opacity = 1; };

            controlbarTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
            controlbarTimer.Tick += (object sender, EventArgs e)=>
            {
                if (controlbarVisible)
                {
                    controlbarVisible = false;
                    ControlBar.BeginAnimation(OpacityProperty, controlbarFadeOutAnimation);
                }
            };
            #endregion
        }

        #region WINDOW

        private void IndexWindow_MouseMove(object sender, MouseEventArgs e)
        {

            #region titlebarAnimation

            if (e.GetPosition(titleBar).Y < 32 && !titleBarVisible)
            {
                titleBar.BeginAnimation(OpacityProperty, titlebarFadeInAnimation);
                titleBarVisible = true;
            }
            else if (e.GetPosition(titleBar).Y > 50 && titleBarVisible)
            {
                titleBar.BeginAnimation(OpacityProperty, titlebarFadeOutAnimation);
                titleBarVisible = false;
            }

            #endregion

            #region controlbarAnimation

            if(Cursor == Cursors.None)
            {
                Cursor = Cursors.Arrow;
            }

            if (controlbarTimer.IsEnabled)
            {
                controlbarTimer.Stop();
            }
            
            if (!controlbarVisible)
            {
                controlbarVisible = true;
                ControlBar.BeginAnimation(OpacityProperty, controlbarFadeInAnimation);
            }

            if (e.GetPosition(ControlBar).Y < 0 && !controlbar_volume_popup.IsOpen && !controlbarSliderClicked && !titleBarVisible)
            {
                controlbarTimer.Start();
            }
                

            #endregion

            #region controlbarSliders

            if (controlbarSliderClicked)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    double sliderWidth = (controlbar_slider.ActualWidth - controlbar_slider_horizontalMargin*2);
                    double val = cap(0, (e.GetPosition(controlbar_slider).X - controlbar_slider_horizontalMargin) / sliderWidth, 1);
                    controlbar_slider.Value = cap(0, val * controlbar_slider_thumbWidth / (controlbar_slider.ActualWidth - controlbar_slider_horizontalMargin*2) - controlbar_slider_thumbWidth / (2 * sliderWidth) + val, 1);
                    controlbar_currentTime.Content = FormatTimeSpan(videoElement.Position);
                    isPositionSetByMouseMove = true;
                }
                else
                {
                    controlbarSliderClicked = false;
                }
            }

            #endregion

        }

        private void middle_previewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (controlbarVisible)
            {
                controlbarVisible = false;
                ControlBar.BeginAnimation(OpacityProperty, controlbarFadeOutAnimation);
            }
            else
            {
                Cursor = Cursors.Arrow;
                controlbarVisible = true;
                ControlBar.BeginAnimation(OpacityProperty, controlbarFadeInAnimation);
                controlbarTimer.Stop();
                controlbarTimer.Start();
            }
        }

        private void IndexWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (controlbarSliderClicked)
            {
                controlbarSliderClicked = false;
            }
        }

        private void IndexWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
            if (fullscreen && maximized)
            {
                
                fullscreen = false;
                fullscreenFromMaximized = false;
                WindowStyle = WindowStyle.SingleBorderWindow;

            }
            
            maximized = WindowState == WindowState.Maximized;
            
        }

        private void IndexWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            controlbarTimer.Start();
            if (titleBarVisible && (!fullscreen || !maximized))
            {
                titleBar.BeginAnimation(OpacityProperty, titlebarFadeOutAnimation);
                titleBarVisible = false;
            }
            
        }

        #endregion

        #region CONTROLBAR

        private void titlebar_close(object sender, RoutedEventArgs e) { Close(); }
        private void titlebar_reduce(object sender, RoutedEventArgs e) { WindowState = WindowState.Normal; }
        private void titlebar_maximize(object sender, RoutedEventArgs e) { WindowState = WindowState.Maximized; }
        private void titlebar_minimize(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }

        private void controlbar_openFullscreen(object sender, RoutedEventArgs e)
        {
            openFullscreen();
        }        

        private void controlbar_playVideo(object sender, RoutedEventArgs e)
        {
            PlayVideo();
        }

        private void controlbar_stopVideo(object sender, RoutedEventArgs e)
        {
            PauseVideo();
        }

        private void ControlBar_MouseLeave(object sender, MouseEventArgs e)
        {
            controlbarTimer.Start();
        }

        private void controlbar_closeFullscreen(object sender, RoutedEventArgs e)
        {
            closeFullscreen();
        }

        private void closeFullscreen()
        {

            fullscreen = false;
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Normal;
            if (fullscreenFromMaximized)
            {
                WindowState = WindowState.Maximized;
                fullscreenFromMaximized = false;
            }
        }

        private void openFullscreen()
        {
            fullscreenFromMaximized = WindowState == WindowState.Maximized;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Normal;
            WindowState = WindowState.Maximized;
            fullscreen = true;
        }
        #endregion

        #region SLIDERS

        private void controlbar_slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            
            controlbarSliderClicked = true;

            double sliderWidth = (controlbar_slider.ActualWidth - controlbar_slider_horizontalMargin * 2);
            double val = cap(0, (e.GetPosition(controlbar_slider).X - controlbar_slider_horizontalMargin) / sliderWidth, 1);
            controlbar_slider.Value = cap(0, val * controlbar_slider_thumbWidth / (controlbar_slider.ActualWidth - controlbar_slider_horizontalMargin * 2) - controlbar_slider_thumbWidth / (2 * sliderWidth) + val, 1);
            videoElement.Position = TimeSpan.FromSeconds(controlbar_slider.Value * videoLength);
            controlbar_currentTime.Content = FormatTimeSpan(videoElement.Position);
        }

        private double cap(double min, double value, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private void controlbar_volume_popup_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (controlbarVolumeSliderClicked)
            {
                if(e.LeftButton == MouseButtonState.Pressed)
            {
                    double sliderWidth = (controlbar_volume_slider.ActualWidth - controlbar_volume_slider_horizontalMargin * 2);
                    double val = cap(0, (e.GetPosition(controlbar_volume_slider).X - controlbar_volume_slider_horizontalMargin) / sliderWidth, 1);
                    controlbar_volume_slider.Value = cap(0, val * controlbar_slider_thumbWidth / (controlbar_volume_slider.ActualWidth - controlbar_volume_slider_horizontalMargin * 2) - controlbar_slider_thumbWidth / (2 * sliderWidth) + val, 1);
                    controlbar_volume_label.Content = Math.Round(controlbar_volume_slider.Value*100).ToString();
                    videoElement.Volume = controlbar_volume_slider.Value;
                }
                else
                {
                    controlbarVolumeSliderClicked = false;
                }
            }
            
        }

        private void controlbar_volume_slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            controlbarVolumeSliderClicked = true;
            double sliderWidth = (controlbar_volume_slider.ActualWidth - controlbar_volume_slider_horizontalMargin * 2);
            double val = cap(0, (e.GetPosition(controlbar_volume_slider).X - controlbar_volume_slider_horizontalMargin) / sliderWidth, 1);
            controlbar_volume_slider.Value = cap(0, val * controlbar_slider_thumbWidth / (controlbar_volume_slider.ActualWidth - controlbar_volume_slider_horizontalMargin * 2) - controlbar_slider_thumbWidth / (2 * sliderWidth) + val, 1);
            controlbar_volume_label.Content = Math.Round(controlbar_volume_slider.Value * 100).ToString();
            videoElement.Volume = controlbar_volume_slider.Value;
        }

        private void controlbar_volume_slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            controlbar_volume_slider.Value = controlbar_volume_slider.Value + (float) e.Delta / 2400;
            controlbar_volume_label.Content = Math.Round(controlbar_volume_slider.Value * 100).ToString();
            videoElement.Volume = controlbar_volume_slider.Value;
        }
        #endregion

        #region COMMANDS

        private void command_close(object sender, ExecutedRoutedEventArgs e)
        {
            if (fullscreen)
            {
                closeFullscreen();
            }
        }

        private void command_toggleFullscreen(object sender, ExecutedRoutedEventArgs e)
        {
            if (fullscreen)
            {
                closeFullscreen();
            }
            else
            {
                openFullscreen();

            }
        }

        private void command_play(object sender, ExecutedRoutedEventArgs e)
        {
            PlayVideo();
        }

        private void command_pause(object sender, ExecutedRoutedEventArgs e)
        {
            PauseVideo();
        }

        private void command_togglePlayPause(object sender, ExecutedRoutedEventArgs e)
        {
            if (isVideoPlaying)
            {
                PauseVideo();
            }
            else
            {
                PlayVideo();
            }
        }

        private void command_toggleMuteVolume(object sender, ExecutedRoutedEventArgs e)
        {
            videoElement.IsMuted = !videoElement.IsMuted;
        }

        private void command_increaseVolume(object sender, ExecutedRoutedEventArgs e)
        {
            if (videoElement.IsMuted)
                videoElement.IsMuted = false;
            controlbar_volume_slider.Value += 0.1;
            videoElement.Volume = controlbar_volume_slider.Value;
            controlbar_volume_label.Content = Math.Round(controlbar_volume_slider.Value * 100).ToString();
        }

        private void command_decreaseVolume(object sender, ExecutedRoutedEventArgs e)
        {
            if (videoElement.IsMuted)
                videoElement.IsMuted = false;
            controlbar_volume_slider.Value -= 0.1;
            videoElement.Volume = controlbar_volume_slider.Value;
            controlbar_volume_label.Content = Math.Round(controlbar_volume_slider.Value * 100).ToString();
        }

        private void command_fastForward(object sender, ExecutedRoutedEventArgs e)
        {
            videoElement.Position = TimeSpan.FromSeconds(Math.Min(videoElement.Position.TotalSeconds + 10,videoLength));
        }

        private void command_rewind(object sender, ExecutedRoutedEventArgs e)
        {
            videoElement.Position = TimeSpan.FromSeconds(Math.Max(videoElement.Position.TotalSeconds - 10, 0));
        }
        
        private void command_openFile(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Video Files |*" + string.Join(";*", allowedFileTypes);
            dialog.Title = "Open Video";
            if(dialog.ShowDialog() == true)
            {
                Uri uri = new Uri(dialog.FileName);
                videoElement.Source = uri;
                videoElement.Play();
                IndexWindow.Title = System.IO.Path.GetFileNameWithoutExtension(uri.LocalPath);
                titlebar_label.Content = IndexWindow.Title;
            }
        }

        private void command_toggleHover(object sender, ExecutedRoutedEventArgs e)
        {
            Topmost = !Topmost;
        }

        #endregion

        #region VIDEO_PLAYER

        private string FormatTimeSpan(TimeSpan timespan)
        {
            return timespan.ToString(timespan.Hours < 1? "mm':'ss" : "hh':'mm':'ss");
        }

        private void PlayVideo()
        {
            if (videoUpdater != null)
            {
                isVideoPlaying = true;
                videoElement.Play();
                videoUpdater.Start();
                controlbarButton_stopVideo.Visibility = Visibility.Visible;
                controlbarButton_playVideo.Visibility = Visibility.Hidden;
            }
        }
       
        private void PauseVideo()
        {
            if (videoUpdater != null)
            {
                isVideoPlaying = false;
                videoElement.Pause();
                videoUpdater.Stop();
                controlbarButton_stopVideo.Visibility = Visibility.Hidden;
                controlbarButton_playVideo.Visibility = Visibility.Visible;
            }
        }

        private void videoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (isVideoPassed)
            {
                isVideoPassed = false;
                IndexWindow.Height = videoElement.NaturalVideoHeight;
                IndexWindow.Width = videoElement.NaturalVideoWidth;
            }

            isVideoPlaying = true;
            videoElement.Play();
            controlbarButton_stopVideo.Visibility = Visibility.Visible;
            controlbarButton_playVideo.Visibility = Visibility.Hidden;

            videoLength = videoElement.NaturalDuration.TimeSpan.TotalSeconds;
            controlbar_totalTime.Content = FormatTimeSpan(videoElement.NaturalDuration.TimeSpan);
            controlbar_currentTime.Content = videoElement.Position.ToString("mm':'ss");

            videoUpdater = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            videoUpdater.Tick += (object se, EventArgs args) =>
            {
                if (isPositionSetByMouseMove)
                {
                    videoElement.Position = TimeSpan.FromSeconds(controlbar_slider.Value * videoLength);
                    isPositionSetByMouseMove = false;
                }
                controlbar_currentTime.Content = FormatTimeSpan(videoElement.Position);
                controlbar_slider.Value = videoElement.Position.TotalSeconds / videoLength;
            };
            videoUpdater.Start();

            videoElement.Volume = 3;
        }

        private void videoElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            videoUpdater.Stop();
        }

        private void controlbar_volume_mute(object sender, RoutedEventArgs e)
        {
            videoElement.IsMuted = true;
            controlbar_volume_mute_button.Visibility = Visibility.Hidden;
            controlbar_volume_unmute_button.Visibility = Visibility.Visible;

            controlbar_volume_button.Content = Application.Current.FindResource("volume_unmute");
        }

        private void controlbar_volume_unmute(object sender, RoutedEventArgs e)
        {
            videoElement.IsMuted = false;
            controlbar_volume_mute_button.Visibility = Visibility.Visible;
            controlbar_volume_unmute_button.Visibility = Visibility.Hidden;

            controlbar_volume_button.Content = Application.Current.FindResource("volume_mute");
        }

        private void middle_dragOver(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                foreach (string filename in filenames)
                {
                    if (!allowedFileTypes.Contains(System.IO.Path.GetExtension(filename)))
                    {
                        dropEnabled = false;
                        break;
                    }
                }
            }
            else
            {
                dropEnabled = false;
            }

            if (!dropEnabled)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void middle_dropFiles(object sender, DragEventArgs e)
        {
            string[] droppedFilenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            videoElement.Stop();
            Uri uri = new Uri(droppedFilenames[0]);
            videoElement.Source = uri;
            IndexWindow.Title = System.IO.Path.GetFileNameWithoutExtension(uri.LocalPath);
            titlebar_label.Content = IndexWindow.Title;
        }

        private void videoElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show(e.ErrorException.Message, "Error", MessageBoxButton.OK);
        }



        #endregion

    }
}
