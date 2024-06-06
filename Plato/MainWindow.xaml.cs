using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace Plato
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HubConnection _connection;
        private string? _token;

        public MainWindow()
        {
            InitializeComponent();
            _token = null;

            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/chat", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_token);
                })
                .Build();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{user}: {message}";
                    messagesList.Items.Add(newMessage);
                });
            });

            try
            {
                var loginRequest = new LoginRequest()
                {
                    UserName = userTextBox.Text,
                    Password = passwordBox.Password
                };

                _token = await GetAuthenticationToken(loginRequest);

                if (_token != null)
                {
                    await _connection.StartAsync();

                    messagesList.Items.Add("Connection started");
                    loginButton.Visibility = Visibility.Hidden;
                    userTextBox.Visibility = Visibility.Hidden;
                    passwordBox.Visibility = Visibility.Hidden;
                    userLabel.Visibility = Visibility.Hidden;
                    passwordLabel.Visibility = Visibility.Hidden;

                    sendMessageButton.Visibility = Visibility.Visible;
                    messageTextBox.Visibility = Visibility.Visible;
                    messagesList.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _connection.InvokeAsync("SendMessageToCaller", userTextBox.Text, messageTextBox.Text);
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        }

        private static async Task<string?> GetAuthenticationToken(LoginRequest loginRequest)
        {
            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
            var httpResponse = await new HttpClient().PostAsync("http://localhost:5126/api/Authentication", content);

            var httpResponseContent = await httpResponse.Content.ReadAsStringAsync();

            if(httpResponse.IsSuccessStatusCode)
            {
                return httpResponseContent;
            }

            return null;
        }

        private sealed class LoginRequest
        {
            public required string UserName { get; set; }
            public required string Password { get; set; }
        }
    }
}