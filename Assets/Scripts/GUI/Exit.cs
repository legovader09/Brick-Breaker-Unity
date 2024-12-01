using System.Collections;
using EventListeners;
using LevelData;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GUI
{
    public class Exit : MonoBehaviour
    {
        bool _exitMode;
        public void CloseGame()
        {
            if (Globals.Score > 100 && Globals.Lives == 0)
            {
                _exitMode = true;
                StartCoroutine(ShowSubmitScoreUI());
            }
            else SceneManager.LoadScene("MainMenu");

        }

        public void Restart()
        {
            if (Globals.Score > 100)
            {
                _exitMode = false;
                StartCoroutine(ShowSubmitScoreUI());
            }
            else GameObject.Find("EventSystem").GetComponent<GameTracker>().RestartGame();
            gameObject.GetComponent<SoundHelper>().PlaySound("Sound/BGM/Conditions/Restart");
        }

        IEnumerator ShowSubmitScoreUI()
        {
            var o = Instantiate(GameObject.Find("EventSystem").GetComponent<GameTracker>().scoreDialog, 
                GameObject.Find("UICanvas").transform);
            var d = o.GetComponent<Dialog>();
            d.CreateDialog(
                "Submit Highscore",
                $"Would you like to submit your highscore of {Globals.Score}?" +
                $"\nPlease enter your name below:", true);
        
            while (d.DialogResult == DialogResult.None)
                yield return null; //yield return null allows the coroutine to jump to this line on every call, so while dialog result is none, it will be stuck to returning here.

            var hasPressedConfirm = false;

            if (d.DialogResult == DialogResult.Confirm)
            { 
                if (!hasPressedConfirm)
                {
                    hasPressedConfirm = true; //this ensures you cannot spam click the confirm button and cause glitches when uploading highscore.
                    StartCoroutine(o.GetComponent<HighscoreController>().PostHighscore(d.ResultText.text, Globals.Score,
                        GetComponent<GameTracker>().currentWave)); //post score to database.

                    while (o.GetComponent<HighscoreController>().PostSuccess == null)
                        yield return null;

                    if (o.GetComponent<HighscoreController>().PostSuccess == true)
                        d.CreateDialog("Success!", "Your highscore has been successfully submitted.", false, okOnly: true);
                    else if (o.GetComponent<HighscoreController>().PostSuccess == false) //needs to be an else if because nullable bool now has 3 values instead of 2, so therefore can't have a flip condition.
                        d.CreateDialog("Error!", "Could not connect to scoreboard.", false, okOnly: true);

                    while (d.DialogResult == DialogResult.None)
                        yield return null;
                }
            }

            if (_exitMode)
                SceneManager.LoadScene("MainMenu");
            else
                GetComponent<GameTracker>().RestartGame();

            Destroy(o);
        }

        public void LoadNextLevel()
        {
            if (!Globals.CustomLevel)
            {
                if (Globals.EndlessMode) //generate new endless level
                {
                    Globals.EndlessLevelData = EndlessLevelGenerator.Generate();
                }
                else if (GetComponent<GameTracker>().currentWave >= Globals.AmountOfLevels) //start endless mode after completing the main levels.
                {
                    Globals.EndlessMode = true;
                    Globals.EndlessLevelData = EndlessLevelGenerator.Generate();
                }
                GetComponent<GameTracker>().currentWave++;
                if (Globals.Lives < 6) Globals.Lives++; else GetComponent<GameTracker>().UpdateScore(100); //add a life on every level completion, if player already has 6 lives, then give 100 points instead.
                GameObject.FindGameObjectWithTag("Ball").GetComponent<BallLogic>().levelMultiplier = GetComponent<GameTracker>().currentWave * 30f; //level speed increase by the level number * a constant of 30.
                GameObject.FindGameObjectWithTag("Ball").GetComponent<BallLogic>().ResetSpeed(); //reset speed if speed multiplier is active.
            }

            GetComponent<GameTracker>().LoadNextWave();
            GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<PlayerController>().ResetPaddlePosition();
            GameObject.FindGameObjectWithTag("Ball").gameObject.GetComponent<BallLogic>().ResetBall();
            Globals.GamePaused = false;
        }
    }
}
