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
            Globals.ConnectToPlayerIO(playerName);
            yield return new WaitUntil(() => Globals.ConsistentClient != null);

            Globals.ConsistentClient.Leaderboards.Set("highscores", "score", score, null);
            Globals.ConsistentClient.Leaderboards.Set("highscores", "levels", level, null);

            PostSuccess = true;
            yield return true;
        }

        /// <summary>
        /// Get the scores from the MySQL DB to display in a GUIText.
        /// </summary>
        /// <returns></returns>
        internal IEnumerator GetHighscores()
        {
            if (Globals.ConnectToPlayerIO())
            {
                yield return new WaitUntil(() => Globals.ConsistentClient != null);

                Globals.ConsistentClient.Leaderboards.GetTop("highscores", "score", 0, 20, null,
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