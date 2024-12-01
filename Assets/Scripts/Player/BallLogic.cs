using System.Collections;
using Bricks;
using EventListeners;
using GUI;
using LevelData;
using Powerups;
using UnityEngine;
using UnityEngine.U2D;

namespace Player
{
    /// <summary>
    /// For everything ball related.
    /// </summary>
    public class BallLogic : MonoBehaviour
    {
        public bool stuckToPlayer = true;
        public GameObject player;
        public GameObject explosionCollider;
        public Vector2 ballVelocity = new(Globals.BallSpeed, Globals.BallSpeed * 1.5f);
        public Vector2 currentVelocity;
        public float safetyYVelocity = 30f;
        public float speed;
        public float minSpeed;
        public float maxSpeed;
        public int ballHitStreak;

        public float levelMultiplier;
        internal bool IsFake = false;
        internal bool IsFireBall;

        public Vector2 initLocation;
        public Vector2 currentLocation;

        private bool _canDoPhysics;
        private Vector2 _pauseVelocity;
        private Color _originalColor;
        private GUIHelper _guiHelper;

        internal bool HookedByMagnet = false;
        internal GameObject MagneticBrick;

        // Start is called before the first frame update
        void Awake()
        {
            //gameObject.transform.position = new Vector2(0, -398);
            initLocation = gameObject.transform.position;
            _guiHelper = GameObject.Find("EventSystem").GetComponent<GUIHelper>();
            _originalColor = gameObject.GetComponent<SpriteShapeRenderer>().color;
            explosionCollider.GetComponent<CircleCollider2D>().enabled = false; //explosion radius collider should be off initially.
            ResetSpeed();
        }


        void FixedUpdate()
        {
            if (Globals.GamePaused) return;
            if (!HookedByMagnet)
            {
                if (_canDoPhysics)
                {
                    currentVelocity = gameObject.GetComponent<Rigidbody2D>().velocity;
                    if (Mathf.FloorToInt(currentVelocity.y) < 20 && Mathf.FloorToInt(currentVelocity.y) > 0) // this ensures ball is not stuck being unreachable by player.
                        gameObject.GetComponent<Rigidbody2D>().velocity = new(currentVelocity.x, safetyYVelocity); //minimum velocity in the Y axis which is configurable.

                    if (Mathf.FloorToInt(currentVelocity.y) > -20 && Mathf.FloorToInt(currentVelocity.y) < 0) // this ensures ball is not stuck being unreachable by player.
                        gameObject.GetComponent<Rigidbody2D>().velocity = new(currentVelocity.x, -safetyYVelocity); //minimum velocity in the (minus)Y axis which is configurable.

                    speed = currentVelocity.magnitude; //measures ball's magnitude.

                    // ensures ball does not exceed, or reach the lower threshold of the min and max speed limit.
                    if (speed < minSpeed) gameObject.GetComponent<Rigidbody2D>().velocity = currentVelocity.normalized * minSpeed;
                    if (speed > maxSpeed) gameObject.GetComponent<Rigidbody2D>().velocity = currentVelocity.normalized * maxSpeed;
                }
                currentLocation = gameObject.transform.position;
            }
            else
            {
                FixedUpdateMagnetic();
            }
        }

        /// <summary>
        /// On every fixed update, if the ball is magnetic, the object will be pulled towards the nearest brick object.
        /// </summary>
        void FixedUpdateMagnetic()
        {
            if (!MagneticBrick) return;
            var maxDistance = 500f + levelMultiplier;
            var distance = Vector3.Distance(gameObject.transform.position, MagneticBrick.transform.position);

            if (!(distance < maxDistance)) return; // Ball is in range of the brick.
            var distanceLerp = Mathf.InverseLerp(maxDistance, 0f, distance); //uses inverselerp function to measure the distance from the brick to the ball.
            var strength = Mathf.Lerp(0f, 1000f, distanceLerp); //uses regular lerp to calculated the force strength, taking into consideration the distance lerp above.

            var getDirection = (MagneticBrick.transform.position - gameObject.transform.position).normalized; //normalised sets the magnetism of the ball's velocity to 1, and from this I can retrieve the general direction ofthe ball

            gameObject.GetComponent<Rigidbody2D>().AddForce(getDirection * strength, ForceMode2D.Force);// apply force to the ball
        }

        /// <summary>
        /// Simply adds the magnetic powerup indicator to the sidebar, and makes the corresponding brick a source of magnet.
        /// </summary>
        /// <param name="brick">The brick that will become the magnet source.</param>
        internal void UpdateMagnetic(GameObject brick)
        {
            _guiHelper.AddPowerupToSidebar(PowerupCodes.Magnetic);
            MagneticBrick = brick;
        }

