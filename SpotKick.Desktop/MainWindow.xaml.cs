using System;
using System.Windows;
using System.Windows.Media;
using MediatR;
using SpotKick.Application;
using SpotKick.Application.Exceptions;
using SpotKick.Application.Services;
using SpotKick.Application.UserRepository;


namespace SpotKick.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly IMediator mediator;
        private readonly ISpotifyService spotifyService;
        private readonly ICurrentUserService currentUserService;
        private UserData user;
        private ContextModel context = new ();


        public MainWindow(IMediator mediator, ISpotifyService spotifyService, ICurrentUserService currentUserService)
        {
            this.mediator = mediator;
            this.spotifyService = spotifyService;
            this.currentUserService = currentUserService;
            InitializeComponent();
            DataContext = context;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            user = currentUserService.GetCurrentUser();
            UpdateContext();
        }

        private async void Run_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStatus.Foreground = Brushes.Black;
            user.SongKickUsername = context.SongKickUsername;
            try
            {
                if (user.SpotifyCredentials.AccessToken == null)
                {
                    ApplicationStatus.Text = "logging In...";
                    user = await currentUserService.ValidateAndGetCurrentUserAsync();
                    UpdateContext();
                    ApplicationStatus.Text = "";
                }
                else
                {
                    ApplicationStatus.Text = "Updating Playlists...";
                    currentUserService.StoreSongkickUsername(context.SongKickUsername);
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
                currentUserService.StoreSongkickUsername(context.SongKickUsername);
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
            currentUserService.ForgetCurrentUser();
            UpdateContext();
        }
        private void UpdateContext()
        {
            context.ButtonText = user.SpotifyCredentials.AccessToken != null ? "Update Playlists" : "Spotify Login";
            context.ShowGreeting = user.SpotifyCredentials.AccessToken != null;

            context.SongKickUsername ??= user.SongKickUsername;
            context.SpotifyUsername = user.SpotifyUser?.Username;
        }
    }
}