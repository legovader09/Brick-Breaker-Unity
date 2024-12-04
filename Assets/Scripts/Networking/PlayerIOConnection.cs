
using Constants;
using PlayerIOClient;
using UnityEngine;

namespace Networking
{
    public class PlayerIOConnection
    {
        private const string GameID = "brick-breaker-jeffkfgevk6eg5yp5m3jsq";
        private const string ConnectionID = "public";
        public Client Client;

        public PlayerIOConnection()
        {
            Authenticate();
        }

        private void Authenticate()
        {
            Application.runInBackground = true;
            Debug.Log("Starting online room...");
            PlayerIO.UseSecureApiRequests = true;
            PlayerIO.Authenticate(GameID, ConnectionID,
                new() { { "userId", PlayerPrefs.GetString(ConfigConstants.Username) } },
                null, delegate (Client client)
                {
                    Debug.Log("Successfully connected to Player.IO");
                    // Comment out the line below to use the live servers instead of your development server
                    // client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);
                    client.Multiplayer.UseSecureConnections = true;
                    Client = client;
                }, 
                delegate (PlayerIOError error)
                {
                    Debug.LogError("Error connecting: " + error);
                });
        }
    }
}