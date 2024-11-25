using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Plato.Constants;
using Plato.DatabaseContext;
using Plato.DatabaseContext.Entities;
using Plato.Encryption.Interfaces;
using Plato.ExternalServices;
using Plato.Models;
using Plato.Models.DTOs;
using Plato.Services;
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

        private readonly Dictionary<string, User> _users = [];

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IAESEncryption _aesEncryption;
        private readonly IRSAEncryption _rsaEncryption;
        private readonly IAuthenticationService _authenticationService;
        private readonly IChatHelper _chatHelper;

        private string _currentChatUsername = ChatDefaultChannelNames.Server;
        private string? _token;

        public ObservableCollection<string> CurrentChat { get; set; } = [];
        public ObservableCollection<User> Users { get; set; } = [];

        public MainWindow
            (
                ApplicationDbContext applicationDbContext,
                IAESEncryption aesEncryption,
                IRSAEncryption rsaEncryption,
                IAuthenticationService authenticationService,
                IChatHelper chatHelper
            )
        {
            InitializeComponent();
            this.DataContext = this;

            _applicationDbContext = applicationDbContext;
            _aesEncryption = aesEncryption;
            _rsaEncryption = rsaEncryption;
            _authenticationService = authenticationService;
            _chatHelper = chatHelper;

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

        #region Button click handlers

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginRequest = new UserLoginRequest(authUserTextBox.Text, passwordBox.Password);

                _token = await _authenticationService.GetAuthenticationToken(loginRequest);

                if (_token != null)
                {
                    await _aesEncryption.Create(passwordBox.Password);

                    RegisterListeners();

                    await _connection.StartAsync();

                    SetAuthFieldsVisibility(Visibility.Hidden);
                    SetChatFieldsVisibility(Visibility.Visible);

                    await _chatHelper.LoadChats();

                    usersList.SelectedItem = _users[_currentChatUsername];
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
                resultLabel.Content = await _authenticationService.RegisterUser(registerRequest);
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
                _chatHelper.AddMessageToChat(_currentChatUsername, messageTextBox.Text);

                await SaveNewMessage(messageTextBox.Text, true);

                await _connection.InvokeAsync(ChatHubEndpointNames.SendMessage, _currentChatUsername, messageTextBox.Text);
            }
            catch (Exception ex)
            {
                //messagesList.Items.Add(ex.Message);
            }
        }

        #endregion

        #region Register listeners

        private void RegisterListeners()
        {
            RegisterReceiveMessageListener();
            RegisterUserJoinsChatListener();
            RegisterUserLogsOutListener();
            RegisterGetUsersListener();
            RegisterGetAsymmetricPublicKeyListener();
        }

        private void RegisterGetAsymmetricPublicKeyListener()
        {
            _connection.On(ListenerMethodNames.GetAsymmetricPublicKey, (Action<string>)((asymmetricPublicKey) =>
            {
                this.Dispatcher.Invoke(async () =>
                {
                    _rsaEncryption.BuildFromXmlString(asymmetricPublicKey);

                    var encryptedSymmetricKey = Convert.ToBase64String(_rsaEncryption.Encrypt(_aesEncryption.Key));
                    var encryptedSymmetricIV = Convert.ToBase64String(_rsaEncryption.Encrypt(_aesEncryption.IV));

                    await _connection.InvokeAsync(ChatHubEndpointNames.StoreSymmetricKey, (encryptedSymmetricKey, encryptedSymmetricIV));
                });
            }));
        }

        private void RegisterGetUsersListener()
        {
            _connection.On(ListenerMethodNames.GetUsers, (Action<IEnumerable<string>>)((users) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    foreach (var username in users)
                    {
                        AddNewUser(username);
                    }
                });
            }));
        }

        private void RegisterUserLogsOutListener()
        {
            _connection.On<string>(ListenerMethodNames.UserLogsOut, (username) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var userToDelete = _users[username];

                    Users.Remove(userToDelete);
                    _users.Remove(username);
                });
            });
        }

        private void RegisterUserJoinsChatListener()
        {
            _connection.On<string>(ListenerMethodNames.UserJoinsChat, (username) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    AddNewUser(username);
                });
            });
        }

        private void RegisterReceiveMessageListener()
        {
            _connection.On<string, string>(ListenerMethodNames.ReceiveMessage, (username, message) =>
            {
                this.Dispatcher.Invoke(async () =>
                {
                    _chatHelper.AddChat(username);

                    await SaveNewMessage(message, false);

                    var decryptedMessage = await _aesEncryption.Decrypt(message);

                    if (string.Equals(username, _currentChatUsername))
                    {
                        CurrentChat.Add(decryptedMessage);
                    }
                    else
                    {
                        _chatHelper.AddMessageToChat(username, decryptedMessage);
                        _users[username].HasNewMessage = true;
                    }
                });
            });
        }

        #endregion

        #region UI Fields visibility

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

        #endregion

        private async Task SaveNewMessage(string newMessage, bool encrypt)
        {
            var newMessageEntity = new MessageEntity()
            {
                Username = _currentChatUsername,
                Message = encrypt ? await _aesEncryption.Encrypt(newMessage) : newMessage,
                Order = _chatHelper.GetChatMessageCount(_currentChatUsername)
            };

            _applicationDbContext.Add(newMessageEntity);

            await _applicationDbContext.SaveChangesAsync();
        }

        private void ChangeChat(object sender, SelectionChangedEventArgs args)
        {
            _currentChatUsername = ((sender as ListBox)?.SelectedItem as User)?.Name;

            if (_currentChatUsername == null)
            {
                _currentChatUsername = ChatDefaultChannelNames.Server; // redirect to the server channel
                usersList.SelectedItem = _users[_currentChatUsername];
            }

            _users[_currentChatUsername].HasNewMessage = false;

            CurrentChat.Clear();

            _chatHelper.AddChat(_currentChatUsername);

            foreach (var message in _chatHelper.GetChat(_currentChatUsername))
            {
                CurrentChat.Add(message);
            }
        }

        private void AddNewUser(string username)
        {
            var newUser = new User() { Name = username, HasNewMessage = false };

            _users.Add(username, newUser);
            Users.Add(newUser);
        }
    }
}