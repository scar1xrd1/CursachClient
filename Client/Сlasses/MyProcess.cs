using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Сlasses
{
    public class MyProcess
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User {  get; set; }
        public string? ProcessName { get; set; }
        public string? ProcessId { get; set; }
    }
}
