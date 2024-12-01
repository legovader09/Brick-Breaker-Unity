using System;
using LevelData;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace GUI
{
    public class SpeedIndicator : MonoBehaviour
    {
        GameObject _ball;

        void Start()
        {
            _ball = GameObject.FindGameObjectWithTag("Ball");
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (!Globals.GamePaused)
            {
                GetComponent<Text>().text = $"Speed\n\n{Math.Round(_ball.GetComponent<BallLogic>().speed / 100, 2) }\nm /s"; //on every late update tick (as this is non-priority), display the ball's current velocity.
            }
        }
    }
}
