namespace GameProjectAndroid;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnStartGameClicked(object sender, EventArgs e) // Oyuna başla butonuna tıklandığında çalışacak metot
    {
        string playerName = PlayerNameEntry.Text;

        if (string.IsNullOrEmpty(playerName)) // Oyuncu ismi boş ise hata mesajı göster
        {
            await DisplayAlert("Hata", "Lütfen isminizi girin.", "Tamam");
            return;
        }

        try
        {
            // Oyuncu ismi parametresi ile GamePage'e git
            await Shell.Current.GoToAsync($"//GamePage?PlayerName={playerName}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
