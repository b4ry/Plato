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
                _chats[_currentChatUser].Add(messageTextBox.Text); // TODO: it should be possible to set a reference of this chat to current chat

                await _connection.InvokeAsync(ChatHubEndpointNames.SendMessage, _currentChatUser, messageTextBox.Text);
            }
            catch (Exception ex)
            {
                //messagesList.Items.Add(ex.Message);
            }
        }

        private void ChangeChat(object sender, SelectionChangedEventArgs args)
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

        private void RegisterListeners()
        {
            _connection.On<string, string>(ListenerMethodNames.ReceiveMessage, (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = user != ChatDefaultChannelNames.Server ? $"{user}: {message}" : $"{message}";

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

            _connection.On<string>(ListenerMethodNames.NewUserJoinedChat, (user) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    Users.Add(user);
                });
            });

            _connection.On<string>(ListenerMethodNames.UserLoggedOut, (user) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    Users.Remove(user);
                });
            });

            _connection.On<IEnumerable<string>>(ListenerMethodNames.GetUsers, (users) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    foreach (var user in users)
                    {
                        Users.Add(user);
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