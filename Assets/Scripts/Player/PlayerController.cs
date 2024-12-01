using System;
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
        public GameObject laserBeamPrefab;
        public GameObject arrowAnchor;
        public GameObject powerupHelper;
        private GameTracker _score;
        private GUIHelper _powerupUI;
        public bool cancelFire;
        private Rigidbody2D _rigidBody;
        private Camera _mainCamera;
        private GameObject _eventSystem;
        private Vector2 _mouseTargetPosition;

        /// <summary>Used to indicate whether the paddle is firing laser beams.</summary> 
        public bool IsFiring { get; private set; }

        // Start is called before the first frame update
        private void Start()
        {
            _eventSystem = GameObject.Find("EventSystem");
            _ball = GameObject.FindGameObjectWithTag("Ball");
            _score = _eventSystem.GetComponent<GameTracker>(); 
            _powerupUI = _eventSystem.GetComponent<GUIHelper>();
            _originalSize = gameObject.transform.localScale;
            _enlargedPaddle = new(gameObject.transform.localScale.x * 1.5f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            _shrunkPaddle = new(gameObject.transform.localScale.x * 0.75f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            _originalPosition = gameObject.transform.position;
            _rigidBody = gameObject.GetComponent<Rigidbody2D>();
            _mainCamera = Camera.main;
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

        private void FixedUpdate()
        {
            if (Globals.GamePaused)
            {
                _rigidBody.velocity = Vector2.zero;
                return;
            }

            if (Globals.AIMode) 
            {
                if (_ball.GetComponent<BallLogic>().currentVelocity.y > 0)
                {
                    var powerUp = GameObject.FindGameObjectWithTag("Powerup");
                    _mouseTargetPosition = _mainCamera.ScreenToWorldPoint(
                        powerUp ? powerUp.transform.position : _ball.transform.position);
                }
                else
                {
                    _mouseTargetPosition = _mainCamera.ScreenToWorldPoint(_ball.transform.position);
                }
            }

            // Smoothly move towards the target position
            var smoothedPosition = Vector2.Lerp(_rigidBody.position, _mouseTargetPosition, .8f);
            _rigidBody.MovePosition(new(smoothedPosition.x, _rigidBody.position.y));
        }

        private void LateUpdate()
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
            
            if (!Globals.GamePaused && !Globals.AIMode)
            {
                // Get the target position from the mouse
                var mousePosition = Input.mousePosition;
                var worldPoint = _mainCamera.ScreenToWorldPoint(mousePosition);
                // Calculate the desired position of the paddle based on the mouse position
                _mouseTargetPosition = new(worldPoint.x, 0);
            }

            if (_ball.GetComponent<BallLogic>().stuckToPlayer) _hasBallAttached = true;

#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.T))
                Globals.AIMode = !Globals.AIMode;
#endif
            if (!Input.GetKeyUp(KeyCode.R)) return;
            _ball.GetComponent<BallLogic>().ResetBall();
            Globals.Lives--;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Wall")) return; //if paddle hits map boundaries, immediately stop acceleration to prevent clipping.
            _rigidBody.velocity = Vector2.zero;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Powerup")) return;
            ActivatePowerup(collision.gameObject.GetComponent<PowerupComponent>().PowerupType);
            Destroy(collision.gameObject);

            gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Pickup_04");
        }

        /// <summary>
        /// Activates a powerup that has been picked up by the player.
        /// </summary>
        /// <param name="id">The ID of the powerup that has been collided with.</param>
        private void ActivatePowerup(PowerupCodes id)
        {
            switch (id)
            {
                default:
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
                        _score.UpdateScore(250);
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
            const float shootDelay = 0.3f;
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
            _eventSystem.GetComponent<GUIHelper>().RemovePowerupFromSidebar(PowerupCodes.LaserBeam);
            IsFiring = false;
        }
    }
}
