using Microsoft.AspNetCore.SignalR.Client;
using Plato.Constants;
using Plato.DTOs;
using Plato.ExternalServices;
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

            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/chat", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_token);
                })
                .Build();

            _connection.On<string, string>(ChatHubEndpointNames.ReceiveMessage, (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{user}: {message}";
                    messagesList.Items.Add(newMessage);
                });
            });
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginRequest = new LoginRequest()
                {
                    UserName = loginUserTextBox.Text,
                    Password = passwordBox.Password
                };

                _token = await CerberusApi.GetAuthenticationToken(loginRequest);

                if (_token != null)
                {
                    await _connection.StartAsync();

                    loginButton.Visibility = Visibility.Hidden;
                    loginUserTextBox.Visibility = Visibility.Hidden;
                    passwordBox.Visibility = Visibility.Hidden;
                    userLabel.Visibility = Visibility.Hidden;
                    passwordLabel.Visibility = Visibility.Hidden;

                    sendMessageButton.Visibility = Visibility.Visible;
                    messageTextBox.Visibility = Visibility.Visible;
                    userTextBox.Visibility = Visibility.Visible;
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
                await _connection.InvokeAsync(ChatHubEndpointNames.SendMessage, userTextBox.Text, messageTextBox.Text);
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        }
    }
}