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
        public Dialog dialogPrefab;
        public GameSessionData sessionData;
        private GameObject _eventSystem;
        private bool _exitMode;

        private void Awake()
        {
            _eventSystem = GameObject.Find("EventSystem");
        }
        
        public void CloseGame()
        {
            if (sessionData.Score > 100 && sessionData.lives == 0)
            {
                _exitMode = true;
                ShowSubmitScoreUI();
            }
            else SceneManager.LoadScene("MainMenu");
        }

        private void ShowSubmitScoreUI()
        {
            StartCoroutine(Dialog.ShowMessageDialog(dialogPrefab, val =>
                {
                    if (val != DialogResult.Confirm) return;
                    StartCoroutine(_eventSystem.GetComponent<HighscoreController>().PostHighscore(sessionData.Score,
                        sessionData.CurrentLevel, success =>
                        {
                            StartCoroutine(success
                                ? Dialog.ShowMessageDialog(dialogPrefab, _ => OnSubmitScore(), "Success!",
                                    "Your high score has been successfully submitted.")
                                : Dialog.ShowMessageDialog(dialogPrefab, _ => OnSubmitScore(), "Error!",
                                    "Could not connect to game server."));
                        }));
                },
                "Submit Highscore",
                $"Would you like to submit your highscore of {sessionData.Score}?",
                false));
            return;

            void OnSubmitScore()
            {
                if (_exitMode)
                    SceneManager.LoadScene("MainMenu");
                else
                    GetComponent<GameTracker>().RestartGame();
            }
        }

        public void Restart()
        {
            if (sessionData.Score > 100)
            {
                _exitMode = false;
                ShowSubmitScoreUI();
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
