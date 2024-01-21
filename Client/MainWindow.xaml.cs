using Client.Images;
using Client.Сlasses;
using ControlzEx.Standard;
using MahApps.Metro.Controls;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        ViewModel viewModel = new ViewModel();
        ClientClass client = new ClientClass();

        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
            token = cts.Token;
            StartListen();

            Task.Run(PeriodicUpdateProcesses);
            Task.Run(StartKillProcesses);
        }

        async Task StartKillProcesses()
        {
            while (true)
            {
                if (client.Login != null)
                {
                    try
                    {
                        List<MyProcess> processes = new List<MyProcess>();
                        Application.Current.Dispatcher.Invoke(() => { processes = viewModel.ForbiddenProcesses.ToList(); });
                        foreach (var process in processes)
                        {
                            if(process.ProcessName != "Client")
                            {
                                var processToKill = Process.GetProcessesByName(process.ProcessName);
                                foreach (var kill in processToKill)
                                    kill.Kill();
                            }                            
                        }
                    }
                    catch { }
                }
            }
        }

        async void StartListen()
        {
            await Task.Run(StartListenAsync);
        }

        async Task StartListenAsync()
        {
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    string received = await client.udp.ReceiveAsync();
                    //MessageBox.Show(received);

                    if (received.Length > 6 && received[..6] == "unhide" && received[6..] == client.Login) Application.Current.Dispatcher.Invoke(Show);
                }
            }
            catch (OperationCanceledException) { }
        }

        async Task PeriodicUpdateProcesses()
        {
            while (true)
            {
                if(client.Login != string.Empty)
                {
                    await Task.Delay(1000);

                    var allProcesses = Process.GetProcesses().DistinctBy(p => p.ProcessName);
                    List<MyProcess> myProcesses = new List<MyProcess>();
                    foreach(var process in allProcesses) { myProcesses.Add(new MyProcess() { ProcessName = process.ProcessName, ProcessId = process.Id.ToString() }); }

                    //viewModel.Processes = new ObservableCollection<MyProcess>(myProcesses);

                    try
                    {
                        using (DatabaseContext db = new DatabaseContext())
                        {
                            var user = db.Users.FirstOrDefault(u => u.Login == client.Login);
                            user.AllProcesses = JsonSerializer.Serialize(myProcesses);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                viewModel.Processes = new ObservableCollection<MyProcess>(user.GetAllProcesses());
                                if (user.ForbiddenProcesses != null) viewModel.ForbiddenProcesses = new ObservableCollection<MyProcess>(user.GetForbiddenProcesses());
                            });

                            //if(user.AllProcesses == null) user.AllProcesses = new List<MyProcess>(myProcesses);
                            //if(db.Users.FirstOrDefault(u => u.Login == client.Login).AllProcesses != null)
                            //    db.Users.FirstOrDefault(u => u.Login == client.Login).AllProcesses.Clear();                            
                            //db.Users.FirstOrDefault(u => u.Login == client.Login).AllProcesses = myProcesses;
                            db.SaveChanges();
                        }

                        //await client.SendServer("updateusers");
                    }
                    catch { }
                }                
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            viewModel.isClosed = true;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            viewModel.LabelContent = viewModel.LabelContent == "Регистрация" ? "Вход" : "Регистрация";
            viewModel.ButtonContent = viewModel.ButtonContent == "Войти" ? "Зарегистрироваться" : "Войти";
            viewModel.PasswordAcceptVisibility = viewModel.PasswordAcceptVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            loginTextBox.Text = string.Empty;
            passwordAcceptTextBox.Password = string.Empty;
            passwordTextBox.Password = string.Empty;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string? content = (sender as Button).Content.ToString();

            if (content == "Войти")
            {
                if (loginTextBox.Text.Length >= 6 && loginTextBox.Text.Length <= 50)
                {
                    if (passwordTextBox.Password.Length >= 6 && passwordTextBox.Password.Length <= 50)
                    {
                        loginGrid.Opacity = 0.3;
                        ApplyBlurEffect(loginGrid, 15);
                        viewModel.IsLoading = true;

                        string login = loginTextBox.Text;
                        string password = passwordTextBox.Password;
                        bool result = await Task.Run(() => IsUserDataSuccess(login, password));

                        if (result)
                        {
                            loginGrid.Opacity = 1.0;
                            loginGrid.Effect = null;
                            viewModel.IsLoading = false;
                            loginTextBox.Text = string.Empty;
                            passwordTextBox.Password = string.Empty;
                            passwordAcceptTextBox.Password = string.Empty;
                            client.Login = login;

                            viewModel.SwitchFrame();
                        }
                        else
                        {
                            loginGrid.Opacity = 1.0;
                            loginGrid.Effect = null;
                            viewModel.IsLoading = false;

                            viewModel.ShowMessage("Неверный логин или пароль", MessageColor.Red);
                        }
                    }
                    else viewModel.ShowMessage("Минимум 6 символов", MessageColor.Red);
                }
                else viewModel.ShowMessage("Минимум 6 символов", MessageColor.Red);
            }
            else
            {
                if (loginTextBox.Text.Length >= 6 && !string.IsNullOrEmpty(loginTextBox.Text) && loginTextBox.Text.Length <= 50)
                {
                    if (passwordTextBox.Password.Length >= 6 && !string.IsNullOrEmpty(passwordTextBox.Password) && passwordTextBox.Password.Length <= 50)
                    {
                        if (passwordTextBox.Password == passwordAcceptTextBox.Password)
                        {
                            loginGrid.Opacity = 0.3;
                            ApplyBlurEffect(loginGrid, 15);
                            viewModel.IsLoading = true;

                            string login = loginTextBox.Text;
                            bool result = await Task.Run(() => IsLoginExist(login));
                            if (result)
                            {
                                viewModel.ShowMessage("Логин занят", MessageColor.Red);
                                loginGrid.Opacity = 1.0;
                                loginGrid.Effect = null;
                                viewModel.IsLoading = false;
                            }
                            else
                            {
                                string password = passwordTextBox.Password;
                                string salt = PasswordHasher.GenerateSalt();

                                try
                                {
                                    using (DatabaseContext db = new DatabaseContext())
                                    {
                                        db.Users.Add(new User(login, PasswordHasher.HashPassword(password, salt), salt));
                                        db.SaveChanges();
                                    }

                                    //await client.SendServer("updateusers");
                                }
                                catch (Exception ex) { MessageBox.Show(ex.Message); }

                                loginTextBox.Text = string.Empty;
                                passwordTextBox.Password = string.Empty;
                                passwordAcceptTextBox.Password = string.Empty;
                                viewModel.ShowMessage("Аккаунт создан", MessageColor.Green);
                                loginGrid.Opacity = 1.0;
                                loginGrid.Effect = null;
                                viewModel.IsLoading = false;
                                TextBlock_MouseLeftButtonUp(messageTextBlock, new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left));
                            }
                        }
                        else viewModel.ShowMessage("Пароли не совпадают", MessageColor.Red);
                    }
                    else viewModel.ShowMessage("Минимум 6 символов", MessageColor.Red);
                }
                else viewModel.ShowMessage("Минимум 6 символов", MessageColor.Red);
            }
        }

        async Task<bool> IsLoginExist(string login)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    return db.IsLoginExist(login);
                }
            }
            catch { }
            return false;
        }

        async Task<bool> IsUserDataSuccess(string login, string password)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    return db.IsUserDataSuccess(login, password);
                }
            }
            catch { }
            return false;
        }

        void ApplyBlurEffect(UIElement target, int radius)
        {
            if (target == null) return;
            BlurEffect blurEffect = new BlurEffect();
            blurEffect.Radius = radius;
            target.Effect = blurEffect;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SwitchFrame();
            client.Login = string.Empty;
        }

        private void Account_Click(object sender, RoutedEventArgs e)
        {
            viewModel.MainFrame = Visibility.Hidden;
            viewModel.AccountFrame = Visibility.Visible;
        }

        private void HideMode_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AccountFrame = Visibility.Hidden;
            viewModel.LoginFrame = Visibility.Visible;

            try
            {
                using (DatabaseContext db = new DatabaseContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Login == client.Login);
                    db.Users.Remove(user);
                    db.SaveChanges();
                }

                //await client.SendServer("updateusers");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ReturnToMenu_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AccountFrame = Visibility.Hidden;
            viewModel.MainFrame = Visibility.Visible;
            passwordNewTextBox.Password = string.Empty;
            loginNewTextBox.Text = string.Empty;
        }

        private async void LoginChange_Click(object sender, RoutedEventArgs e)
        {
            string login = loginNewTextBox.Text;

            if (loginNewTextBox.Text.Length >= 6 && !string.IsNullOrEmpty(loginNewTextBox.Text) && loginNewTextBox.Text.Length <= 50)
            {
                try
                {
                    using (DatabaseContext db = new DatabaseContext())
                    {
                        db.Users.FirstOrDefault(u => u.Login == client.Login).Login = login;
                        client.Login = login;
                        loginNewTextBox.Text = string.Empty;
                        db.SaveChanges();
                    }

                    //await client.SendServer("updateusers");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            else MessageBox.Show("Длина логина должна составлять от 6 до 50.");
        }

        private void PasswordChange_Click(object sender, RoutedEventArgs e)
        {
            string password = passwordNewTextBox.Password;
            string salt = PasswordHasher.GenerateSalt();

            if (passwordNewTextBox.Password.Length >= 6 && !string.IsNullOrEmpty(passwordNewTextBox.Password) && passwordNewTextBox.Password.Length <= 50)
            {
                try
                {
                    using (DatabaseContext db = new DatabaseContext())
                    {
                        db.Users.FirstOrDefault(u => u.Login == client.Login).Password = PasswordHasher.HashPassword(password, salt);
                        db.Users.FirstOrDefault(u => u.Login == client.Login).PasswordSalt = salt;
                        passwordNewTextBox.Password = string.Empty;
                        db.SaveChanges();
                    }

                    //await client.SendServer("updateusers");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            else MessageBox.Show("Длина пароля должна составлять от 6 до 50.");
        }

        private void ProcessItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem listBoxItem && listBoxItem.DataContext is MyProcess process)
            {
                try
                {
                    //viewModel.ForbiddenProcesses.Add(process);

                    if (process.ProcessName == "Client")
                    {
                        MessageBox.Show("Это шутка какая-то?");
                        return;
                    }

                    Task.Run(() =>
                    {
                        using (var db = new DatabaseContext())
                        {
                            var user = db.Users.FirstOrDefault(u => u.Login == client.Login);
                            if (user.ForbiddenProcesses != null)
                            {
                                var forbProcesses = user.GetForbiddenProcesses();
                                if (forbProcesses.FirstOrDefault(p => p.ProcessName == process.ProcessName) != null) return;
                                forbProcesses.Add(process);
                                user.ForbiddenProcesses = JsonSerializer.Serialize(forbProcesses);
                            }
                            else user.ForbiddenProcesses = JsonSerializer.Serialize(new List<MyProcess>() { process });
                            Application.Current.Dispatcher.Invoke(() => { viewModel.ForbiddenProcesses = new ObservableCollection<MyProcess>(user.GetForbiddenProcesses()); });
                            db.SaveChanges();
                        }
                    });


                    //using (DatabaseContext db = new DatabaseContext())
                    //{
                    //    var user = db.Users.FirstOrDefault(u => u.Login == client.Login);

                    //    if (user.ForbiddenProcesses != null && user.ForbiddenProcesses.Count > 0)
                    //        user.ForbiddenProcesses.Add(process);
                    //    else user.ForbiddenProcesses = viewModel.ForbiddenProcesses.ToList();

                    //    db.SaveChanges();
                    //}
                }
                catch { }
            }
        }

        private void ForbiddenProcessItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem listBoxItem && listBoxItem.DataContext is MyProcess process)
            {
                try
                {
                    //viewModel.ForbiddenProcesses.Remove(process);
                    Task.Run(() =>
                    {
                        using (var db = new DatabaseContext())
                        {
                            var user = db.Users.FirstOrDefault(u => u.Login == client.Login);
                            if (user.ForbiddenProcesses != null)
                            {
                                var forbProcesses = user.GetForbiddenProcesses();
                                var result = forbProcesses.FirstOrDefault(p => p.ProcessName == process.ProcessName);
                                forbProcesses.Remove(result);
                                user.ForbiddenProcesses = JsonSerializer.Serialize(forbProcesses);
                            }
                            Application.Current.Dispatcher.Invoke(() => { viewModel.ForbiddenProcesses = new ObservableCollection<MyProcess>(user.GetForbiddenProcesses()); });
                            db.SaveChanges();
                        }
                    });


                    //using (DatabaseContext db = new DatabaseContext())
                    //{
                    //    var user = db.Users.FirstOrDefault(u => u.Login == client.Login);

                    //    if (user.ForbiddenProcesses != null && user.ForbiddenProcesses.Count > 0)
                    //        user.ForbiddenProcesses.Remove(process);

                    //    db.SaveChanges();
                    //}
                }
                catch { }
            }
        }
    }
}