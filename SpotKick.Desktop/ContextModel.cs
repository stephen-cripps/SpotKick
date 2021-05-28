using SpotKick.Desktop.Context;

namespace SpotKick.Desktop
{
    public class ContextModel : ObservableObject
    {
        string greeting;
        string buttonText;

        public string Greeting
        {
            get => string.IsNullOrEmpty(greeting) ? "" : greeting;
            set
            {
                greeting = value;
                OnPropertyChanged(nameof(greeting));
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
    }
}
