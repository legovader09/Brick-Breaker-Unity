using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointsIndicator : MonoBehaviour
{
    private bool canMoveUp = false;
    private Vector3 initScale;
    // Start is called before the first frame update
    private void Awake()
    {
        initScale = gameObject.transform.localScale;
        CheckScale();
    }

    /// <summary>
    /// This function resets the points indicator scaling depending on if the paddle is enlarged, or shrunk, to ensure readability.
    /// </summary>
    private void CheckScale()
    {
        PlayerController p = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (p.gameObject.transform.localScale == p.enlargedPaddle)
            gameObject.transform.localScale = new Vector3(initScale.x / 1.5f, initScale.y, initScale.z);
        else if (p.gameObject.transform.localScale == p.shrunkPaddle)
            gameObject.transform.localScale = new Vector3(initScale.x / 0.75f, initScale.y, initScale.z);
    }

    internal void ShowPoints(int amount)
    {
        gameObject.GetComponentInChildren<Text>().text = $"+{amount}";
        StopCoroutine(DoAnimation());
        StartCoroutine(DoAnimation());
    }

    private IEnumerator DoAnimation()
    {
        canMoveUp = true; //allows transform to occur in update method.
        yield return new WaitForSeconds(1f); //allow floating up for an ingame second.
        canMoveUp = false;
        Destroy(gameObject);
    }

    void Update()
    {
        if (canMoveUp)
        { //move object upwards from the player.
            gameObject.transform.localPosition = new Vector2(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y + 10f * Time.deltaTime);
            CheckScale(); //checking scale every update tick in case the player has picked up another shrink or enlarge powerup.
        }
    }
}
