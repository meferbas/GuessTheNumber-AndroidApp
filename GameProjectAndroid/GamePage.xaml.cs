using Microsoft.Maui.Controls;

namespace GameProjectAndroid
{
    [QueryProperty(nameof(Player1Name), "Player1Name")]
    [QueryProperty(nameof(Player2Name), "Player2Name")]
    public partial class GamePage : ContentPage
    {
        private string hiddenNumber; 
        private int attempts;
        private bool isPlayer1Turn = false;
        private int roundNumber = 1; // Hangi turda olduðumuzu takip eder
        private int maxRounds = 6; // Toplam tur sayýsý (3 tur her oyuncu için)
        private int player1Score = 0; // Oyuncu 1'in toplam deneme sayýsý
        private int player2Score = 0; // Oyuncu 2'nin toplam deneme sayýsý

        private string currentGuesser; 

        private string player1Name;
        public string Player1Name
        {
            get => player1Name;
            set
            {
                player1Name = value;
                OnPropertyChanged();
                UpdatePlayerNames();
            }
        }
        private string player2Name;
        public string Player2Name
        {
            get => player2Name;
            set
            {
                player2Name = value;
                currentGuesser = Player2Name;
                OnPropertyChanged();
                UpdatePlayerNames();
            }
        }


        public GamePage()
        {
            InitializeComponent();
        }
        private void UpdateTurnLabel()
        {
            TurnLabel.Text = $"Sayýyý tahmin edecek kiþi: {currentGuesser} ";
        }


        private void UpdatePlayerNames()
        {
            if (!string.IsNullOrEmpty(Player1Name) && !string.IsNullOrEmpty(Player2Name))
            {
                PlayerNamesLabel.Text = $"{Player1Name} vs {Player2Name}";
                UpdateTurnLabel();
            }
           
        }


        private void OnStartGameClicked(object sender, EventArgs e)
        {
            hiddenNumber = HiddenNumberEntry.Text;

            if (string.IsNullOrEmpty(hiddenNumber) || !hiddenNumber.All(char.IsDigit))
            {
                DisplayAlert("Hata", "Lütfen geçerli bir sayý girin. Sadece rakamlar kullanabilirsiniz.", "Tamam");
                return;
            }


            DigitCountLabel.Text = $"{hiddenNumber.Length} basamaklý bir sayý girildi.";
            HiddenNumberEntry.IsEnabled = false; // Gizli sayý giriþi kapatýlýr
            GuessEntry.IsEnabled = true; // Tahmin giriþi aktif hale getirilir
            ((Button)sender).IsEnabled = false; // "Oyuna Baþla" butonu kapatýlýr
            attempts = 0;

            Button guessButton = this.FindByName<Button>("GuessButton");
            guessButton.IsEnabled = true;

            UpdateTurnLabel();
        }

        private void OnGuessButtonClicked(object sender, EventArgs e)
        {
            string guess = GuessEntry.Text;
            attempts++;

            if (!guess.All(char.IsDigit))
            {
                DisplayAlert("Hata", "Lütfen sadece sayý tahmini yapýnýz!", "Tamam");
                return;
            }

            if (guess.Length != hiddenNumber.Length)
            {
                ResultLabel.Text = $"Girdiðiniz sayý {hiddenNumber.Length} basamaklý olmalý.";
                GuessEntry.Text = "";
                return;
            }

            int correctDigitsInPlace = 0;
            int correctDigitsWrongPlace = 0;

            // FormattedString oluþturuyoruz
            FormattedString feedbackFormatted = new FormattedString();

            for (int i = 0; i < hiddenNumber.Length; i++)
            {
                if (guess[i] == hiddenNumber[i])
                {
                    correctDigitsInPlace++;
                    feedbackFormatted.Spans.Add(new Span
                    {
                        Text = $"{guess[i]} sayýsý var ve doðru yerde.\n",
                        TextColor = Colors.Green // Doðru yerde olan sayý yeþil
                    });
                }
                else if (hiddenNumber.Contains(guess[i]))
                {
                    correctDigitsWrongPlace++;
                    feedbackFormatted.Spans.Add(new Span
                    {
                        Text = $"{guess[i]} sayýsý var ama yanlýþ yerde.\n",
                        TextColor = Colors.Gray // Yanlýþ yerde olan sayý gri
                    });
                }
                else
                {
                    feedbackFormatted.Spans.Add(new Span
                    {
                        Text = $"{guess[i]} sayýsý yok.\n",
                        TextColor = Colors.Red // Sayý yoksa kýrmýzý
                    });
                }
            }

            if (correctDigitsInPlace == hiddenNumber.Length)
            {
                ResultLabel.Text = $"Tebrikler! {attempts} denemede doðru tahmin ettiniz: {hiddenNumber}";
                GuessEntry.IsEnabled = false;
                GuessButton.IsEnabled = false;
                ContinueButton.IsVisible = true;

                // Skoru güncelle
                if (isPlayer1Turn)
                {
                    player1Score += attempts;
                }
                else
                {
                    player2Score += attempts;
                }

                // Eðer maksimum tur sayýsýna ulaþýldýysa kazananý belirle
                if (roundNumber >= maxRounds)
                {
                    DetermineWinner();
                }
                else
                {
                    roundNumber++;
                }
            }
            else
            {
                // Renkli geri bildirimi ResultLabel'a atayýn
                ResultLabel.FormattedText = feedbackFormatted;
            }

            GuessEntry.Text = ""; // Tahmin giriþini temizliyoruz
        }

        private void OnContinueButtonClicked(object sender, EventArgs e)
        {
            // Oyuncu sýrasýný deðiþtiriyoruz
            isPlayer1Turn = !isPlayer1Turn;
            attempts = 0;
            // Sýra deðiþtirildiðinde tahmin eden oyuncuyu deðiþtiriyoruz
            currentGuesser = isPlayer1Turn ? Player1Name : Player2Name;

            DigitCountLabel.Text = "";
            ContinueButton.IsVisible = false;
            ResultLabel.Text = "";
            HiddenNumberEntry.Text = "";
            HiddenNumberEntry.IsEnabled = true; // Gizli sayý giriþi tekrar aktif hale gelir
            startButton.IsEnabled = true; // Oyuna baþla butonu tekrar aktif olur
            GuessEntry.IsEnabled = false; // Tahmin giriþini kapatýyoruz
            GuessButton.IsEnabled = false; // Tahmin et butonunu kapatýyoruz
            UpdateTurnLabel(); // Oyuncu sýrasýný güncelliyoruz
        }

        private void DetermineWinner()
        {
            string winnerMessage;

            if (player1Score < player2Score)
            {
                winnerMessage = $"{Player1Name} kazandý! Toplam deneme: {player1Score}";
            }
            else if (player2Score < player1Score)
            {
                winnerMessage = $"{Player2Name} kazandý! Toplam deneme: {player2Score}";
            }
            else
            {
                winnerMessage = "Oyun berabere bitti!";
            }
            ResetGame();

            DisplayAlert("Oyun Bitti", winnerMessage, "Tamam");

        }

        private void ResetGame()
        {
            roundNumber = 1;
            player1Score = 0;
            player2Score = 0;
            isPlayer1Turn = true;
            currentGuesser = Player2Name;

            // Diðer gerekli sýfýrlama iþlemleri
            HiddenNumberEntry.Text = "";
            HiddenNumberEntry.IsEnabled = true;
            GuessEntry.IsEnabled = false;
            GuessButton.IsEnabled = false;
            ContinueButton.IsVisible = false;
            UpdateTurnLabel();
        }


    }
}