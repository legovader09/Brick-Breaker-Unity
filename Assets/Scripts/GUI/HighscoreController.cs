using System.Collections;
using LevelData;
using PlayerIOClient;
using UnityEngine;

namespace GUI
{
    public class HighscoreController : MonoBehaviour
    {
        internal bool HasLoaded;
        internal bool? PostSuccess;
        public GameObject canvas;
        public GameObject leadboardEntryPrefab;

        /// <summary>
        /// Posts score to database.
        /// </summary>
        /// <param name="playerName">Username of player</param>
        /// <param name="score">The score they achieved.</param>
        /// <param name="level">The highest level they achieved</param>
        /// <returns></returns>
        internal IEnumerator PostHighscore(string playerName, int score, int level)
        {
            var client = Globals.Client;
            client.ConnectToOnlineService(playerName);
            yield return new WaitUntil(() => Globals.Client.ConsistentClient != null);

            client.ConsistentClient.Leaderboards.Set("highscores", "score", score, null);
            client.ConsistentClient.Leaderboards.Set("highscores", "levels", level, null);

            PostSuccess = true;
            client.Disconnect();
            yield return true;
        }

        /// <summary>
        /// Get the scores from the MySQL DB to display in a GUIText.
        /// </summary>
        /// <returns></returns>
        internal IEnumerator GetHighscores()
        {
            var client = Globals.Client;
            if (client.ConnectToOnlineService())
            {
                yield return new WaitUntil(() => client.ConsistentClient != null);

                client.ConsistentClient.Leaderboards.GetTop("highscores", "score", 0, 20, null,
                    delegate (LeaderboardEntry[] e)
                    {
                        foreach (var entry in e)
                        {
                            var entryPrefab = Instantiate(leadboardEntryPrefab, canvas.transform);
                            entryPrefab.GetComponent<LeaderboardEntryComponent>().SetData(entry.Rank, entry.ConnectUserId, entry.Score);
                        }
                    }); 
            }

            yield return null;
        }
    }
}