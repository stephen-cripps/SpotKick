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


        private void LoginRun_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStatus.Foreground = Brushes.Black;
            ApplicationStatus.Text = "";
            try
            {
                if (spotifyCredentials?.AccessToken == null || DateTime.Now > spotifyCredentials.ExpiresOn)
                    SpotifyLogin();
                else
                    Run();
            }
            catch (Exception exception)
            {
                SetExceptionMessage(exception);
            }
        }

        private async void Run()
        {
            ApplicationStatus.Foreground = Brushes.Black;
            ApplicationStatus.Text = "Running...";

            //TODO: Check if access token has expired
            await playlistBuilder.Create(spotifyCredentials.AccessToken, SongKickUsername.Text);
            ApplicationStatus.Text = "Successfully Updated Playlist";
        }

        private async void SpotifyLogin()
        {
            ApplicationStatus.Text = "Logging In";
            spotifyCredentials = await spotifyAuthService.LogIn();
            ApplicationStatus.Text = "";
            LoginRun.Content = "Run"; //This is a bit weird, find a better way to do this
        }

        public void SetExceptionMessage(Exception ex)
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
