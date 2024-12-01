using UnityEngine;
using UnityEngine.UI;

namespace GUI
{
    public class LeaderboardEntryComponent : MonoBehaviour
    {
        public Text entryRankText;
        public Text entryConnectUserIdText;
        public Text entryScoreText;

        public void SetData(uint entryRank, string entryConnectUserId, long entryScore)
        {
            entryRankText.text = entryRank.ToString();
            entryConnectUserIdText.text = entryConnectUserId;
            entryScoreText.text = entryScore.ToString();
        }
    }
}
