using Interfaces;
using PlayerIOClient;
using UnityEngine;

namespace Networking
{
    public class NetworkManager : INetworkManager
    {
        public Client ConsistentClient { get; private set; }
        private Connection Connection { get; set; }
        
        public bool ConnectToOnlineService(string name = "guest")
        {
            if (ConsistentClient != null) return true;

            PlayerIO.UseSecureApiRequests = true;
            PlayerIO.Authenticate(
                "brick-breaker-jeffkfgevk6eg5yp5m3jsq",
                "public",
                new()
                {
                    { "userId", name },
                },
                null,
                delegate (Client client)
                {
                    Debug.Log("Successfully connected to Player.IO");

                    Debug.Log("Create ServerEndpoint");
                    // Comment out the line below to use the live servers instead of your development server
                    //client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);
                    client.Multiplayer.UseSecureConnections = true;
                    ConsistentClient = client;

                    client.Multiplayer.CreateJoinRoom(
                        null,
                        "Brick",
                        true,
                        null,
                        null,
                        delegate (Connection connection) {
                            Debug.Log("Joined Room.");
                            Connection = connection;
                        },
                        delegate (PlayerIOError error) {
                            Debug.Log("Error Joining Room: " + error);
                        }
                    );
                },
                delegate (PlayerIOError error)
                {
                    Debug.Log("Error connecting: " + error);
                }
            );
            return true;
        }

        public void Disconnect()
        {
            if (ConsistentClient == null) return;
            Connection.Disconnect();
            Connection = null;
            ConsistentClient = null;
        }
    }
}