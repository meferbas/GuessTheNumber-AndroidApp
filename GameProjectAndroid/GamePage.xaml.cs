using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace GameProjectAndroid
{
    
    [QueryProperty(nameof(PlayerName), "PlayerName")] // PlayerName adýnda bir query parametresi alýr
    public partial class GamePage : ContentPage
    {
        private GameClient client;
        private string playerName;
        private string playerID;

        // PlayerName adýnda bir property tanýmlanýr
        public string PlayerName
        {
            get => playerName;
            set
            {
                playerName = value; // PlayerName deðeri deðiþtiðinde yeni deðeri alýr
                OnPropertyChanged(); // Deðiþiklik olduðunda property'yi günceller
            }
        }

        public GamePage() // Constructor 
        {
            InitializeComponent();
            string serverIp = "13.61.12.95"; // Sunucu IP adresi
            int port = 8888;
            client = new GameClient(serverIp, port); // GameClient sýnýfýndan bir nesne oluþturulur

            StartListening();
        }
        // Sunucudan gelen mesajlarý dinleyen metot
        private async void StartListening()
        {
            while (true)
            {
                // Sunucudan gelen mesajý al
                string message = await Task.Run(() => client.ReceiveData());

                if (message.StartsWith("PlayerID:"))
                {
                    playerID = message.Substring(9);
                    TurnLabel.Text = $"Player ID'niz: {playerID} Diðer oyuncu bekleniliyor..";
                }
                else if (message.StartsWith("Gizli sayiyi belirleyin."))
                {
                    if (playerID == "1")
                    {
                        HiddenNumberEntry.IsEnabled = true;
                        startButton.IsEnabled = true;
                        GuessEntry.IsEnabled = false;
                        GuessButton.IsEnabled = false;
                        TurnLabel.Text = "Rakibinizin tahmin edeceði gizli sayýyý belirleyin.";
                    }
                    else if (playerID == "2")
                    {
                        HiddenNumberEntry.IsEnabled = false;
                        startButton.IsEnabled = false;
                        GuessEntry.IsEnabled = true;
                        GuessButton.IsEnabled = false; // Gizli sayý girilene kadar tahmin yapamayacak
                        TurnLabel.Text = "Diðer oyuncu gizli sayýyý belirliyor. Lütfen bekleyin...";
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
                        TurnLabel.Text = "Tahmin yapma sýrasý sizde.";
                    }
                    else
                    {
                        HiddenNumberEntry.IsEnabled = false;
                        GuessEntry.IsEnabled = false;
                        GuessButton.IsEnabled = false;
                        TurnLabel.Text = "Diðer oyuncu tahmin yapýyor. Lütfen bekleyin...";
                    }
                }
                else if (message.StartsWith("Yanlis tahmin"))
                {
                    GuessEntry.IsEnabled = true; // Yanlýþ tahminden sonra tekrar tahmin yapýlabilsin
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
                    await DisplayAlert("Sonuç", message, "Tamam");
                    ResetGame();
                }
                else if (message.StartsWith("Yanlýþ tahmin"))
                {
                    ResultLabel.Text = message;
                }



            }
        }
        // Oyuna baþla butonuna týklandýðýnda çalýþacak metot
        private void OnStartGameClicked(object sender, EventArgs e)
        {
            string hiddenNumber = HiddenNumberEntry.Text;

            // Gizli sayý boþ veya rakam içermiyorsa hata mesajý göster
            if (string.IsNullOrEmpty(hiddenNumber) || !hiddenNumber.All(char.IsDigit))
            {
                DisplayAlert("Hata", "Lütfen geçerli bir sayý girin. Sadece rakamlar kullanabilirsiniz.", "Tamam");
                return;
            }
            // Gizli sayýnýn uzunluðunu göster
            DigitCountLabel.Text = $"{hiddenNumber.Length} basamaklý bir sayý girildi.";
            HiddenNumberEntry.IsEnabled = false; // Gizli sayý giriþi kapatýlýr
            ((Button)sender).IsEnabled = false; // "Oyuna Baþla" butonu kapatýlýr

            // Gizli sayýyý ve uzunluðunu sunucuya gönder
            client.SendData($"SetHiddenNumber:{hiddenNumber}");
            
        }

        // Tahmin yap butonuna týklandýðýnda çalýþacak metot
        private void OnGuessButtonClicked(object sender, EventArgs e)
        {
            string guess = GuessEntry.Text;

            if (!guess.All(char.IsDigit))
            {
                DisplayAlert("Hata", "Lütfen sadece sayý tahmini yapýnýz!", "Tamam");
                return;
            }

            // Sunucuya tahmin yapýlacak sayýyý gönder
            client.SendData($"Guess:{guess}");
        }

        // Oyunu sýfýrlayan metot
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
