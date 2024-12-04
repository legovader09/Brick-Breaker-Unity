using Bricks;
using DiscordRP;
using Enums;
using EventListeners;
using GUI;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LevelData
{
    /// <summary>
    /// Tracks any game-related events.
    /// </summary>
    public class GameTracker : MonoBehaviour
    {
        public GameObject pauseText;
        public Button continueGameButton;
        public GameObject brickPrefab;
        public GameObject brickParent;
        public GameObject scoreText;
        public Text levelCompleteTxt;
        public Button nextLvlButton;
        public Text lvlIndicatorTxt;
        public GameObject indicator;
        public GameObject safetyNet;
        public GameObject player;
        public GameObject ball;
        public GameObject heartContainer;
        public GameSessionData sessionData;
        public Vector2 startingCoords = new(150, 783);

        private int _lives;
        private bool _levelLoaded;
        private bool _levelComplete;

        // Start is called before the first frame update
        private void Awake()
        {
            pauseText.gameObject.SetActive(false);
            continueGameButton.gameObject.SetActive(false);
            safetyNet.SetActive(false);
            if (Time.timeScale == 0) Time.timeScale = 1;
        }

        // Update is called once per frame
        private void Update()
        {
            if (_lives != sessionData.lives)
            {
                _lives = sessionData.lives;

                var lives = new [] { heartContainer, heartContainer.transform.GetChild(0).gameObject };
                foreach (var g in lives)
                    g.GetComponent<SpriteRenderer>().size = new(_lives * 2.54f, 2.54f); //update heart UI.

                if (_lives == 0) GameOver();
            }

            if (Input.GetKeyDown(KeyCode.Escape)) TogglePauseGame();

            if (!_levelLoaded && !_levelComplete) RestartGame();
            else CheckLevelCompletion(); //only check level completion if there is a loaded level.
        }

        private void LateUpdate()
        {
            sessionData.BricksRemaining = GameObject.FindGameObjectsWithTag("Brick").Length; //(non-priority) counts how many bricks are remaining.
        }

        public void TogglePauseGame()
        {
            if (GameObject.FindGameObjectsWithTag("Brick").Length <= 0 || sessionData.lives == 0) return; //only allow pausing when it's not game over, and the level is not complete, otherwise the UI will overlap.
            sessionData.GamePaused = !sessionData.GamePaused;
            Time.timeScale = sessionData.GamePaused ? 0f : 1f; //setting the time scale to 0 completely halts every physics simulation that is happening.
            pauseText.gameObject.SetActive(sessionData.GamePaused);
            continueGameButton.gameObject.SetActive(sessionData.GamePaused);
            gameObject.GetComponent<SoundHelper>().TogglePauseSound();
            GameObject.Find("pnlSettings")?.SetActive(false);
        }

        public void LoadNextLevel()
        {
            if (!sessionData.customLevel)
            {
                if (sessionData.endlessMode) //generate new endless level
                {
                    sessionData.EndlessLevelData = EndlessLevelGenerator.Generate(sessionData.Random);
                }
                else if (sessionData.CurrentLevel >= sessionData.amountOfLevels) //start endless mode after completing the main levels.
                {
                    sessionData.endlessMode = true;
                    sessionData.EndlessLevelData = EndlessLevelGenerator.Generate(sessionData.Random);
                }
                sessionData.CurrentLevel++;
                if (sessionData.lives < 6) sessionData.lives++; else UpdateScore(100); //add a life on every level completion, if player already has 6 lives, then give 100 points instead.
                ball.GetComponent<BallLogic>().levelMultiplier = GetComponent<GameTracker>().sessionData.CurrentLevel * 30f; //level speed increase by the level number * a constant of 30.
                ball.GetComponent<BallLogic>().ResetSpeed(); //reset speed if speed multiplier is active.
            }

            LoadNextWave();
            ball.GetComponent<BallLogic>().ResetBall();
            TogglePauseGame();
        }

        private void CheckLevelCompletion()
        {
            switch (sessionData.BricksRemaining)
            {
                case 0:
                {
                    if (_levelComplete) return;
                    _levelComplete = true;
                    sessionData.GamePaused = true;
                    gameObject.GetComponent<SoundHelper>().StopSound(); //stop main bgm before playing victory sound.
                    gameObject.GetComponent<SoundHelper>().PlaySound("Sound/BGM/Conditions/LevelComplete");
                    var allBalls = GameObject.FindGameObjectsWithTag("Ball");
                    foreach (var o in allBalls)
                    {
                        o.gameObject.GetComponent<BallLogic>().PauseBall();
                    }
                    levelCompleteTxt.gameObject.SetActive(true);
                    if (!sessionData.customLevel) nextLvlButton.gameObject.SetActive(true);
                    break;
                }
                case > 0:
                    _levelComplete = false;
                    levelCompleteTxt.gameObject.SetActive(false);
                    nextLvlButton.gameObject.SetActive(false);
                    break;
            }
        }

        internal void RestartGame()
        {
            sessionData.lives = 3;
            sessionData.Score = 0;
            scoreText.GetComponent<Text>().text = $"Score: {sessionData.Score}";
            if (!sessionData.customLevel) sessionData.CurrentLevel = 1; //if custom level, reset current wave (level) also.
            if (sessionData.endlessMode) sessionData.EndlessLevelData = EndlessLevelGenerator.Generate(sessionData.Random);
        
            LoadNextWave();

            var playerController = player.GetComponent<PlayerController>();
            playerController.ResetPaddlePosition();
            playerController.cancelFire = playerController.IsFiring; // cancel laser beams.
            ball.GetComponent<BallLogic>().ResetBall();
            if (sessionData.GamePaused) TogglePauseGame();
            pauseText.gameObject.GetComponent<Animator>().StartPlayback();
            pauseText.gameObject.SetActive(sessionData.GamePaused);
            pauseText.gameObject.GetComponent<Text>().text = "GAME PAUSED";
            continueGameButton.gameObject.SetActive(true);

#if UNITY_STANDALONE 
            GetComponent<DiscordController>().ResetTimeElapsed(); //reset time spent on current run.
#endif
        }

        /// <summary>
        /// This is purely a visual, but adds gravity to all remaining bricks, and lets them fall from the sky on game over.
        /// </summary>
        private void MakeAllRemainingBricksFall()
        {
            var allObjects = GameObject.FindGameObjectsWithTag("Brick");
            foreach (var obj in allObjects) 
            {
                obj.AddComponent<Rigidbody2D>().gravityScale = sessionData.Random.Next(3, 10);
                obj.GetComponent<Rigidbody2D>().AddForceAtPosition(new(100, 100), Vector2.zero);
            }
        }


        private void GameOver()
        {
            gameObject.GetComponent<SoundHelper>().StopSound();
            gameObject.GetComponent<SoundHelper>().PlaySound("Sound/BGM/Conditions/GameOver");
            MakeAllRemainingBricksFall();
            sessionData.GamePaused = true;
            pauseText.SetActive(sessionData.GamePaused);
            pauseText.GetComponent<Animator>().StopPlayback();
            pauseText.GetComponent<Text>().text = "GAME OVER";
            continueGameButton.gameObject.SetActive(false);
            sessionData.endlessMode = sessionData.startWithEndless;
        }

        /// <summary>
        /// Resets every instance of everything.
        /// </summary>
        private void DestroyAllBricksAndPowerups() 
        {
            var allObjects = GameObject.FindGameObjectsWithTag("Brick");
            foreach (var obj in allObjects)
            {
                Destroy(obj);
            }
            allObjects = GameObject.FindGameObjectsWithTag("Powerup");
            foreach (var obj in allObjects)
            {
                Destroy(obj);
            }
            allObjects = GameObject.FindGameObjectsWithTag("LaserBeam");
            foreach (var obj in allObjects)
            {
                Destroy(obj);
            }
            allObjects = GameObject.FindGameObjectsWithTag("Ball");
            foreach (var o in allObjects)
            {
                if (o.GetComponent<BallLogic>().IsFake) Destroy(o);
            }
            allObjects = GameObject.FindGameObjectsWithTag("Indicator");
            foreach (var o in allObjects)
            {
                Destroy(o);
            }
            safetyNet.SetActive(false);

            gameObject.GetComponent<GUIHelper>().RemoveAllPowerups();
        }


        /// <summary>
        /// Load data from the next level
        /// </summary>
        private void LoadNextWave()
        {
            gameObject.GetComponent<SoundHelper>().PlaySound($"Sound/BGM/BGM_{sessionData.Random.Next(1, 6)}", true);
            DestroyAllBricksAndPowerups();
            string levelStruct;
            if (sessionData.customLevel)
            {
                levelStruct = sessionData.CustomLevelData;
                lvlIndicatorTxt.text = "Custom";
            }
            else if (sessionData.endlessMode)
            {
                levelStruct = sessionData.EndlessLevelData;
                lvlIndicatorTxt.text = $"Endless {sessionData.CurrentLevel}";
            }
            else
            {
                levelStruct = Resources.Load<TextAsset>($"Levels/level{sessionData.CurrentLevel}").text;
                lvlIndicatorTxt.text = $"Level {sessionData.CurrentLevel}";
            }
            
            var currentCoords = startingCoords;
            var width = brickPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
            var height = brickPrefab.GetComponent<SpriteRenderer>().bounds.size.y;
            var tempBrick = brickPrefab;
            tempBrick.GetComponent<BrickComponent>().colour = BrickColours.Purple;
            foreach (var c in levelStruct)
            {
                switch (c)
                { //this will read the text file containing level data, and lay out the bricks according to the specification below:
                    case '0': //no brick, empty gap.
                        currentCoords = new((int)(currentCoords.x + width), currentCoords.y);
                        break;
                    case '1': //brick taking up 1 gap.
                        Instantiate(tempBrick, brickParent.transform, false).transform.position = currentCoords;
                        currentCoords = new((int)(currentCoords.x + width), currentCoords.y);
                        break;
                    case ',': //new row.
                        tempBrick.GetComponent<BrickComponent>().colour = 
                            BrickColourHelper.GetNextColour(tempBrick.GetComponent<BrickComponent>().colour);
                        currentCoords = new(startingCoords.x, (int)(currentCoords.y - height));
                        break;
                }
            }

            _levelLoaded = true;
        }

        public void UpdateScore(int scoreToAdd)
        {
            if (sessionData.GamePaused) return;
            scoreToAdd = Mathf.FloorToInt(scoreToAdd * sessionData.scoreMultiplier);
            Instantiate(indicator, player.transform).GetComponent<PointsIndicator>().ShowPoints(scoreToAdd);

            sessionData.Score += scoreToAdd;
            scoreText.GetComponent<Text>().text = $"Score: {sessionData.Score}";
        }
    }
}
