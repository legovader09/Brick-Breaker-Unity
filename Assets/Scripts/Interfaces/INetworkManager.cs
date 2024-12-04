namespace Interfaces
{
    public interface INetworkManager
    {
        bool ConnectToOnlineService(string name = "guest");
        void Disconnect();
    }
}