// File:        MainWindow.xaml.cs
// Programmer:  Emily Goodwin
// Date:        2021 March 28
// Project:     Video Player w/ auto-scrolling subtitles
// Description: Contains a video player capable of playing a video, and displaying current position in the subtitles

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace VideoPlayer
{
    // Class:   MainWindow
    // Desc:    Main window using windows media player to power playing videos, with listbox displaying subtitles.
    public partial class MainWindow : Window
    {
        private TimeSpan VideoTime;

        SRT_Parser parser = new SRT_Parser();
        Dictionary<int,SubtitleItem> subtitles = new Dictionary<int,SubtitleItem>();
        DispatcherTimer sliderTimer = new DispatcherTimer();
        DispatcherTimer captionTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
        }

        // Method: File_Button_Click
        // Params: 
        //  object sender - who raised event
        //  RoutedEventArgs e - event args
        // Desc: Open the file dialog to choose a subtitle file
        private void File_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                subtitles = parser.ParseFile(openFileDialog.FileName);
                VideosListBox.ItemsSource = subtitles;
                SubtitleFile_TextBox.Text = openFileDialog.FileName;
            }
        }

        // Method: OpenVideo_MenuItem_Click
        // Params: 
        //  object sender - who raised event
        //  RoutedEventArgs e - event args
        // Desc: Open the file dialog for the video to display
        private void OpenVideo_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                VideoPlayer.Source = new System.Uri(fileName);
                PlayVideo();
            }
        }

        // Method: VideoPlayer_MediaOpened
        // Params: 
        //  object sender - who raised event
        //  RoutedEventArgs e - event args
        // Desc:    Called when media has been opened in the video player. 
        //          Setup the timers for syncing captions, and video progress bar.
        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {

            Video_Slider.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            VideoTime = VideoPlayer.NaturalDuration.TimeSpan;
            VideoPlayer.Volume = (double)Volume_Slider.Value / 100;


            sliderTimer = new DispatcherTimer();
            sliderTimer.Interval = TimeSpan.FromMilliseconds(400);
            sliderTimer.Tick += new EventHandler(Video_Tick);
            sliderTimer.Start();

            captionTimer = new DispatcherTimer();
            captionTimer.Interval = TimeSpan.FromMilliseconds(200);
            captionTimer.Tick += new EventHandler(Caption_Tick);
            captionTimer.Start();
        }

        // Method: Play_Btn_Click
        // Params: 
        //  object sender - who raised event
        //  RoutedEventArgs e - event args
        // Desc:    Handle play button being pressed.
        private void Play_Btn_Click(object sender, RoutedEventArgs e)
        {
            PlayVideo();
        }

        // Method: PlayVideo
        // Params:
        // Desc:    Play the video and start the progress bar
        private void PlayVideo()
        {
            if (VideoPlayer.Source != null)
            {
                VideoPlayer.Play();
                sliderTimer.Start();
            }
        }

        // Method: StopVideo
        // Params:
        // Desc:    Stop playing the video and stop the progress bar
        private void StopVideo()
        {
            if (VideoPlayer.CanPause)
            {
                VideoPlayer.Pause();
                sliderTimer.Stop();
            }
        }

        // Method: Pause_Btn_Click
        // Params: 
        //  object sender - who raised event
        //  RoutedEventArgs e - event args
        // Desc:    Handles pause button being pressed.
        private void Pause_Btn_Click(object sender, RoutedEventArgs e)
        {
            StopVideo();
        }

        // Method: Video_Slider_PreviewMouseUp
        // Params: 
        //  object sender - who raised event
        //  MouseButtonEventArgs e - event args
        // Desc:    Handle the video slider being manually changed, update the position in the video accordingly
        private void Video_Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            int SliderValue = (int)Video_Slider.Value;

            // Overloaded constructor takes the arguments days, hours, minutes, seconds, milliseconds.
            // Create a TimeSpan with miliseconds equal to the slider value.
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
            VideoPlayer.Position = ts;
        }

        // Method: Video_Tick
        // Params: 
        //  object sender - who raised event
        //  EventArgs e - event args
        // Desc:    Update video progress bar
        //https://stackoverflow.com/questions/10208959/binding-mediaelement-to-slider-position-in-wpf
        private void Video_Tick(object sender, EventArgs e)
        {
            Video_Slider.Value = VideoPlayer.Position.TotalMilliseconds;
        }

        // Method: Caption_Tick
        // Params: 
        //  object sender - who raised event
        //  EventArgs e - event args
        // Desc:    Update current caption
        private void Caption_Tick(object sender, EventArgs e)
        {
            if (VideoPlayer.HasVideo)
            {
                if (VideosListBox.Items.Count> 0)
                {
                    TimeSpan videoTime = VideoPlayer.Position;
                    if (subtitles.ContainsKey((int)videoTime.TotalSeconds))
                    {
                        VideosListBox.SelectedValue = new KeyValuePair<int, SubtitleItem>((int)videoTime.TotalSeconds, subtitles[(int)videoTime.TotalSeconds]);
                        VideosListBox.ScrollIntoView(VideosListBox.SelectedItem);
                    }
                }
            }
        }

        // Method: Volume_Slider_ValueChanged
        // Params: 
        //  object sender - who raised event
        //  RoutedPropertyChangedEventArgs<double>  e - event args
        // Desc:    update video volume
        private void Volume_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VideoPlayer.Volume = (double)Volume_Slider.Value/100;
        }

        // Method: VideosListBox_MouseDoubleClick
        // Params: 
        //  object sender - who raised event
        //  MouseButtonEventArgs  e - event args
        // Desc:    Update the video position if the user double clicks a time stamp
        private void VideosListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (VideosListBox.SelectedItem != null)
            {
                if (VideoPlayer.HasVideo)
                {
                    TimeSpan positionTime = ((KeyValuePair<int, SubtitleItem>)VideosListBox.SelectedItem).Value.GetBeginningTimeStamp();
                    if (positionTime < VideoPlayer.NaturalDuration.TimeSpan)
                    {
                        VideoPlayer.Position = positionTime;
                    }
                }
            }
        }

        // Method: SelectCurrentItem
        // Params: 
        //  object sender - who raised event
        //  KeyboardFocusChangedEventArgs  e - event args
        // Desc:    Select the current item in the list box
        //https://stackoverflow.com/questions/653524/selecting-a-textbox-item-in-a-listbox-does-not-change-the-selected-item-of-the-li
        private void SelectCurrentItem(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            item.IsSelected = true;
        }


    }
}
