using System;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
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
        DispatcherTimer dispatcherTimer;

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

            //Poll to check user validity
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(CheckUserToken);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(60);
            dispatcherTimer.Start();
        }

        async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //Initialise the user
            await InitialiseUser();
        }

        /// <summary>
        /// If user is invalid, try to refresh. If this fails, Update button prompt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void CheckUserToken(object sender, System.EventArgs e)
        {
            if (context.SpotifyCredentials.UserIsValid || await SpotifyRefresh())
                return;

            BindingOperations.GetBindingExpression(LoginRun, Button.ContentProperty).UpdateTarget();
        }

        private async Task InitialiseUser()
        {
            var previousUser = userRepo.GetPreviousUser();
            if (previousUser == null)
                return;

            context.SpotifyCredentials = previousUser.SpotifyCredentials;
            context.SongKickUsername = previousUser.SongKickUsername;

            if (!context.SpotifyCredentials.UserIsValid)
                await SpotifyRefresh();
        }

        private async void LoginRun_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStatus.Foreground = Brushes.Black;
            ApplicationStatus.Text = "";
            try
            {
                LoginRun.IsEnabled = false;
                if (context.SpotifyCredentials.UserIsValid)
                    await Run();
                else
                    await SpotifyLogin();
            }
            catch (Exception exception)
            {
                SetExceptionMessage(exception);
            }
            finally
            {
                LoginRun.IsEnabled = true;
            }
        }

        async Task Run()
        {
            ApplicationStatus.Foreground = Brushes.Black;
            ApplicationStatus.Text = "Running...";
            userRepo.StoreCurrentUser(context);
            await playlistBuilder.Create(context.SpotifyCredentials.AccessToken, context.SongKickUsername);
            ApplicationStatus.Text = "Successfully Updated Playlist";
        }

        async Task SpotifyLogin()
        {
            if (await SpotifyRefresh())
                return;
            ApplicationStatus.Text = "Logging In...";
            context.SpotifyCredentials = await spotifyAuthService.LogIn();
            ApplicationStatus.Text = "";
            userRepo.StoreCurrentUser(context);
            BindingOperations.GetBindingExpression(LoginRun, Button.ContentProperty).UpdateTarget();
        }

        /// <summary>
        /// Retrieves new access token from login token. Returns true if successful, false if not.
        /// </summary>
        /// <returns></returns>
        async Task<bool> SpotifyRefresh()
        {
            try
            {
                if (context.SpotifyCredentials.RefreshToken == null)
                    return false;
                
                ApplicationStatus.Text = "Verifying Spotify login..."; // why is this not writing>? 
                LoginRun.IsEnabled = false;
                context.SpotifyCredentials = await spotifyAuthService.RefreshAccessToken(context.SpotifyCredentials.RefreshToken);
                ApplicationStatus.Text = "";
                return true;
            }
            catch (Exception e)
            {
                ApplicationStatus.Text = "Spotify token expired, please log in again";
                return false;
            }
            finally
            {
                LoginRun.IsEnabled = true;
            }
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