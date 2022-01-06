using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// A class for all paddle related events.
/// </summary>
public class PlayerController : MonoBehaviour
{
    private Vector2 mousePos;
    internal bool hasBallAttached = true;
    internal Vector3 originalSize;
    internal Vector3 enlargedPaddle;
    internal Vector3 shrunkPaddle;
    private Vector2 originalPosition;
    private GameObject ball;
    public GameObject LaserBeamPrefab;
    public GameObject arrowAnchor;
    public GameObject powerupHelper;
    GameTracker score;
    GUIHelper PowerupUI;
    public bool cancelFire = false;

    /// <summary>Used to indicate whether the paddle is firing laser beams.</summary> 
    public bool IsFiring { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
        score = GameObject.Find("EventSystem").GetComponent<GameTracker>(); 
        PowerupUI = GameObject.Find("EventSystem").GetComponent<GUIHelper>();
        originalSize = gameObject.transform.localScale;
        enlargedPaddle = new Vector3(gameObject.transform.localScale.x * 1.5f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
        shrunkPaddle = new Vector3(gameObject.transform.localScale.x * 0.75f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
        originalPosition = gameObject.transform.position;
    }

    internal void ResetPaddlePosition()
    {
        gameObject.transform.position = originalPosition;
    }

    internal void ResetPaddleSize()
    {
        gameObject.transform.localScale = originalSize;
        PowerupUI.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.ShrinkPaddle);
        PowerupUI.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.GrowPaddle);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Globals.GamePaused)
        {
            if (Globals.AIMode) // AI  mode is used for TESTING purposes only and can be toggled by pressing T in game.
            {
                if (ball.GetComponent<BallLogic>().currentVelocity.y > 0) // if ball is going upwards
                {
                    GameObject g = GameObject.FindGameObjectWithTag("Powerup"); 
                    if (g != null)
                        mousePos = Camera.main.ScreenToWorldPoint(g.transform.position); //follow powerup if there is one, if not keep following ball.
                    else
                        mousePos = Camera.main.ScreenToWorldPoint(ball.transform.position);
                }
                else
                {
                    mousePos = Camera.main.ScreenToWorldPoint(ball.transform.position); //follow ball
                }
            }
            else
            {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //follow mouse if not AI Mode.
            }
            gameObject.GetComponent<Rigidbody2D>().MovePosition(new Vector2(mousePos.x, 0));
        }
        else gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero; 
    }

    void LateUpdate()
    {
        if (hasBallAttached && !Globals.GamePaused)
        {
            if ((Input.GetMouseButtonDown(0) && Input.mousePosition.y < 850) || Globals.AIMode)
            {
                BallLogic b = ball.GetComponent<BallLogic>();
                b.stuckToPlayer = false;
                hasBallAttached = false;
            }
        }

        if (ball.GetComponent<BallLogic>().stuckToPlayer) hasBallAttached = true;

#if UNITY_EDITOR //editor ONLY, AI mode will not work in the actual game. This mode is for testing purposes only.
        if (Input.GetKeyUp(KeyCode.T))
            Globals.AIMode = !Globals.AIMode;
#endif
        if (Input.GetKeyUp(KeyCode.R)) //if the ball is stuck, or extremely slow, the player can press the R key on the keyboard to reset the ball, this will cost one player life.
        {
            ball.GetComponent<BallLogic>().ResetBall();
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
            ActivatePowerup(collision.gameObject.GetComponent<PowerupComponent>().powerupType);
            Destroy(collision.gameObject);

            gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Pickup_04");
        }
    }

    /// <summary>
    /// Activates a powerup that has been picked up by the player.
    /// </summary>
    /// <param name="ID">The ID of the powerup that has been collided with.</param>
    internal void ActivatePowerup(PowerupComponent.PowerupCodes ID)
    {
        switch ((int)ID)
        {
            case 1:
                score.UpdateScore(50);
                break;
            case 2:
                score.UpdateScore(100);
                break;
            case 3:
                score.UpdateScore(250);
                break;
            case 4:
                score.UpdateScore(500);
                break;
            case 5: //Slow ball
                IEnumerator slowCall = ball.GetComponent<BallLogic>().ChangeSpeed(2); //setting the function to a IEnumerator variable allows me to stop the coroutine and restart it.
                StopCoroutine(slowCall);
                StartCoroutine(slowCall);
                score.UpdateScore(20);
                PowerupUI.AddPowerupToSidebar(ID);
                PowerupUI.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.FastBall);
                break;
            case 6: //Fast ball
                IEnumerator speedCall = ball.GetComponent<BallLogic>().ChangeSpeed(1); //setting the function to a IEnumerator variable allows me to stop the coroutine and restart it.
                StopCoroutine(speedCall);
                StartCoroutine(speedCall);
                score.UpdateScore(40);
                PowerupUI.AddPowerupToSidebar(ID);
                PowerupUI.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.SlowBall);
                break;
            case 7: //Triple balls
                for (int i = 0; i < 2; i++)
                {
                    hasBallAttached = false;
                    GameObject temp = Instantiate(ball, ball.transform.parent, false);
                    temp.GetComponent<BallLogic>().isFake = true;
                    temp.GetComponent<SpriteShapeRenderer>().color = Color.green;
                    GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
                    foreach (GameObject b in balls)
                    {
                        b.GetComponent<BallLogic>().stuckToPlayer = false;
                    }
                }
                score.UpdateScore(50);
                break;
            case 8: //Shrink paddle
                gameObject.transform.localScale = shrunkPaddle;
                score.UpdateScore(50);
                PowerupUI.AddPowerupToSidebar(ID);
                PowerupUI.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.GrowPaddle);
                break;
            case 9: //Enlarge paddle
                gameObject.transform.localScale = enlargedPaddle;
                score.UpdateScore(20);
                PowerupUI.AddPowerupToSidebar(ID);
                PowerupUI.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.ShrinkPaddle);
                break;
            case 10: //Laser Beam
                StartCoroutine(FireBeams());
                PowerupUI.AddPowerupToSidebar(ID);
                score.UpdateScore(50);
                break;
            case 11: //Life Up
                if (Globals.Lives < 6)
                    Globals.Lives++;
                else
                    score.UpdateScore(100);
                break;
            case 12: // Safety Net
                Instantiate(powerupHelper, gameObject.transform).GetComponent<PowerupHelper>().SetSafetyNet();
                PowerupUI.AddPowerupToSidebar(ID);
                score.UpdateScore(50);
                break;
            case 13: // Double points
                Instantiate(powerupHelper, gameObject.transform).GetComponent<PowerupHelper>().SetMultiplier(2f);
                PowerupUI.AddPowerupToSidebar(ID);
                PowerupUI.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.HalfPoints);
                break;
            case 14: // Red fireball.
                ball.GetComponent<BallLogic>().ActivateFireBall(true);
                score.UpdateScore(50);
                break;
            case 15: //Half points
                Instantiate(powerupHelper, gameObject.transform).GetComponent<PowerupHelper>().SetMultiplier(0.5f);
                PowerupUI.AddPowerupToSidebar(ID);
                PowerupUI.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.DoublePoints);
                break;
        }
    }

    private IEnumerator FireBeams()
    {
        float shootDelay = 0.3f;
        int shotsFired = 0;
        while (shotsFired < 5) //shoot a total of 6 shots.
        {
            IsFiring = true;
            if (!Globals.GamePaused)
            {
                Instantiate(LaserBeamPrefab, 
                    GameObject.FindGameObjectWithTag("MainCamera").transform, false).transform.position = 
                    GameObject.FindGameObjectWithTag("PlayerBeamPos1").transform.position;

                gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/8Bit/Shoot_01");

                yield return new WaitForSeconds(shootDelay);
                shotsFired++;

                Instantiate(LaserBeamPrefab, 
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
        GameObject.Find("EventSystem").GetComponent<GUIHelper>().RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.LaserBeam);
        IsFiring = false;
    }
}
