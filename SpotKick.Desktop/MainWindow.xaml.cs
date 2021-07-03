using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MediatR;
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
        readonly ISpotifyAuthService spotifyAuthService;
        readonly IUserRepo userRepo;
        readonly IMediator mediator;
        UserData user = new UserData();
        ContextModel context = new ContextModel();


        public MainWindow(ISpotifyAuthService spotifyAuthService, IUserRepo userRepo, IMediator mediator)
        {
            this.spotifyAuthService = spotifyAuthService;
            this.userRepo = userRepo;
            this.mediator = mediator;
            InitializeComponent();
            DataContext = context;

            //Check every 5 minutes the user is not about to expire
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += CheckUserToken;
            dispatcherTimer.Interval = TimeSpan.FromMinutes(5);
            dispatcherTimer.Start();
        }

        /// <summary>
        /// Code to run once window has loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await InitialiseUser();
            UpdateContext();
        }

        /// <summary>
        /// If user is invalid, try to refresh. If this fails, Update button prompt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void CheckUserToken(object sender, System.EventArgs e)
        {
            if (!user.SpotifyCredentials.UserIsValid)
                await SpotifyRefresh();
        }

        /// <summary>
        /// Checks stored user data. Performs token refresh if required.
        /// </summary>
        /// <returns></returns>
        private async Task InitialiseUser()
        {
            var previousUser = userRepo.GetPreviousUser();

            if (previousUser == null)
                return;

            user = previousUser;

            if (!user.SpotifyCredentials.UserIsValid)
                await SpotifyRefresh();
        }

        /// <summary>
        /// If user has a valid spotify token, runs the program. Else prompts user to log in. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LoginRun_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStatus.Foreground = Brushes.Black;
            ApplicationStatus.Text = "";
            try
            {
                LoginRun.IsEnabled = false;
                if (context.ButtonText == "Update Playlists")
                {
                    if (user.SpotifyCredentials.UserIsValid || await SpotifyRefresh())
                        await Run();
                }
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

        /// <summary>
        /// Resets context and clears stored data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ForgetMe_OnClick(object sender, RoutedEventArgs e)
        {
            context = new ContextModel();
            user = new UserData();
            DataContext = context;
            userRepo.ForgetUser();
            UpdateContext();
        }

        /// <summary>
        /// Runs the CreatePlaylist command 
        /// </summary>
        /// <returns></returns>
        async Task Run()
        {
            ApplicationStatus.Foreground = Brushes.Black;
            ApplicationStatus.Text = "Running...";

            user.SongKickUsername = context.SongKickUsername;
            userRepo.StoreCurrentUser(user);

            var command = new CreatePlaylist.Command(user.SpotifyCredentials.AccessToken, context.SongKickUsername);
            await mediator.Send(command);

            ApplicationStatus.Text = "Successfully Updated Playlist";
        }

        /// <summary>
        /// Runs Export to CSV
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ExportRun_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStatus.Foreground = Brushes.Black;
            ApplicationStatus.Text = "Running...";

            try
            {
                user.SongKickUsername = context.SongKickUsername;
                userRepo.StoreCurrentUser(user);
                var command = new CreateCSVExport.Command(context.FolderPath + "/" + context.FileName, context.SongKickUsername);
                await mediator.Send(command);
                ApplicationStatus.Text = "File generated";
            }
            catch (Exception exception)
            {
                SetExceptionMessage(exception);
            }

   
        }

        /// <summary>
        /// Launches the browser for a user to auth, returns Access and Refresh Tokens
        /// </summary>
        /// <returns></returns>
        async Task SpotifyLogin()
        {
            if (await SpotifyRefresh())
                return;

            ApplicationStatus.Text = "Logging In...";

            user.SpotifyCredentials = await spotifyAuthService.LogIn();
            await GetUsername();
            userRepo.StoreCurrentUser(user);
            UpdateContext();

            ApplicationStatus.Text = "";
            Activate();
        }

        /// <summary>
        /// Retrieves new access token from login token. Returns true if successful, false if not.
        /// </summary>
        /// <returns></returns>
        async Task<bool> SpotifyRefresh()
        {
            try
            {
                if (user.SpotifyCredentials.RefreshToken == null)
                    return false;

                ApplicationStatus.Text = "Verifying Spotify login...";
                user.SpotifyCredentials = await spotifyAuthService.RefreshAccessToken(user.SpotifyCredentials.RefreshToken);
                ApplicationStatus.Text = "";

                if (user.SpotifyUsername == null)
                    await GetUsername();

                userRepo.StoreCurrentUser(user);
                return true;
            }
            catch (Exception)
            {
                ApplicationStatus.Text = "Spotify token expired, please log in again";
                return false;
            }
            finally
            {
                UpdateContext();
            }
        }

        /// <summary>
        /// Calls the spotify API to find a user's username
        /// </summary>
        async Task GetUsername()
        {
            var query = new ReadSpotifyUsername.Query(user.SpotifyCredentials.AccessToken);
            user.SpotifyUsername = await mediator.Send(query);
            UpdateContext();
        }


        /// <summary>
        /// Converts exception type into error message
        /// </summary>
        /// <param name="ex"></param>
        void SetExceptionMessage(Exception ex)
        {
            ApplicationStatus.Foreground = Brushes.Red;
            ApplicationStatus.Text = ex switch
            {
                SpotifyAuthException _ => "An error occurred authenticating the spotify account",
                SongKickUserNotFoundException _ => "SongKick user not found",
                CsvWriteException => "Error Saving CSV. Check your folder path is valid",
                _ => "An Unexpected Error Occurred"
            };
        }

        /// <summary>
        /// Refresh bound values in the UI
        /// </summary>
        void UpdateContext()
        {
            if (context.SongKickUsername == null)
                context.SongKickUsername = user.SongKickUsername;

            context.ButtonText = user.SpotifyCredentials.UserIsValid ? "Update Playlists" : "Spotify Login";
            context.ShowGreeting = user.SpotifyUsername != null && user.SpotifyCredentials.UserIsValid;
            context.SpotifyUsername = user.SpotifyUsername;
        }
    }
}