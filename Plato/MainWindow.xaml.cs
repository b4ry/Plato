using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Plato.Constants;
using Plato.DTOs;
using Plato.ExternalServices;
using Plato.Models;
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
        private string _currentChatUsername = ChatDefaultChannelNames.Server;
        private string? _token;

        public ObservableCollection<string> CurrentChat { get; set; } = [];
        public ObservableCollection<User> Users { get; set; } = [ new User() { Name = ChatDefaultChannelNames.Server, HasNewMessage = false }];

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

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
            _currentChatUsername = ((sender as ListBox)!.SelectedItem as User)!.Name;
            Users.Single(x => x.Name == _currentChatUsername).HasNewMessage = false;

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
                        var user = Users.Single(x => x.Name == username);
                        user.HasNewMessage = true;
                    }
                }));
            });

            _connection.On<string>(ListenerMethodNames.NewUserJoinedChat, (username) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    Users.Add(new User() { Name = username, HasNewMessage = false });
                });
            });

            _connection.On<string>(ListenerMethodNames.UserLoggedOut, (username) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var userToDelete = Users.Single(x => x.Name == username);

                    Users.Remove(userToDelete);
                });
            });

            _connection.On<IEnumerable<string>>(ListenerMethodNames.GetUsers, (users) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    foreach (var user in users)
                    {
                        Users.Add(new User() { Name = user, HasNewMessage = false });
                    }
                });
            });
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