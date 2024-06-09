using Microsoft.AspNetCore.SignalR.Client;
using Plato.Constants;
using Plato.DTOs;
using Plato.ExternalServices;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace Plato
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HubConnection _connection;
        private readonly Dictionary<string, IList<string>> _chats = new() { { ChatDefaultChannelNames.Server, [] } };
        private string _currentChatUser = ChatDefaultChannelNames.Server;
        private string? _token;

        public ObservableCollection<string> CurrentChat { get; set; } = [];
        public ObservableCollection<string> Users { get; set; } = [ChatDefaultChannelNames.Server];

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            _connection = new HubConnectionBuilder()
                .WithUrl(ConfigurationManager.AppSettings.Get("ChatHubUrl")!, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_token);
                })
                .Build();

            _connection.On<string, string>(ChatHubEndpointNames.ReceiveMessage, (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{user}: {message}";

                    if (!_chats.ContainsKey(user))
                    {
                        _chats.Add(user, []);
                    }

                    _chats[user].Add(newMessage);

                    if (string.Equals(user, _currentChatUser))
                    {
                        CurrentChat.Add(newMessage);
                    }
                });
            });

            _connection.On<string>(ChatHubEndpointNames.NewUserJoinedChat, (user) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    Users.Add(user);
                });
            });
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginRequest = new LoginRequest(loginUserTextBox.Text, passwordBox.Password);

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
                    usersList.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                //messagesList.Items.Add(ex.Message);
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
                //messagesList.Items.Add(ex.Message);
            }
        }

        void ChangeChat(object sender, SelectionChangedEventArgs args)
        {
            _currentChatUser = (sender as ListBox)!.SelectedItem.ToString()!;

            CurrentChat.Clear();

            if (!_chats.ContainsKey(_currentChatUser))
            {
                _chats.Add(_currentChatUser, []);
            }

            foreach (var message in _chats[_currentChatUser])
            {
                CurrentChat.Add(message);
            }
        }
    }
}