        // Update is called once per frame
        void Update()
        {
            if (!Globals.GamePaused)
            {
                if (stuckToPlayer) //if attached to player, enable joint component which disallows the ball to move independently.
                {
                    gameObject.GetComponent<FixedJoint2D>().enabled = true;
                    _canDoPhysics = false;
                    currentVelocity = Vector2.zero;
                    speed = 0f;
                }
                else // if not attached to player, the joint will be disabled allowing the ball to move independently again.
                {
                    gameObject.GetComponent<FixedJoint2D>().enabled = false;
                    if (!_canDoPhysics)
                    {
                        var random = Globals.Random.Next(0, 2); //50% chance to go left, or right.
                        if (random == 0) 
                            gameObject.GetComponent<Rigidbody2D>().velocity = ballVelocity; 
                        else 
                            gameObject.GetComponent<Rigidbody2D>().velocity = 
                                new(-ballVelocity.x, ballVelocity.y);

                        _canDoPhysics = true;
                    }
                }
            }
            gameObject.GetComponentInChildren<TrailRenderer>().emitting = (IsFireBall && !IsFake); //sets the fireball trail to emit, if the powerup has been picked up, and the ball is not a duplicate (green ball).
        }

        internal void ResetSpeed()
        {
            minSpeed = 500f + levelMultiplier;
            maxSpeed = 770f + levelMultiplier;
        }

        /// <summary>
        /// Changes ball speed from powerup
        /// </summary>
        /// <param name="state">0 = default, 1 = fast, 2 = slow</param>
        internal IEnumerator ChangeSpeed(int state)
        {
            switch (state)
            {
                case 0:
                    minSpeed = 600f;
                    maxSpeed = 1060f;
                    break;
                case 1:
                    minSpeed = 1100f;
                    maxSpeed = 1560f;
                    break;
                case 2:
                    minSpeed = 240f;
                    maxSpeed = 600f;
                    break;
            }
            minSpeed += levelMultiplier;
            maxSpeed += levelMultiplier;

            if (state == 0) yield break;
            //reset speed back to normal after 5 seconds.
            yield return new WaitForSeconds(2f);

            StartCoroutine(_guiHelper.ShowPowerupExpiring(state == 1 ? PowerupCodes.FastBall : PowerupCodes.SlowBall));
            yield return new WaitForSeconds(3f);

            minSpeed = 500f + levelMultiplier;
            maxSpeed = 770f + levelMultiplier;
            _guiHelper.RemovePowerupFromSidebar(state == 1 ? PowerupCodes.FastBall : PowerupCodes.SlowBall);
        }

        /// <summary>
        /// Removes all relevant powerup indicators that occur when player loses a life.
        /// </summary>
        private void RemovePowerupIndicators()
        {
            _guiHelper.RemovePowerupFromSidebar(PowerupCodes.FastBall);
            _guiHelper.RemovePowerupFromSidebar(PowerupCodes.SlowBall);
            _guiHelper.RemovePowerupFromSidebar(PowerupCodes.GrowPaddle);
            _guiHelper.RemovePowerupFromSidebar(PowerupCodes.ShrinkPaddle);
            _guiHelper.RemovePowerupFromSidebar(PowerupCodes.Magnetic);
        }

        /// <summary>
        /// Temporarily sets the ball velocity to 0, if game is unpaused, this velocity is restored.
        /// </summary>
        internal void PauseBall()
        {
            if (Globals.GamePaused)
            {
                _pauseVelocity = gameObject.GetComponent<Rigidbody2D>().velocity;
                gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
            else
            {
                gameObject.GetComponent<Rigidbody2D>().velocity = _pauseVelocity;
            }
        }

        private void OnBecameInvisible()
        {
            if (!IsFake) //only lose a life if the ball is not a duplicate (green) ball.
            {
                gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/TakeDamage");
                ResetBall();
                Globals.Lives--;
                ResetSpeed();
                RemovePowerupIndicators();
            }
            else
            {
                Destroy(gameObject); //destroy fake ball.
            }
        }

        internal void ResetBall()
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().ResetPaddleSize();
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            stuckToPlayer = true;
            gameObject.transform.position = initLocation;
            player.GetComponent<PlayerController>().ResetPaddlePosition();
            ActivateFireBall(false);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Hit_01");

            if (!collision.gameObject.CompareTag("Brick")) return;
            if (collision.gameObject.GetComponent<BrickComponent>().state == 0) //check state after state has been updated from brick collision
            {
                ballHitStreak++;
                GameObject.Find("EventSystem").GetComponent<GameTracker>().UpdateScore(10 * ballHitStreak);
                GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Explosion_02");
            }
            if (IsFireBall)
            {
                explosionCollider.GetComponent<CircleCollider2D>().enabled = true;
                IsFireBall = false;
                GetComponent<SpriteShapeRenderer>().color = _originalColor;
                GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Explosion_03");
            }
            else
            {
                collision.gameObject.GetComponent<BrickComponent>().StateCheck(); //do necessary brick updates after collision.
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player")) //reset brick hit streak if ball comes in contact with player.
                ballHitStreak = 0;

            if (!IsFake) return; //this forces the paddle to release the main ball upon coming into contact with the green ball. This prevents cheating, so that the player can not simply use the green ball only and not worry about lives.
            if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Ball")) return;
            foreach (var g in GameObject.FindGameObjectsWithTag("Ball"))
                g.GetComponent<BallLogic>().stuckToPlayer = false;
        }

        /// <summary>
        /// Activates the fireball state.
        /// </summary>
        /// <param name="state">Set to true if fireball mode on, false if not.</param>
        internal void ActivateFireBall(bool state)
        {
            if (IsFake) return;

            IsFireBall = state;
            gameObject.GetComponent<SpriteShapeRenderer>().color = state ? Color.red : _originalColor;
        }
    }
}
