using System;
using System.Collections;
using Enums;
using EventListeners;
using GUI;
using LevelData;
using Powerups;
using UnityEngine;
using UnityEngine.U2D;

namespace Player
{
    /// <summary>
    /// A class for all paddle related events.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public GameSessionData sessionData;
        public GameObject laserBeamPrefab;
        public GameObject paddleBase;
        public bool cancelFire;
        public Transform laserBeamPos1;
        public Transform laserBeamPos2;
        private Vector2 _mousePos;
        private bool _hasBallAttached = true;
        private Vector2 _originalPosition;
        private GameObject _ball;
        private GameTracker _gameTracker;
        private GUIHelper _powerupUI;
        private Rigidbody2D _rigidBody;
        private Camera _mainCamera;
        private GameObject _eventSystem;
        private Vector2 _mouseTargetPosition;
        private PowerupHelper _powerupHelper;
        private Coroutine _safetyNetTracker;

        /// <summary>Used to indicate whether the paddle is firing laser beams.</summary> 
        public bool IsFiring { get; private set; }

        // Start is called before the first frame update
        private void Start()
        {
            _eventSystem = GameObject.Find("EventSystem");
            _ball = GameObject.FindGameObjectWithTag("Ball");
            _gameTracker = _eventSystem.GetComponent<GameTracker>(); 
            _powerupUI = _eventSystem.GetComponent<GUIHelper>();
            _originalPosition = gameObject.transform.position;
            _rigidBody = gameObject.GetComponent<Rigidbody2D>();
            _powerupHelper = _eventSystem.GetComponent<PowerupHelper>();
            _mainCamera = Camera.main;
        }

        internal void ResetPaddlePosition()
        {
            gameObject.transform.position = _originalPosition;
        }

        internal void ResetPaddleSize()
        {
            paddleBase.GetComponent<PaddleSynchronizer>().SetPaddleSize(1);
            _powerupUI.RemovePowerupFromSidebar(PowerupCodes.ShrinkPaddle);
            _powerupUI.RemovePowerupFromSidebar(PowerupCodes.GrowPaddle);
        }

        private void FixedUpdate()
        {
            if (sessionData.GamePaused)
            {
                _rigidBody.linearVelocity = Vector2.zero;
                return;
            }

            if (sessionData.AIMode) 
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
            if (_hasBallAttached && !sessionData.GamePaused)
            {
                if ((Input.GetMouseButtonDown(0) && Input.mousePosition.y < 850) || sessionData.AIMode)
                {
                    var ball = _ball.GetComponent<BallLogic>();
                    ball.stuckToPlayer = false;
                    _hasBallAttached = false;
                }
            }
            
            if (!sessionData.GamePaused && !sessionData.AIMode)
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
                sessionData.AIMode = !sessionData.AIMode;
#endif
            if (!Input.GetKeyUp(KeyCode.R)) return;
            _ball.GetComponent<BallLogic>().ResetBall();
            sessionData.lives--;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Wall")) return; //if paddle hits map boundaries, immediately stop acceleration to prevent clipping.
            _rigidBody.linearVelocity = Vector2.zero;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var component = collision.gameObject;
            if (!component.CompareTag("Powerup")) return;
            var powerup = collision.gameObject.GetComponent<PowerupComponent>();
            if (powerup == null || powerup.IsActivated) return;
            powerup.IsActivated = true;
            
            ActivatePowerup(powerup.PowerupType);
            Destroy(component);

            gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Pickup_04");
        }

