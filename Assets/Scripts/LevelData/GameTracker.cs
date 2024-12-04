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
        public GameObject scoreDialog;
        public GameObject player;
        public GameObject ball;
        public int currentLevel;
        public Vector2 startingCoords = new(150, 783);

        private int _lives;
        private bool _levelLoaded;

        // Start is called before the first frame update
        private void Awake()
        {
            pauseText.gameObject.SetActive(false);
            continueGameButton.gameObject.SetActive(false);
            safetyNet.SetActive(false);
        }

        // Update is called once per frame
        private void Update()
        {
            if (_lives != Globals.Lives)
            {
                _lives = Globals.Lives;

                var lives = GameObject.FindGameObjectsWithTag("Lives");
                foreach (var g in lives)
                    g.GetComponent<SpriteRenderer>().size = new(_lives * 2.54f, 2.54f); //update heart UI.

                if (_lives == 0)
                    GameOver();
            }

            if (Input.GetKeyDown(KeyCode.Escape)) //make sure you cant accidentally unpause game
            {
                PauseGame();
            }

            if (!_levelLoaded)
                RestartGame();
            else
                CheckLevelCompletion(); //only check level completion if there is a loaded level.
        }

        private void LateUpdate()
        {
            Globals.BricksRemaining = GameObject.FindGameObjectsWithTag("Brick").Length; //(non-priority) counts how many bricks are remaining.
        }

        public void PauseGame()
        {
            if (GameObject.FindGameObjectsWithTag("Brick").Length <= 0 || Globals.Lives == 0) return; //only allow pausing when it's not game over, and the level is not complete, otherwise the UI will overlap.
            Globals.GamePaused = !Globals.GamePaused;
            Time.timeScale = Globals.GamePaused ? 0f : 1f; //setting the time scale to 0 completely halts every physics simulation that is happening.
            pauseText.gameObject.SetActive(Globals.GamePaused);
            continueGameButton.gameObject.SetActive(Globals.GamePaused);
            gameObject.GetComponent<SoundHelper>().PauseSound();
            GameObject.Find("pnlSettings")?.SetActive(false);
        }

        public void LoadNextLevel()
        {
            if (!Globals.CustomLevel)
            {
                if (Globals.EndlessMode) //generate new endless level
                {
                    Globals.EndlessLevelData = EndlessLevelGenerator.Generate();
                }
                else if (currentLevel >= Globals.AmountOfLevels) //start endless mode after completing the main levels.
                {
                    Globals.EndlessMode = true;
                    Globals.EndlessLevelData = EndlessLevelGenerator.Generate();
                }
                currentLevel++;
                if (Globals.Lives < 6) Globals.Lives++; else UpdateScore(100); //add a life on every level completion, if player already has 6 lives, then give 100 points instead.
                ball.GetComponent<BallLogic>().levelMultiplier = GetComponent<GameTracker>().currentLevel * 30f; //level speed increase by the level number * a constant of 30.
                ball.GetComponent<BallLogic>().ResetSpeed(); //reset speed if speed multiplier is active.
            }

            LoadNextWave();
            player.GetComponent<PlayerController>().ResetPaddlePosition();
            ball.GetComponent<BallLogic>().ResetBall();
            Globals.GamePaused = false;
        }

        private void CheckLevelCompletion()
        {
            var bricksRemaining = GameObject.FindGameObjectsWithTag("Brick");
            switch (bricksRemaining.Length)
            {
                case 0:
                {
                    if (!Globals.GamePaused)
                    {
                        gameObject.GetComponent<SoundHelper>().StopSound(); //stop main bgm before playing victory sound.
                        gameObject.GetComponent<SoundHelper>().PlaySound("Sound/BGM/Conditions/LevelComplete");
                    }
                    Globals.GamePaused = true;
                    var allBalls = GameObject.FindGameObjectsWithTag("Ball");
                    foreach (var o in allBalls)
                    {
                        o.gameObject.GetComponent<BallLogic>().PauseBall();
                    }
                    levelCompleteTxt.gameObject.SetActive(true);
                    nextLvlButton.gameObject.SetActive(true);
                    break;
                }
                case > 0:
                    levelCompleteTxt.gameObject.SetActive(false);
                    nextLvlButton.gameObject.SetActive(false);
                    break;
            }
        }

        internal void RestartGame()
        {
            Globals.Lives = 3;
            Globals.Score = 0;
            scoreText.GetComponent<Text>().text = $"Score: {Globals.Score}";
            if (!Globals.CustomLevel) currentLevel = 1; //if custom level, reset current wave (level) also.
            if (Globals.EndlessMode) Globals.EndlessLevelData = EndlessLevelGenerator.Generate();
        
            LoadNextWave();

            var playerController = player.GetComponent<PlayerController>();
            playerController.ResetPaddlePosition();
            playerController.cancelFire = playerController.IsFiring; // cancel laser beams.
            ball.GetComponent<BallLogic>().ResetBall();
            Globals.GamePaused = false;
            pauseText.gameObject.GetComponent<Animator>().StartPlayback();
            pauseText.gameObject.SetActive(Globals.GamePaused);
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
                obj.AddComponent<Rigidbody2D>().gravityScale = Globals.Random.Next(3, 10);
                obj.GetComponent<Rigidbody2D>().AddForceAtPosition(new(100, 100), Vector2.zero);
            }
        }


        private void GameOver()
        {
            gameObject.GetComponent<SoundHelper>().StopSound();
            gameObject.GetComponent<SoundHelper>().PlaySound("Sound/BGM/Conditions/GameOver");
            MakeAllRemainingBricksFall();
            Globals.GamePaused = true;
            pauseText.SetActive(Globals.GamePaused);
            pauseText.GetComponent<Animator>().StopPlayback();
            pauseText.GetComponent<Text>().text = "GAME OVER";
            continueGameButton.gameObject.SetActive(false);
            Globals.EndlessMode = Globals.StartWithEndless;
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
                if (o.GetComponent<BallLogic>().IsFake) Destroy(o); else o.GetComponent<BallLogic>().ActivateFireBall(false);
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
        internal void LoadNextWave()
        {
            gameObject.GetComponent<SoundHelper>().PlaySound($"Sound/BGM/BGM_{Globals.Random.Next(1, 6)}", true);

            var currentCoords = startingCoords;
            var width = brickPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
            var height = brickPrefab.GetComponent<SpriteRenderer>().bounds.size.y;
            DestroyAllBricksAndPowerups();
            Time.timeScale = 1f;
            var tempBrick = brickPrefab;
            tempBrick.GetComponent<BrickComponent>().colour = BrickColours.Purple;
            string levelStruct;
            if (Globals.CustomLevel)
            {
                levelStruct = Globals.CustomLevelData;
                lvlIndicatorTxt.text = "Custom";
            }
            else if (Globals.EndlessMode)
            {
                levelStruct = Globals.EndlessLevelData;
                lvlIndicatorTxt.text = $"Endless {currentLevel}";
            }
            else
            {
                levelStruct = Resources.Load<TextAsset>($"Levels/level{currentLevel}").text;
                lvlIndicatorTxt.text = $"Level {currentLevel}";
            }
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
            if (Globals.GamePaused) return;
            scoreToAdd = Mathf.FloorToInt(scoreToAdd * Globals.ScoreMultiplier);
            Instantiate(indicator, player.transform).GetComponent<PointsIndicator>().ShowPoints(scoreToAdd);

            Globals.Score += scoreToAdd;
            scoreText.GetComponent<Text>().text = $"Score: {Globals.Score}";
        }
    }
}
