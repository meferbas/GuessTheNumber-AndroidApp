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
        private int roundNumber = 1; // Hangi turda oldu�umuzu takip eder
        private int maxRounds = 6; // Toplam tur say�s� (3 tur her oyuncu i�in)
        private int player1Score = 0; // Oyuncu 1'in toplam deneme say�s�
        private int player2Score = 0; // Oyuncu 2'nin toplam deneme say�s�

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
            TurnLabel.Text = $"Say�y� tahmin edecek ki�i: {currentGuesser} ";
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
                DisplayAlert("Hata", "L�tfen ge�erli bir say� girin. Sadece rakamlar kullanabilirsiniz.", "Tamam");
                return;
            }


            DigitCountLabel.Text = $"{hiddenNumber.Length} basamakl� bir say� girildi.";
            HiddenNumberEntry.IsEnabled = false; // Gizli say� giri�i kapat�l�r
            GuessEntry.IsEnabled = true; // Tahmin giri�i aktif hale getirilir
            ((Button)sender).IsEnabled = false; // "Oyuna Ba�la" butonu kapat�l�r
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
                DisplayAlert("Hata", "L�tfen sadece say� tahmini yap�n�z!", "Tamam");
                return;
            }

            if (guess.Length != hiddenNumber.Length)
            {
                ResultLabel.Text = $"Girdi�iniz say� {hiddenNumber.Length} basamakl� olmal�.";
                GuessEntry.Text = "";
                return;
            }

            int correctDigitsInPlace = 0;
            int correctDigitsWrongPlace = 0;

            // FormattedString olu�turuyoruz
            FormattedString feedbackFormatted = new FormattedString();

            for (int i = 0; i < hiddenNumber.Length; i++)
            {
                if (guess[i] == hiddenNumber[i])
                {
                    correctDigitsInPlace++;
                    feedbackFormatted.Spans.Add(new Span
                    {
                        Text = $"{guess[i]} say�s� var ve do�ru yerde.\n",
                        TextColor = Colors.Green // Do�ru yerde olan say� ye�il
                    });
                }
                else if (hiddenNumber.Contains(guess[i]))
                {
                    correctDigitsWrongPlace++;
                    feedbackFormatted.Spans.Add(new Span
                    {
                        Text = $"{guess[i]} say�s� var ama yanl�� yerde.\n",
                        TextColor = Colors.Gray // Yanl�� yerde olan say� gri
                    });
                }
                else
                {
                    feedbackFormatted.Spans.Add(new Span
                    {
                        Text = $"{guess[i]} say�s� yok.\n",
                        TextColor = Colors.Red // Say� yoksa k�rm�z�
                    });
                }
            }

            if (correctDigitsInPlace == hiddenNumber.Length)
            {
                ResultLabel.Text = $"Tebrikler! {attempts} denemede do�ru tahmin ettiniz: {hiddenNumber}";
                GuessEntry.IsEnabled = false;
                GuessButton.IsEnabled = false;
                ContinueButton.IsVisible = true;

                // Skoru g�ncelle
                if (isPlayer1Turn)
                {
                    player1Score += attempts;
                }
                else
                {
                    player2Score += attempts;
                }

                // E�er maksimum tur say�s�na ula��ld�ysa kazanan� belirle
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
                // Renkli geri bildirimi ResultLabel'a atay�n
                ResultLabel.FormattedText = feedbackFormatted;
            }

            GuessEntry.Text = ""; // Tahmin giri�ini temizliyoruz
        }

        private void OnContinueButtonClicked(object sender, EventArgs e)
        {
            // Oyuncu s�ras�n� de�i�tiriyoruz
            isPlayer1Turn = !isPlayer1Turn;
            attempts = 0;
            // S�ra de�i�tirildi�inde tahmin eden oyuncuyu de�i�tiriyoruz
            currentGuesser = isPlayer1Turn ? Player1Name : Player2Name;

            DigitCountLabel.Text = "";
            ContinueButton.IsVisible = false;
            ResultLabel.Text = "";
            HiddenNumberEntry.Text = "";
            HiddenNumberEntry.IsEnabled = true; // Gizli say� giri�i tekrar aktif hale gelir
            startButton.IsEnabled = true; // Oyuna ba�la butonu tekrar aktif olur
            GuessEntry.IsEnabled = false; // Tahmin giri�ini kapat�yoruz
            GuessButton.IsEnabled = false; // Tahmin et butonunu kapat�yoruz
            UpdateTurnLabel(); // Oyuncu s�ras�n� g�ncelliyoruz
        }

        private void DetermineWinner()
        {
            string winnerMessage;

            if (player1Score < player2Score)
            {
                winnerMessage = $"{Player1Name} kazand�! Toplam deneme: {player1Score}";
            }
            else if (player2Score < player1Score)
            {
                winnerMessage = $"{Player2Name} kazand�! Toplam deneme: {player2Score}";
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

            // Di�er gerekli s�f�rlama i�lemleri
            HiddenNumberEntry.Text = "";
            HiddenNumberEntry.IsEnabled = true;
            GuessEntry.IsEnabled = false;
            GuessButton.IsEnabled = false;
            ContinueButton.IsVisible = false;
            UpdateTurnLabel();
        }


    }
}