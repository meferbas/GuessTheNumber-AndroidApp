using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GameProjectAndroid
{
    public class GameClient
    {
        private TcpClient client;
        private NetworkStream stream;

        public GameClient(string serverIp, int port)
        {
            client = new TcpClient(serverIp, port);
            stream = client.GetStream();
        }

        public void SendData(string data)
        {
            byte[] dataBytes = Encoding.ASCII.GetBytes(data);
            stream.Write(dataBytes, 0, dataBytes.Length);
        }

        public string ReceiveData()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }

        public void Close()
        {
            stream.Close();
            client.Close();
        }
    }
}