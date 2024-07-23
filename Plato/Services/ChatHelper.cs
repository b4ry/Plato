using Microsoft.EntityFrameworkCore;
using Plato.DatabaseContext;
using Plato.Encryption.Interfaces;

namespace Plato.Services
{
    public class ChatHelper : IChatHelper
    {
        private readonly Dictionary<string, IList<string>> _chats = [];
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IAESEncryption _aesEncryption;

        public ChatHelper(ApplicationDbContext applicationDbContext, IAESEncryption aesEncryption)
        {
            _applicationDbContext = applicationDbContext;
            _aesEncryption = aesEncryption;
        }

        public void AddChat(string username)
        {
            if(_chats.ContainsKey(username))
            {
                return;
            }

            _chats.Add(username, []);
        }

        public void AddMessageToChat(string username, string message)
        {
            _chats[username].Add(message); // TODO: it should be possible to set a reference of this chat to current chat
        }

        public IList<string> GetChat(string username)
        {
            if (!_chats.ContainsKey(username))
            {
                return [];
            }

            return _chats[username];
        }

        public int GetChatMessageCount(string username)
        {
            if (!_chats.ContainsKey(username))
            {
                return 0;
            }

            return _chats[username].Count;
        }

        public async Task LoadChats()
        {
            var chats = await _applicationDbContext.Messages
                .GroupBy(message => message.Username)
                .Select(group => new
                {
                    Username = group.Key,
                    Messages = group.OrderBy(g => g.Order).Select(g => g.Message).ToList()
                })
                .ToListAsync();

            foreach (var chat in chats)
            {
                if (!_chats.ContainsKey(chat.Username))
                {
                    _chats[chat.Username] = [];

                    foreach (var encryptedMessage in chat.Messages)
                    {
                        var message = await _aesEncryption.Decrypt(encryptedMessage);

                        _chats[chat.Username].Add(message);
                    }
                }
            }
        }
    }
}
