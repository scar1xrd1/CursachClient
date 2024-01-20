using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class UDP
    {
        public string Ip { get; set; }
        public string ServerPort { get; set; }
        public string LocalPort { get; set; }
        private UdpClient udp;
        private IPEndPoint? serverEP;

        public UDP()
        {
            Ip = "127.0.0.1";
            LocalPort = "55961";
            ServerPort = "55960";

            udp = new UdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55961));
        }

        public UDP(string ip, string serverport, string localport)
        {
            Ip = ip;
            LocalPort = localport;
            ServerPort = serverport;
            udp = new UdpClient(new IPEndPoint(IPAddress.Parse(Ip), int.Parse(LocalPort)));
        }

        public void SetPoint(string ip, string serverport, string localport)
        {
            Ip = ip;
            ServerPort = serverport;
            LocalPort = localport;
            udp = new UdpClient(new IPEndPoint(IPAddress.Parse(Ip), int.Parse(LocalPort)));
        }

        public async Task<string> SendAsync(string data)
        {
            string feedback = "Данные успешно отправлены.";
            try
            {
                serverEP = new IPEndPoint(IPAddress.Parse(Ip), int.Parse(ServerPort));
                await udp.SendAsync(Encoding.UTF8.GetBytes(data), serverEP);
            }
            catch (Exception e) { feedback = $"Ошибка отправки данных: {e.Message}"; }
            return feedback;
        }

        public async Task<string> ReceiveAsync()
        {
            string feedback = "";
            try
            {
                var result = await udp.ReceiveAsync();
                feedback = Encoding.UTF8.GetString(result.Buffer);
            }
            catch (Exception e) { feedback = $"Ошибка получения данных: {e.Message}"; }
            return feedback;
        }
    }
}
