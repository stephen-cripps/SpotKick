using System;
using System.Windows;
using System.Windows.Media;
using SpotKick.Application;
using SpotKick.Application.Exceptions;
using SpotKick.Desktop.SpotifyAuth;


namespace SpotKick.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly IPlaylistBuilder playlistBuilder;
        readonly ISpotifyAuthService spotifyAuthService;
        SpotifyCredentials spotifyCredentials;

        public MainWindow(IPlaylistBuilder playlistBuilder, ISpotifyAuthService spotifyAuthService)
        {
            this.playlistBuilder = playlistBuilder;
            this.spotifyAuthService = spotifyAuthService;
            InitializeComponent();
        }

        private async void Run_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStatus.Foreground = Brushes.Black;
            ApplicationStatus.Text = "Running...";
            try
            {
                //TODO: Check if access token has expired
                await playlistBuilder.Create(spotifyCredentials.AccessToken, SongKickUsername.Text);
                ApplicationStatus.Text = "Successfully Updated Playlist";
            }
            catch (Exception exception)
            {
                SetRunExceptionMessage(exception);
            }
        }

        private async void SpotifyLogin_Click(object sender, RoutedEventArgs e)
        {
            SpotifyError.Text = "";
            try
            {
                spotifyCredentials = await spotifyAuthService.LogIn();
            }
            catch (Exception)
            {
                SpotifyError.Text = "An Unexpected Error Occurred";
            }
        }

        public void SetRunExceptionMessage(Exception ex)
        {
            ApplicationStatus.Foreground = Brushes.Red;
            ApplicationStatus.Text = ex switch
            {
                SpotifyAuthException _ => "An error occurred authenticating the spotify account",
                SongKickUserNotFoundException _ => "SongKick user not found",
                _ => "An Unexpected Error Occurred"
            };
        }
    }
}
