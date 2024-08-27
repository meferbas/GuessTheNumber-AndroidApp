using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static TcpListener server = new TcpListener(IPAddress.Any, 8888);
    static TcpClient player1;
    static TcpClient player2;
    static bool isPlayer1Turn = true;
    static string hiddenNumber;

    static void Main(string[] args)
    {
        server.Start();
        Console.WriteLine("Sunucu başlatıldı. Bağlantı bekleniyor...");

        // Player 1'i kabul et
        player1 = server.AcceptTcpClient();
        SendMessage(player1, "PlayerID:1");

        // Player 2'yi kabul et
        player2 = server.AcceptTcpClient();
        SendMessage(player2, "PlayerID:2");
        SendMessage(player1, "Gizli sayiyi belirleyin.");
        SendMessage(player2, "Gizli sayiyi belirleyin.");


        // Player 1'in gizli sayıyı belirlemesini bekle
        hiddenNumber = ReceiveMessage(player1).Substring(16).Trim();
        Console.WriteLine($"Gizli Sayı: {hiddenNumber}");

        SendMessage(player1, $"Gizli sayi {hiddenNumber.Length} basamakli olarak belirlendi.");
        SendMessage(player2, $"Gizli sayi {hiddenNumber.Length} basamakli olarak belirlendi.");


        // Sıra Player 2'ye tahmin yapma sırası
        SendMessage(player2, "Tahmin yapma sirasi sizde.");
        SendMessage(player1, "Player 2 tahmin yapiyor. Lutfen bekleyin...");

        // Oyun döngüsü
        while (true)
        {
            if (isPlayer1Turn)
            {
                ProcessTurn(player2, player1, "Tahmin yapma sirasi sizde.");
            }
            else
            {
                ProcessTurn(player1, player2, "Tahmin yapma sirasi sizde.");
            }
        }
    }

    static void ProcessTurn(TcpClient currentPlayer, TcpClient waitingPlayer, string nextPrompt)
    {
        string message = ReceiveMessage(currentPlayer);


            if (message.StartsWith("Guess:"))
            {
                string guess = message.Substring(6);
                Console.WriteLine($"Gelen Tahmin: {guess}");

                if (guess == hiddenNumber)
                {
                    SendMessage(currentPlayer, "Tebrikler, dogru tahmin!");
                    SendMessage(waitingPlayer, "Maalesef, kaybettiniz.");
                    isPlayer1Turn = !isPlayer1Turn;
                    CleanupAndExit();
                }
                else
                {
                    string feedback = EvaluateGuess(guess, hiddenNumber);
                    SendMessage(currentPlayer, feedback); // Geri bildirimi tahmin yapan oyuncuya gönder
                    SendMessage(waitingPlayer, $"Rakibinizin tahmini: {guess}. {nextPrompt}");
                    SendMessage(currentPlayer, "Yanlis tahmin, tekrar deneyin.");

            }
        }
        

    }

    static string EvaluateGuess(string guess, string hiddenNumber)
    {
        if (guess.Length != hiddenNumber.Length)
        {
            return $"Tahmin edilen sayi {hiddenNumber.Length} basamakli olmali.";
        }

        int correctDigitsInPlace = 0;
        int correctDigitsWrongPlace = 0;
        StringBuilder feedback = new StringBuilder();

        for (int i = 0; i < hiddenNumber.Length; i++)
        {
            if (guess[i] == hiddenNumber[i])
            {
                correctDigitsInPlace++;
                feedback.AppendLine($"{guess[i]} sayisi var ve dogru yerde.");
            }
            else if (hiddenNumber.Contains(guess[i]))
            {
                correctDigitsWrongPlace++;
                feedback.AppendLine($"{guess[i]} sayisi var ama yanlis yerde.");
            }
            else
            {
                feedback.AppendLine($"{guess[i]} sayisi yok.");
            }
        }

        return feedback.ToString();
    }

    static void SendMessage(TcpClient client, string message)
    {
        NetworkStream stream = client.GetStream();
        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    static string ReceiveMessage(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
    }

    static void CleanupAndExit()
    {
        player1.Close();
        player2.Close();
        server.Stop();
        Console.WriteLine("Oyun sona erdi. Sunucu kapatıldı.");
        Environment.Exit(0);
    }
}
