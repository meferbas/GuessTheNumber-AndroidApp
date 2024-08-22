namespace GameProjectAndroid;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        string player1Name = Player1NameEntry.Text;
        string player2Name = Player2NameEntry.Text;

        if (string.IsNullOrEmpty(player1Name) || string.IsNullOrEmpty(player2Name))
        {
            await DisplayAlert("Hata", "Lütfen her iki oyuncunun da ismini girin.", "Tamam");
            return;
        }

        try
        {
            await Shell.Current.GoToAsync($"//GamePage?Player1Name={player1Name}&Player2Name={player2Name}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

}

