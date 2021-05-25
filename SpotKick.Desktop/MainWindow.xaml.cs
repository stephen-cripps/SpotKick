using System;
using System.Globalization;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SpotKick.Application;
using SpotKick.Application.Exceptions;
using SpotKick.Desktop.SpotifyAuth;
using SpotKick.Desktop.UserRepository;


namespace SpotKick.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly IPlaylistBuilder playlistBuilder;
        readonly ISpotifyAuthService spotifyAuthService;
        readonly IUserRepo userRepo;

        readonly UserData context = new UserData()
        {
            SpotifyCredentials = new SpotifyCredentials(),
            SongKickUsername = ""
        };

        public MainWindow(IPlaylistBuilder playlistBuilder, ISpotifyAuthService spotifyAuthService, IUserRepo userRepo)
        {
            this.playlistBuilder = playlistBuilder;
            this.spotifyAuthService = spotifyAuthService;
            this.userRepo = userRepo;
            InitializeComponent();
            DataContext = context;
            //InitialiseUser();
        }

        private void InitialiseUser()
        {
            this.DataContext = userRepo.GetPreviousUser() ?? new UserData()
            {
                SpotifyCredentials = new SpotifyCredentials()
            };

            // If user is invalid, try refresh
        }

        private void LoginRun_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStatus.Foreground = Brushes.Black;
            ApplicationStatus.Text = "";
            try
            {
                if (context.SpotifyCredentials.UserIsValid)
                    Run(); // Why is this not catching the exception? 
                else
                    SpotifyLogin();
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
            userRepo.StoreCurrentUser(context);
            await playlistBuilder.Create(context.SpotifyCredentials.AccessToken, context.SongKickUsername);
            ApplicationStatus.Text = "Successfully Updated Playlist";
        }

        private async void SpotifyLogin()
        {
            ApplicationStatus.Text = "Logging In";
            context.SpotifyCredentials = await spotifyAuthService.LogIn();
            ApplicationStatus.Text = "";
            userRepo.StoreCurrentUser(context);
            BindingOperations.GetBindingExpression(LoginRun, Button.ContentProperty).UpdateTarget();
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