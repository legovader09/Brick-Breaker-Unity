using System.Collections;
using Constants;
using EventListeners;
using LevelData;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GUI
{
    public class PauseMenuHelper : MonoBehaviour
    {
        private bool _exitMode;
        public void CloseGame()
        {
            if (Globals.Score > 100 && Globals.Lives == 0)
            {
                _exitMode = true;
                StartCoroutine(ShowSubmitScoreUI());
            }
            else SceneManager.LoadScene("MainMenu");
        }

        private IEnumerator ShowSubmitScoreUI()
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
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (!hasPressedConfirm)
                {
                    hasPressedConfirm = true; //this ensures you cannot spam click the confirm button and cause glitches when uploading highscore.
                    StartCoroutine(o.GetComponent<HighscoreController>().PostHighscore(d.ResultText.text, Globals.Score,
                        GetComponent<GameTracker>().currentLevel)); //post score to database.

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

        public void Restart()
        {
            if (Globals.Score > 100)
            {
                _exitMode = false;
                StartCoroutine(ShowSubmitScoreUI());
            }
            else GetComponent<GameTracker>().RestartGame();
            gameObject.GetComponent<SoundHelper>().PlaySound("Sound/BGM/Conditions/Restart");
        }
        
        #region "Settings menu UI"
        public void SetVolumeSlider(Slider g)
        {
            switch (g.gameObject.name)
            {
                case "SFXSlider":
                    PlayerPrefs.SetFloat(ConfigConstants.SFXVolumeSetting, g.value);
                    break;
                case "BGMSlider":
                    PlayerPrefs.SetFloat(ConfigConstants.BGMVolumeSetting, g.value);
                    gameObject.GetComponent<AudioSource>().volume = g.value;
                    break;
            }
        }

        public void HideUIMenu(GameObject menuPanel) => menuPanel.SetActive(false);
        public void ShowUIMenu(GameObject menuPanel)
        {
            menuPanel.SetActive(true);

            if (menuPanel.name != "pnlSettings") return;
            GameObject.Find("SFXSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat(ConfigConstants.SFXVolumeSetting);
            GameObject.Find("BGMSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat(ConfigConstants.BGMVolumeSetting);
        }
        #endregion
    }
}
