using System;
using UnityEngine;
using UnityEngine.UI;

public class SpeedIndicator : MonoBehaviour
{
    GameObject ball;

    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!Globals.GamePaused)
        {
            GetComponent<Text>().text = $"Speed\n\n{Math.Round(ball.GetComponent<BallLogic>().speed / 100, 2) }\nm /s"; //on every late update tick (as this is non-priority), display the ball's current velocity.
        }
    }
}
