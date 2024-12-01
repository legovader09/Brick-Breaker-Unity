using System.Collections;
using LevelData;
using Player;
using UnityEngine;

namespace Bricks
{
    public class BrickComponent : MonoBehaviour
    {
        public BrickColours colour;
        public int state = 2; //2 is full brick, 1 is damaged brick, 0 is broken.
        public GameObject powerupPrefab;
        private Vector2 _ballVel;

        // Start is called before the first frame update
        void Awake()
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = BrickColour.GetBrickColour(colour); //when created, immediately fetch brick texture.
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ball") && !Globals.GamePaused)
            {
                state--; //state minus on collision
                _ballVel = collision.gameObject.GetComponent<Rigidbody2D>().velocity; // get velocity of ball on impact
            }
        }
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("LaserBeam") && !Globals.GamePaused)
            {
                state--;
                Destroy(collider.gameObject); //destroy laser beam on impact.
                StateCheck(); //set new state of the brick, i.e. set to damaged state, or destroy it if broken.
            }
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.gameObject.name == "ExplodeRadius")
            {
                collider.gameObject.GetComponent<CircleCollider2D>().enabled = false; //disable explosion radius.
                gameObject.AddComponent<Rigidbody2D>().AddRelativeForce(-_ballVel, ForceMode2D.Impulse); //launch the affected bricks forward to simulate an explosion.
                gameObject.GetComponent<Rigidbody2D>().MoveRotation(180f * Time.deltaTime); //give bricks a small amount of rotation.
                state = 1;
                StateCheck(); //make bricks look damaged before finally destroying them.
                StartCoroutine(Explode());
            }
            if (GameObject.FindGameObjectsWithTag("Brick").Length <= 5 && collider.CompareTag("Ball"))
            {
                collider.GetComponent<BallLogic>().HookedByMagnet = true;
                collider.GetComponent<BallLogic>().UpdateMagnetic(gameObject); //this enables magnet mode, allows the ball to be physically attracted to the brick object.
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.CompareTag("Ball"))
            {
                collider.GetComponent<BallLogic>().HookedByMagnet = false; //when ball exits the magnet's range, it is no longer affected by magnetism.
            }
        }

        IEnumerator Explode()
        {
            yield return new WaitForSeconds((Globals.Random.Next(30, 40) / 100)); //wait for a random interval of time.
            state = 0;
            GameObject.Find("EventSystem").GetComponent<GameTracker>().UpdateScore(50); //add score for each exploding brick.
            StateCheck(); // Finally destroy the brick.
        }

        public void StateCheck()
        {
            if (state == 1)
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = BrickColour.GetBrickColour(colour + 1); //change to damaged version of the brick texture.
            }
            else if (state <= 0)
            {
                StartCoroutine(PowerupDropCheck());
            }
        }

        private IEnumerator PowerupDropCheck()
        {
            int chance = Globals.Random.Next(0, 100); //random change for powerup to appear.

            gameObject.GetComponent<BoxCollider2D>().isTrigger = true; //trigger allows ball object to go through the brick while the breaking animation plays.
            for (int i = 0; i < 3; i++)
            { // this halves the brick's scale every 0.1 in game seconds, to show a breaking effect, for 3 iterations.
                gameObject.transform.localScale = new(transform.localScale.x / 2, transform.localScale.y / 2, transform.localScale.z);
                yield return new WaitForSeconds(0.1f);
            }
            gameObject.GetComponent<BoxCollider2D>().isTrigger = false; // try to prevent object from going out of bounds by re-adding collision.

            if (chance <= Globals.ChanceToDropPowerup) //if change is within the power up chance rate.
            {
                GameObject tempPower = powerupPrefab;
                Instantiate(tempPower,  // create new powerup.
                        GameObject.FindGameObjectWithTag("MainCamera").transform, false).transform.localPosition = 
                    new(gameObject.transform.localPosition.x, 
                        gameObject.transform.localPosition.y, 
                        gameObject.transform.localPosition.z - 1);
            }
            Destroy(gameObject); // destroy brick.
        }
    }
}
