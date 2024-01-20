using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientClass
    {
        UDP udp = new UDP();
        public string? Login { get; set; } = string.Empty;

        public async Task<string> SendServer(string message) => await udp.SendAsync(message);
    }
}
