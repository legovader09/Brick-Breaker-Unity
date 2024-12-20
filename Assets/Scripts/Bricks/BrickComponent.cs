﻿using System.Collections;
using Enums;
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
        public GameSessionData sessionData;
        private Vector2 _ballVel;
        private Camera _mainCamera;

        // Start is called before the first frame update
        private void Awake()
        {
            _mainCamera = Camera.main;
            gameObject.GetComponent<SpriteRenderer>().sprite = BrickColourHelper.GetBrickColour(colour); //when created, immediately fetch brick texture.
        }

        private void OnCollisionEnter2D(Collision2D ballCollision)
        {
            if (!ballCollision.gameObject.CompareTag("Ball") || sessionData.GamePaused) return;
            state--; //state minus on collision
            _ballVel = ballCollision.gameObject.GetComponent<Rigidbody2D>().linearVelocity; // get velocity of ball on impact
        }
        
        private void OnTriggerEnter2D(Collider2D laserCollider)
        {
            if (!laserCollider.gameObject.CompareTag("LaserBeam") || sessionData.GamePaused) return;
            state--;
            Destroy(laserCollider.gameObject); //destroy laser beam on impact.
            StateCheck(); //set new state of the brick, i.e. set to damaged state, or destroy it if broken.
        }

        private void OnTriggerStay2D(Collider2D explosionCollider)
        {
            if (explosionCollider.gameObject.name == "ExplodeRadius")
            {
                explosionCollider.gameObject.GetComponent<CircleCollider2D>().enabled = false; //disable explosion radius.
                gameObject.AddComponent<Rigidbody2D>().AddRelativeForce(-_ballVel, ForceMode2D.Impulse); //launch the affected bricks forward to simulate an explosion.
                gameObject.GetComponent<Rigidbody2D>().MoveRotation(180f * Time.deltaTime); //give bricks a small amount of rotation.
                state = 1;
                StateCheck(); //make bricks look damaged before finally destroying them.
                StartCoroutine(Explode());
            }

            // TODO: Add magnet as normal powerup
            // if (GameObject.FindGameObjectsWithTag("Brick").Length > 5 || !explosionCollider.CompareTag("Ball")) return;
            // explosionCollider.GetComponent<BallLogic>().HookedByMagnet = true;
            // explosionCollider.GetComponent<BallLogic>().UpdateMagnetic(gameObject); //this enables magnet mode, allows the ball to be physically attracted to the brick object.
        }

        private void OnTriggerExit2D(Collider2D ballCollider)
        {
            if (ballCollider.CompareTag("Ball"))
            {
                ballCollider.GetComponent<BallLogic>().HookedByMagnet = false; //when ball exits the magnet's range, it is no longer affected by magnetism.
            }
        }

        private IEnumerator Explode()
        {
            yield return new WaitForSeconds(sessionData.Random.Next(30, 40) / 100); //wait for a random interval of time.
            state = 0;
            GameObject.Find("EventSystem").GetComponent<GameTracker>().UpdateScore(50); //add score for each exploding brick.
            StateCheck(); // Finally destroy the brick.
        }

        public void StateCheck()
        {
            switch (state)
            {
                case 1:
                    gameObject.GetComponent<SpriteRenderer>().sprite = BrickColourHelper.GetBrickColour(colour + 1); //change to damaged version of the brick texture.
                    break;
                case <= 0:
                    StartCoroutine(PowerupDropCheck());
                    break;
            }
        }

        private IEnumerator PowerupDropCheck()
        {
            var chance = sessionData.Random.Next(0, 100); //random change for powerup to appear.

            gameObject.GetComponent<BoxCollider2D>().isTrigger = true; //trigger allows ball object to go through the brick while the breaking animation plays.
            for (var i = 0; i < 3; i++)
            { // this halves the brick's scale every 0.1 in game seconds, to show a breaking effect, for 3 iterations.
                gameObject.transform.localScale = new(transform.localScale.x / 2, transform.localScale.y / 2, transform.localScale.z);
                yield return new WaitForSeconds(0.1f);
            }
            gameObject.GetComponent<BoxCollider2D>().isTrigger = false; // try to prevent object from going out of bounds by re-adding collision.

            if (chance <= sessionData.chanceToDropPowerup) //if change is within the power up chance rate.
            {
                Instantiate(powerupPrefab, _mainCamera.transform, false)
                    .transform.localPosition = new(gameObject.transform.localPosition.x, 
                        gameObject.transform.localPosition.y, 
                        gameObject.transform.localPosition.z - 1);
            }
            Destroy(gameObject); // destroy brick.
        }
    }
}
