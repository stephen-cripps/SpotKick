using System;
using System.Collections.Generic;
using System.Linq;
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
using SpotKick.Application;
using SpotKick.Application.Exceptions;

namespace SpotKick.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly IPlaylistBuilder playlistBuilder;

        public MainWindow(IPlaylistBuilder playlistBuilder)
        {
            this.playlistBuilder = playlistBuilder;
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStatus.Text = "Running...";
            try
            {
                await playlistBuilder.Create("", SongKickUsername.Text);
                ApplicationStatus.Text = "Successfully Updated Playlist";
            }
            catch (Exception exception)
            {
                SetExceptionMessage(exception);
            }
        }


        public void SetExceptionMessage(Exception ex)
        {
            ApplicationStatus.Foreground = Brushes.Red;
            switch (ex)
            {
                case SongKickUserNotFoundException _:
                    ApplicationStatus.Text = "SongKick user not found";
                    break;
                default:
                    ApplicationStatus.Text = "An Unexpected Error Occurred";
                    break;
            }
        }
    }
}
