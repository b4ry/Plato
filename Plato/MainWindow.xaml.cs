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
        private readonly HubConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            string? _token = GetAuthenticationToken();

            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/chat", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_token);
                })
                .Build();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{user}: {message}";
                    messagesList.Items.Add(newMessage);
                });
            });

            try
            {
                await connection.StartAsync();

                messagesList.Items.Add("Connection started");
                connectButton.IsEnabled = false;
                sendMessageButton.IsEnabled = true;
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
                await connection.InvokeAsync("SendMessageToCaller", userTextBox.Text, messageTextBox.Text);
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        }

        private string GetAuthenticationToken()
        {
            var loginRequest = new LoginRequest()
            {
                UserName = "b4ry",
                Password = "test"
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
            var httpResponse = new HttpClient().PostAsync("http://localhost:5126/api/Authentication", content).Result;

            return httpResponse.Content.ReadAsStringAsync().Result;
        }

        private sealed class LoginRequest
        {
            public required string UserName { get; set; }
            public required string Password { get; set; }
        }
    }
}