using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MediatR;
using SpotKick.Application;
using SpotKick.Application.Exceptions;
using SpotKick.Application.Services;
using SpotKick.Application.SpotifyAuth;
using SpotKick.Application.UserRepository;


namespace SpotKick.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ISpotifyAuthService spotifyAuthService;
        private readonly IUserRepo userRepo;
        private readonly IMediator mediator;
        private readonly ISpotifyService spotifyService;
        private UserData user = new UserData();
        private ContextModel context = new ContextModel();


        public MainWindow(ISpotifyAuthService spotifyAuthService, IUserRepo userRepo, IMediator mediator,
            ISpotifyService spotifyService)
        {
            this.spotifyAuthService = spotifyAuthService;
            this.userRepo = userRepo;
            this.mediator = mediator;
            this.spotifyService = spotifyService;
            InitializeComponent();
            DataContext = context;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            user = userRepo.GetPreviousUser();
            UpdateContext();
        }

        private async void Run_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStatus.Foreground = Brushes.Black;
            user.SongKickUsername = context.SongKickUsername;
            userRepo.StoreCurrentUser(user);
            try
            {
                if (user.SpotifyCredentials == null)
                {
                    ApplicationStatus.Text = "logging In...";
                    user.SpotifyCredentials = await spotifyAuthService.GetCredentials();
                    UpdateContext();
                    ApplicationStatus.Text = "";
                }
                else
                {
                    ApplicationStatus.Text = "Updating Playlists...";
                    var command = new CreatePlaylist.Command(context.SongKickUsername);
                    await mediator.Send(command);
                    ApplicationStatus.Text = "";
                }
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
        
        private async void ExportRun_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStatus.Foreground = Brushes.Black;
            ApplicationStatus.Text = "Running...";

            try
            {
                user.SongKickUsername = context.SongKickUsername;
                userRepo.StoreCurrentUser(user);
                var command = new CreateCSVExport.Command(context.FolderPath + "/" + context.FileName,
                    context.SongKickUsername);
                await mediator.Send(command);
                ApplicationStatus.Text = "File generated";
            }
            catch (Exception exception)
            {
                SetExceptionMessage(exception);
            }
        }

        private void SetExceptionMessage(Exception ex)
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
        
        private void ForgetMe_OnClick(object sender, RoutedEventArgs e)
        {
            context = new ContextModel();
            user = new UserData();
            DataContext = context;
            userRepo.ForgetUser();
            UpdateContext();
        }
        private void UpdateContext()
        {
            context.SongKickUsername ??= user.SongKickUsername;

            context.ButtonText = user.SpotifyCredentials.UserIsValid ? "Update Playlists" : "Spotify Login";
            context.ShowGreeting = user.SpotifyUsername != null && user.SpotifyCredentials.UserIsValid;
            context.SpotifyUsername = user.SpotifyUsername;
        }
    }
}