        /// <summary>
        /// Activates a powerup that has been picked up by the player.
        /// </summary>
        /// <param name="id">The ID of the powerup that has been collided with.</param>
        private void ActivatePowerup(PowerupCodes id)
        {
            var paddleSynchronizer = paddleBase.GetComponent<PaddleSynchronizer>();
            switch (id)
            {
                default:
                case PowerupCodes.Pts50:
                    _gameTracker.UpdateScore(50);
                    break;
                case PowerupCodes.Pts100:
                    _gameTracker.UpdateScore(100);
                    break;
                case PowerupCodes.Pts250:
                    _gameTracker.UpdateScore(250);
                    break;
                case PowerupCodes.Pts500:
                    _gameTracker.UpdateScore(500);
                    break;
                case PowerupCodes.SlowBall:
                    _powerupHelper.ActivatePowerup(id, 10f, () => _ball.GetComponent<BallLogic>().ChangeSpeed(2), () => _ball.GetComponent<BallLogic>().ChangeSpeed(0));
                    _gameTracker.UpdateScore(20);
                    _powerupUI.RemovePowerupFromSidebar(PowerupCodes.FastBall);
                    break;
                case PowerupCodes.FastBall:
                    _powerupHelper.ActivatePowerup(id, 10f, () => _ball.GetComponent<BallLogic>().ChangeSpeed(1), () => _ball.GetComponent<BallLogic>().ChangeSpeed(0));
                    _gameTracker.UpdateScore(40);
                    _powerupUI.RemovePowerupFromSidebar(PowerupCodes.SlowBall);
                    break;
                case PowerupCodes.TripleBall:
                    _hasBallAttached = false;
                    _gameTracker.UpdateScore(50);
                    var ballCollection = GameObject.FindGameObjectsWithTag("Ball");
                    if (ballCollection.Length > 15)
                    {
                        // reward the player for somehow juggling 15 balls.
                        _gameTracker.UpdateScore(1000);
                    }
                    else
                    {
                        var colorSequence = new [] { Color.green, Color.yellow, new Color(1.0f, 0.65f, 0.0f) };
                        var currentColorIndex = -1;
                        foreach (var ball in ballCollection)
                        {
                            var sprite = ball.GetComponent<SpriteShapeRenderer>();
                            for (var index = 0; index < colorSequence.Length; index++)
                            {
                                if (!sprite.color.Equals(colorSequence[index])) continue;
                                currentColorIndex = index;
                                break;
                            }
                            
                            for (var i = 0; i < 2; i++)
                            {
                                var instance = Instantiate(ball, ball.transform.parent, false);
                                instance.GetComponent<BallLogic>().IsFake = true;
                                instance.GetComponent<SpriteShapeRenderer>().color = colorSequence[(currentColorIndex + 1) % colorSequence.Length];
                                instance.GetComponent<BallLogic>().stuckToPlayer = false;
                            }
                        }
                    }
                    break;
                case PowerupCodes.ShrinkPaddle:
                    if (_powerupUI.CheckIfPowerupExists(PowerupCodes.GrowPaddle))
                    {
                        paddleSynchronizer.SetPaddleSize(1);
                        _powerupUI.RemovePowerupFromSidebar(PowerupCodes.GrowPaddle);
                    }
                    paddleSynchronizer.ModifyPaddleSize(.80f);
                    _gameTracker.UpdateScore(50);
                    _powerupUI.AddPowerupToSidebar(id);
                    break;
                case PowerupCodes.GrowPaddle:
                    if (_powerupUI.CheckIfPowerupExists(PowerupCodes.ShrinkPaddle))
                    {
                        paddleSynchronizer.SetPaddleSize(1);
                        _powerupUI.RemovePowerupFromSidebar(PowerupCodes.ShrinkPaddle);
                    }
                    paddleSynchronizer.ModifyPaddleSize(1.2f);
                    _gameTracker.UpdateScore(20);
                    _powerupUI.AddPowerupToSidebar(id);
                    break;
                case PowerupCodes.LaserBeam:
                    StartCoroutine(FireBeams());
                    _powerupUI.AddPowerupToSidebar(id);
                    _gameTracker.UpdateScore(50);
                    break;
                case PowerupCodes.LifeUp:
                    if (sessionData.lives < 6)
                        sessionData.lives++;
                    else
                        _gameTracker.UpdateScore(250);
                    break;
                case PowerupCodes.SafetyNet:
                    var image = _gameTracker.safetyNet.GetComponent<SpriteShapeRenderer>();
                    if (_safetyNetTracker != null)
                    {
                        StopCoroutine(_safetyNetTracker);
                        image.color = new (image.color.r, image.color.g, image.color.b, 1f);
                    }
                    _powerupHelper.ActivatePowerup(id, 10f, 
                        () => _gameTracker.safetyNet.SetActive(true), 
                        () => _gameTracker.safetyNet.SetActive(false),
                        () =>
                        {
                            _safetyNetTracker = StartCoroutine(Flicker());
                            return;

                            IEnumerator Flicker()
                            {
                                const int blinkCount = 12;
                                const float halfBlinkDuration = 0.25f;

                                for (var i = 0; i < blinkCount; i++)
                                {
                                    image.color = new (image.color.r, image.color.g, image.color.b, i % 2 == 0 ? 0.8f : 1f);
                                    yield return new WaitForSeconds(halfBlinkDuration);
                                }
                            }
                        });
                    _gameTracker.UpdateScore(50);
                    break;
                case PowerupCodes.DoublePoints:
                    _powerupHelper.RemovePowerup(PowerupCodes.HalfPoints);
                    _powerupHelper.ActivatePowerup(id, 10f, () => sessionData.scoreMultiplier = 2f, () => sessionData.scoreMultiplier = 1f);
                    break;
                case PowerupCodes.RedFireBall:
                    _ball.GetComponent<BallLogic>().ActivateFireBall(true);
                    _gameTracker.UpdateScore(50);
                    break;
                case PowerupCodes.HalfPoints:
                    _powerupHelper.RemovePowerup(PowerupCodes.DoublePoints);
                    _powerupHelper.ActivatePowerup(id, 10f, () => sessionData.scoreMultiplier = .5f, () => sessionData.scoreMultiplier = 1f);
                    break;
                case PowerupCodes.Magnetic:
                    // TODO: Add magnetic powerup functions here
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
                if (!sessionData.GamePaused)
                {
                    Instantiate(laserBeamPrefab, _mainCamera.transform).transform.position = laserBeamPos1.position;

                    gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Shoot_01");

                    yield return new WaitForSeconds(shootDelay);
                    shotsFired++;

                    Instantiate(laserBeamPrefab, _mainCamera.transform).transform.position = laserBeamPos2.transform.position;

                    gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Shoot_01");

                    yield return new WaitForSeconds(shootDelay);
                    shotsFired++;
                }
                yield return new WaitUntil(() => sessionData.GamePaused == false); //this line is iterated through if the game is running, but should the game be paused, the thread will come back tot his line again until the game is unpaused again.
                if (!cancelFire) continue; //if after the pause check, the firing has been cancelled, exit the loop immediately.
                cancelFire = false;
                break;
            }
            _eventSystem.GetComponent<GUIHelper>().RemovePowerupFromSidebar(PowerupCodes.LaserBeam);
            IsFiring = false;
        }
    }
}
