namespace Plato.Services
{
    public interface IChatHelper
    {
        public Task LoadChats();
        public void AddMessageToChat(string username, string message);
        public void AddChat(string username);
        public IList<string> GetChat(string username);
        public int GetChatMessageCount(string username);
    }
}
