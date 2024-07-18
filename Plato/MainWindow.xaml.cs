using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Plato.Constants;
using Plato.ExternalServices;
using Plato.Models;
using Plato.Models.DTOs;
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
        private readonly Dictionary<string, User> _users = [];
        private string _currentChatUsername = ChatDefaultChannelNames.Server;
        private string? _token;

        public ObservableCollection<string> CurrentChat { get; set; } = [];
        public ObservableCollection<User> Users { get; set; } = [];

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            var serverUser = new User() { Name = ChatDefaultChannelNames.Server, HasNewMessage = false };
            _users.Add(ChatDefaultChannelNames.Server, serverUser);
            Users.Add(serverUser);

            _connection = new HubConnectionBuilder()
                .WithUrl(ConfigurationManager.AppSettings.Get("ChatHubUrl")!, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_token);
                })
                .AddMessagePackProtocol()
                .Build();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginRequest = new UserLoginRequest(authUserTextBox.Text, passwordBox.Password);

                _token = await CerberusApi.GetAuthenticationToken(loginRequest);

                if (_token != null)
                {
                    RegisterListeners();

                    await _connection.StartAsync();

                    SetAuthFieldsVisibility(Visibility.Hidden);
                    SetChatFieldsVisibility(Visibility.Visible);
                }
            }
            catch (Exception ex)
            {
                //messagesList.Items.Add(ex.Message);
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var registerRequest = new UserRegisterRequest(authUserTextBox.Text, passwordBox.Password);
                resultLabel.Content = await CerberusApi.RegisterUser(registerRequest);
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
                CurrentChat.Add(messageTextBox.Text);
                _chats[_currentChatUsername].Add(messageTextBox.Text); // TODO: it should be possible to set a reference of this chat to current chat

                await _connection.InvokeAsync(ChatHubEndpointNames.SendMessage, _currentChatUsername, messageTextBox.Text);
            }
            catch (Exception ex)
            {
                //messagesList.Items.Add(ex.Message);
            }
        }

        private void ChangeChat(object sender, SelectionChangedEventArgs args)
        {
            _currentChatUsername = ((sender as ListBox)?.SelectedItem as User)?.Name;

            if(_currentChatUsername == null)
            {
                _currentChatUsername = ChatDefaultChannelNames.Server; // redirect to the server channel
                usersList.SelectedItem = _users[_currentChatUsername];
            }

            _users[_currentChatUsername].HasNewMessage = false;

            CurrentChat.Clear();

            if (!_chats.ContainsKey(_currentChatUsername))
            {
                _chats.Add(_currentChatUsername, []);
            }

            foreach (var message in _chats[_currentChatUsername])
            {
                CurrentChat.Add(message);
            }
        }

        private void RegisterListeners()
        {
            _connection.On<string, string>(ListenerMethodNames.ReceiveMessage, (username, message) =>
            {
                this.Dispatcher.Invoke((Delegate)(() =>
                {
                    var newMessage = username != ChatDefaultChannelNames.Server ? $"{username}: {message}" : $"{message}";

                    if (!_chats.ContainsKey(username))
                    {
                        _chats.Add(username, []);
                    }

                    _chats[username].Add(newMessage);

                    if (string.Equals(username, _currentChatUsername))
                    {
                        CurrentChat.Add(newMessage);
                    }
                    else
                    {
                        _users[username].HasNewMessage = true;
                    }
                }));
            });

            _connection.On<string>(ListenerMethodNames.NewUserJoinedChat, (username) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    AddNewUser(username);
                });
            });

            _connection.On<string>(ListenerMethodNames.UserLoggedOut, (username) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var userToDelete = _users[username];

                    Users.Remove(userToDelete);
                    _users.Remove(username);
                });
            });

            _connection.On(ListenerMethodNames.GetUsers, (Action<IEnumerable<string>>)((users) =>
            {
                this.Dispatcher.Invoke((Delegate)(() =>
                {
                    foreach (var username in users)
                    {
                        AddNewUser(username);
                    }
                }));
            }));
        }

        private void AddNewUser(string username)
        {
            var newUser = new User() { Name = username, HasNewMessage = false };

            _users.Add(username, newUser);
            Users.Add(newUser);
        }

        private void SetAuthFieldsVisibility(Visibility visibility)
        {
            loginButton.Visibility = visibility;
            authUserTextBox.Visibility = visibility;
            passwordBox.Visibility = visibility;
            userLabel.Visibility = visibility;
            passwordLabel.Visibility = visibility;
            resultLabel.Visibility = visibility;
            registerButton.Visibility = visibility;
        }

        private void SetChatFieldsVisibility(Visibility visibility)
        {
            sendMessageButton.Visibility = visibility;
            messageTextBox.Visibility = visibility;
            messagesList.Visibility = visibility;
            usersList.Visibility = visibility;
        }
    }
}