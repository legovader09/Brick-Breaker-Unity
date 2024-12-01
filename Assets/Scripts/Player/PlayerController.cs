using System.Collections;
using EventListeners;
using GUI;
using LevelData;
using Powerups;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

namespace Player
{
    /// <summary>
    /// A class for all paddle related events.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        private Vector2 _mousePos;
        private bool _hasBallAttached = true;
        private Vector3 _originalSize;
        private Vector3 _enlargedPaddle;
        private Vector3 _shrunkPaddle;
        private Vector2 _originalPosition;
        private GameObject _ball;
        [FormerlySerializedAs("LaserBeamPrefab")] public GameObject laserBeamPrefab;
        public GameObject arrowAnchor;
        public GameObject powerupHelper;
        GameTracker _score;
        GUIHelper _powerupUI;
        public bool cancelFire;

        /// <summary>Used to indicate whether the paddle is firing laser beams.</summary> 
        public bool IsFiring { get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            _ball = GameObject.FindGameObjectWithTag("Ball");
            _score = GameObject.Find("EventSystem").GetComponent<GameTracker>(); 
            _powerupUI = GameObject.Find("EventSystem").GetComponent<GUIHelper>();
            _originalSize = gameObject.transform.localScale;
            _enlargedPaddle = new(gameObject.transform.localScale.x * 1.5f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            _shrunkPaddle = new(gameObject.transform.localScale.x * 0.75f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            _originalPosition = gameObject.transform.position;
        }

        internal void ResetPaddlePosition()
        {
            gameObject.transform.position = _originalPosition;
        }

        internal void ResetPaddleSize()
        {
            gameObject.transform.localScale = _originalSize;
            _powerupUI.RemovePowerupFromSidebar(PowerupCodes.ShrinkPaddle);
            _powerupUI.RemovePowerupFromSidebar(PowerupCodes.GrowPaddle);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!Globals.GamePaused)
            {
                if (Globals.AIMode) // AI  mode is used for TESTING purposes only and can be toggled by pressing T in game.
                {
                    if (_ball.GetComponent<BallLogic>().currentVelocity.y > 0) // if ball is going upwards
                    {
                        var powerUp = GameObject.FindGameObjectWithTag("Powerup");
                        _mousePos = Camera.main.ScreenToWorldPoint(powerUp ? powerUp.transform.position : //follow powerup if there is one, if not keep following ball.
                            _ball.transform.position);
                    }
                    else
                    {
                        _mousePos = Camera.main.ScreenToWorldPoint(_ball.transform.position); //follow ball
                    }
                }
                else
                {
                    _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //follow mouse if not AI Mode.
                }
                gameObject.GetComponent<Rigidbody2D>().MovePosition(new(_mousePos.x, 0));
            }
            else gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero; 
        }

        void LateUpdate()
        {
            if (_hasBallAttached && !Globals.GamePaused)
            {
                if ((Input.GetMouseButtonDown(0) && Input.mousePosition.y < 850) || Globals.AIMode)
                {
                    var ball = _ball.GetComponent<BallLogic>();
                    ball.stuckToPlayer = false;
                    _hasBallAttached = false;
                }
            }

            if (_ball.GetComponent<BallLogic>().stuckToPlayer) _hasBallAttached = true;

#if UNITY_EDITOR //editor ONLY, AI mode will not work in the actual game. This mode is for testing purposes only.
            if (Input.GetKeyUp(KeyCode.T))
                Globals.AIMode = !Globals.AIMode;
#endif
            if (Input.GetKeyUp(KeyCode.R)) //if the ball is stuck, or extremely slow, the player can press the R key on the keyboard to reset the ball, this will cost one player life.
            {
                _ball.GetComponent<BallLogic>().ResetBall();
                Globals.Lives--;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Wall")) //if paddle hits map boundaries, immediately stop acceleration to prevent clipping.
            {
                gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Powerup"))
            {
                ActivatePowerup(collision.gameObject.GetComponent<PowerupComponent>().PowerupType);
                Destroy(collision.gameObject);

                gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Pickup_04");
            }
        }

        /// <summary>
        /// Activates a powerup that has been picked up by the player.
        /// </summary>
        /// <param name="id">The ID of the powerup that has been collided with.</param>
        private void ActivatePowerup(PowerupCodes id)
        {
            switch (id)
            {
                case PowerupCodes.Pts50:
                    _score.UpdateScore(50);
                    break;
                case PowerupCodes.Pts100:
                    _score.UpdateScore(100);
                    break;
                case PowerupCodes.Pts250:
                    _score.UpdateScore(250);
                    break;
                case PowerupCodes.Pts500:
                    _score.UpdateScore(500);
                    break;
                case PowerupCodes.SlowBall:
                    var slowCall = _ball.GetComponent<BallLogic>().ChangeSpeed(2); //setting the function to a IEnumerator variable allows me to stop the coroutine and restart it.
                    StopCoroutine(slowCall);
                    StartCoroutine(slowCall);
                    _score.UpdateScore(20);
                    _powerupUI.AddPowerupToSidebar(id);
                    _powerupUI.RemovePowerupFromSidebar(PowerupCodes.FastBall);
                    break;
                case PowerupCodes.FastBall:
                    var speedCall = _ball.GetComponent<BallLogic>().ChangeSpeed(1); //setting the function to a IEnumerator variable allows me to stop the coroutine and restart it.
                    StopCoroutine(speedCall);
                    StartCoroutine(speedCall);
                    _score.UpdateScore(40);
                    _powerupUI.AddPowerupToSidebar(id);
                    _powerupUI.RemovePowerupFromSidebar(PowerupCodes.SlowBall);
                    break;
                case PowerupCodes.TripleBall:
                    for (var i = 0; i < 2; i++)
                    {
                        _hasBallAttached = false;
                        var temp = Instantiate(_ball, _ball.transform.parent, false);
                        temp.GetComponent<BallLogic>().IsFake = true;
                        temp.GetComponent<SpriteShapeRenderer>().color = Color.green;
                        var balls = GameObject.FindGameObjectsWithTag("Ball");
                        foreach (var b in balls)
                        {
                            b.GetComponent<BallLogic>().stuckToPlayer = false;
                        }
                    }
                    _score.UpdateScore(50);
                    break;
                case PowerupCodes.ShrinkPaddle:
                    gameObject.transform.localScale = _shrunkPaddle;
                    _score.UpdateScore(50);
                    _powerupUI.AddPowerupToSidebar(id);
                    _powerupUI.RemovePowerupFromSidebar(PowerupCodes.GrowPaddle);
                    break;
                case PowerupCodes.GrowPaddle:
                    gameObject.transform.localScale = _enlargedPaddle;
                    _score.UpdateScore(20);
                    _powerupUI.AddPowerupToSidebar(id);
                    _powerupUI.RemovePowerupFromSidebar(PowerupCodes.ShrinkPaddle);
                    break;
                case PowerupCodes.LaserBeam:
                    StartCoroutine(FireBeams());
                    _powerupUI.AddPowerupToSidebar(id);
                    _score.UpdateScore(50);
                    break;
                case PowerupCodes.LifeUp:
                    if (Globals.Lives < 6)
                        Globals.Lives++;
                    else
                        _score.UpdateScore(100);
                    break;
                case PowerupCodes.SafetyNet:
                    Instantiate(powerupHelper, gameObject.transform).GetComponent<PowerupHelper>().SetSafetyNet();
                    _powerupUI.AddPowerupToSidebar(id);
                    _score.UpdateScore(50);
                    break;
                case PowerupCodes.DoublePoints:
                    Instantiate(powerupHelper, gameObject.transform).GetComponent<PowerupHelper>().SetMultiplier(2f);
                    _powerupUI.AddPowerupToSidebar(id);
                    _powerupUI.RemovePowerupFromSidebar(PowerupCodes.HalfPoints);
                    break;
                case PowerupCodes.RedFireBall:
                    _ball.GetComponent<BallLogic>().ActivateFireBall(true);
                    _score.UpdateScore(50);
                    break;
                case PowerupCodes.HalfPoints:
                    Instantiate(powerupHelper, gameObject.transform).GetComponent<PowerupHelper>().SetMultiplier(0.5f);
                    _powerupUI.AddPowerupToSidebar(id);
                    _powerupUI.RemovePowerupFromSidebar(PowerupCodes.DoublePoints);
                    break;
            }
        }

        private IEnumerator FireBeams()
        {
            var shootDelay = 0.3f;
            var shotsFired = 0;
            while (shotsFired < 5)
            {
                IsFiring = true;
                if (!Globals.GamePaused)
                {
                    Instantiate(laserBeamPrefab, 
                            GameObject.FindGameObjectWithTag("MainCamera").transform, false).transform.position = 
                        GameObject.FindGameObjectWithTag("PlayerBeamPos1").transform.position;

                    gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Shoot_01");

                    yield return new WaitForSeconds(shootDelay);
                    shotsFired++;

                    Instantiate(laserBeamPrefab, 
                            GameObject.FindGameObjectWithTag("MainCamera").transform, false).transform.position = 
                        GameObject.FindGameObjectWithTag("PlayerBeamPos2").transform.position;

                    gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Shoot_01");

                    yield return new WaitForSeconds(shootDelay);
                    shotsFired++;
                }
                yield return new WaitUntil(() => Globals.GamePaused == false); //this line is iterated through if the game is running, but should the game be paused, the thread will come back tot his line again until the game is unpaused again.
                if (cancelFire) //if after the pause check, the firing has been cancelled, exit the loop immediately.
                {
                    cancelFire = false;
                    break;
                }
            }
            GameObject.Find("EventSystem").GetComponent<GUIHelper>().RemovePowerupFromSidebar(PowerupCodes.LaserBeam);
            IsFiring = false;
        }
    }
}
