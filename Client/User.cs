using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string PasswordSalt {  get; set; }
        public string? ForbiddenProcesses { get; set; }
        public string? AllProcesses { get; set; }

        public User() { }
        public User(string login, string password, string passwordSalt)
        {
            Login = login;
            Password = password;
            PasswordSalt = passwordSalt;
        }

        public List<Process>? GetForbiddenProcesses()
        {
            try { return JsonSerializer.Deserialize<List<Process>>(ForbiddenProcesses); }
            catch { return null; }
        }

        public List<Process>? GetAllProcesses()
        {
            try { return JsonSerializer.Deserialize<List<Process>>(AllProcesses); }
            catch { return null; }
        }
    }
}
