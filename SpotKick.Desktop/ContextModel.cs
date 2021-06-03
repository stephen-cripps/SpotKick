using SpotKick.Desktop.Context;

namespace SpotKick.Desktop
{
    public class ContextModel : ObservableObject
    {
        bool showGreeting;
        string buttonText;
        string spotifyUsername;

        public bool ShowGreeting
        {
            get => showGreeting;
            set
            {
                showGreeting = value;
                OnPropertyChanged(nameof(showGreeting));
            }
        }

        public string SongKickUsername { get; set; }

        public string ButtonText
        {
            get => string.IsNullOrEmpty(buttonText) ? "" : buttonText;
            set
            {
                buttonText = value;
                OnPropertyChanged(nameof(buttonText));
            }
        }

        public string SpotifyUsername
        {
            get => string.IsNullOrEmpty(spotifyUsername) ? "" : spotifyUsername;
            set
            {
                spotifyUsername = value;
                OnPropertyChanged(nameof(spotifyUsername));
            }
        }
    }
}
