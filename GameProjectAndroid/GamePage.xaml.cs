using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace GameProjectAndroid
{
    
    [QueryProperty(nameof(PlayerName), "PlayerName")] // PlayerName ad�nda bir query parametresi al�r
    public partial class GamePage : ContentPage
    {
        private GameClient client;
        private string playerName;
        private string playerID;

        // PlayerName ad�nda bir property tan�mlan�r
        public string PlayerName
        {
            get => playerName;
            set
            {
                playerName = value; // PlayerName de�eri de�i�ti�inde yeni de�eri al�r
                OnPropertyChanged(); // De�i�iklik oldu�unda property'yi g�nceller
            }
        }

        public GamePage() // Constructor 
        {
            InitializeComponent();
            string serverIp = "13.61.12.95"; // Sunucu IP adresi
            int port = 8888;
            client = new GameClient(serverIp, port); // GameClient s�n�f�ndan bir nesne olu�turulur

            StartListening();
        }
        // Sunucudan gelen mesajlar� dinleyen metot
        private async void StartListening()
        {
            while (true)
            {
                // Sunucudan gelen mesaj� al
                string message = await Task.Run(() => client.ReceiveData());

                if (message.StartsWith("PlayerID:"))
                {
                    playerID = message.Substring(9);
                    TurnLabel.Text = $"Player ID'niz: {playerID} Di�er oyuncu bekleniliyor..";
                }
                else if (message.StartsWith("Gizli sayiyi belirleyin."))
                {
                    if (playerID == "1")
                    {
                        HiddenNumberEntry.IsEnabled = true;
                        startButton.IsEnabled = true;
                        GuessEntry.IsEnabled = false;
                        GuessButton.IsEnabled = false;
                        TurnLabel.Text = "Rakibinizin tahmin edece�i gizli say�y� belirleyin.";
                    }
                    else if (playerID == "2")
                    {
                        HiddenNumberEntry.IsEnabled = false;
                        startButton.IsEnabled = false;
                        GuessEntry.IsEnabled = true;
                        GuessButton.IsEnabled = false; // Gizli say� girilene kadar tahmin yapamayacak
                        TurnLabel.Text = "Di�er oyuncu gizli say�y� belirliyor. L�tfen bekleyin...";
                    }
                }

                else if (message.Contains("Gizli sayi "))
                {
                    DigitCountLabel.Text = message;
                }

                else if (message.StartsWith("Tahmin yapma sirasi sizde."))
                {
                    if (playerID == "2")
                    {
                        HiddenNumberEntry.IsEnabled = false;
                        GuessEntry.IsEnabled = true;
                        GuessButton.IsEnabled = true;
                        TurnLabel.Text = "Tahmin yapma s�ras� sizde.";
                    }
                    else
                    {
                        HiddenNumberEntry.IsEnabled = false;
                        GuessEntry.IsEnabled = false;
                        GuessButton.IsEnabled = false;
                        TurnLabel.Text = "Di�er oyuncu tahmin yap�yor. L�tfen bekleyin...";
                    }
                }
                else if (message.StartsWith("Yanlis tahmin"))
                {
                    GuessEntry.IsEnabled = true; // Yanl�� tahminden sonra tekrar tahmin yap�labilsin
                    GuessButton.IsEnabled = true;
                }
                else if (message.Contains("sayisi var ve dogru yerde") ||
                         message.Contains("sayisi var ama yanlis yerde") ||
                         message.Contains("sayisi yok"))
                {
                    ResultLabel.Text = message;
                }
                else if (message.StartsWith("Tebrikler"))
                {
                    await DisplayAlert("Sonu�", message, "Tamam");
                    ResetGame();
                }
                else if (message.StartsWith("Yanl�� tahmin"))
                {
                    ResultLabel.Text = message;
                }



            }
        }
        // Oyuna ba�la butonuna t�kland���nda �al��acak metot
        private void OnStartGameClicked(object sender, EventArgs e)
        {
            string hiddenNumber = HiddenNumberEntry.Text;

            // Gizli say� bo� veya rakam i�ermiyorsa hata mesaj� g�ster
            if (string.IsNullOrEmpty(hiddenNumber) || !hiddenNumber.All(char.IsDigit))
            {
                DisplayAlert("Hata", "L�tfen ge�erli bir say� girin. Sadece rakamlar kullanabilirsiniz.", "Tamam");
                return;
            }
            // Gizli say�n�n uzunlu�unu g�ster
            DigitCountLabel.Text = $"{hiddenNumber.Length} basamakl� bir say� girildi.";
            HiddenNumberEntry.IsEnabled = false; // Gizli say� giri�i kapat�l�r
            ((Button)sender).IsEnabled = false; // "Oyuna Ba�la" butonu kapat�l�r

            // Gizli say�y� ve uzunlu�unu sunucuya g�nder
            client.SendData($"SetHiddenNumber:{hiddenNumber}");
            
        }

        // Tahmin yap butonuna t�kland���nda �al��acak metot
        private void OnGuessButtonClicked(object sender, EventArgs e)
        {
            string guess = GuessEntry.Text;

            if (!guess.All(char.IsDigit))
            {
                DisplayAlert("Hata", "L�tfen sadece say� tahmini yap�n�z!", "Tamam");
                return;
            }

            // Sunucuya tahmin yap�lacak say�y� g�nder
            client.SendData($"Guess:{guess}");
        }

        // Oyunu s�f�rlayan metot
        private void ResetGame()
        {
            HiddenNumberEntry.Text = "";
            HiddenNumberEntry.IsEnabled = false;
            GuessEntry.IsEnabled = false;
            GuessButton.IsEnabled = false;
            TurnLabel.Text = "";
            ResultLabel.Text = "";
        }
    }
}
