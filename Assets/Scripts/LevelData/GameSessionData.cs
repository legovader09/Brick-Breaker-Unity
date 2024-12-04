﻿using System;
using Networking;
using UnityEngine;

namespace LevelData
{
    [Serializable, CreateAssetMenu(fileName = "Game Session Data", menuName = "Game Session Data")]
    public class GameSessionData : ScriptableObject
    {
        [Range(1, 99)]
        public int amountOfLevels = 7;
        [Range(1, 100)]
        public int chanceToDropPowerup = 40;
        [Range(100, 9999)] 
        public float ballSpeed = 200f;
        [Range(0, 6)] 
        public int lives = 3;
        [Range(0.1f, 10f)]
        public float scoreMultiplier = 1f;
        public bool startWithEndless;
        public bool endlessMode;
        public bool customLevel;
        internal int CurrentLevel;
        internal string CustomLevelData;
        internal string EndlessLevelData;
        internal System.Random Random = new();
        internal Discord.Discord Discord;
        internal PlayerIOConnection Connection;
        internal string ErrorMessage;
        internal bool AIMode;
        internal int Score;
        internal bool GamePaused;
        internal int BricksRemaining;

        public void Initialize(Action<bool> callback)
        {
            Connection = new();
            callback(true);
        }
    }
}