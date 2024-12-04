using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Enums;
using LevelData;
using UnityEngine;

namespace Powerups
{
    /// <summary>
    /// This component handles a spawned powerup's data.
    /// </summary>
    public class PowerupComponent : MonoBehaviour
    {
        public GameSessionData sessionData;
        public bool IsActivated { get; set; }
        public float fallSpeed;
        internal PowerupCodes PowerupType;

        // Start is called before the first frame update
        private void Awake()
        {
            var powerupCode = GetRandomWeightedPowerup();
            SelectPowerupType(powerupCode);
            Debug.Log($"Powerup Component \"{Enum.GetName(typeof(PowerupCodes), powerupCode)}\" Spawned! Powerup Code: " + PowerupType);
        }
        
        private PowerupCodes GetRandomWeightedPowerup()
        {
            var totalWeight = PowerupSpawnWeights.PowerupWeights.Values.Sum();
            var randomValue = sessionData.Random.NextDouble() * totalWeight;

            var cumulativeWeight = 0.0;
            foreach (var pair in PowerupSpawnWeights.PowerupWeights)
            {
                cumulativeWeight += pair.Value;
                if (randomValue < cumulativeWeight)
                {
                    return pair.Key;
                }
            }

            return PowerupSpawnWeights.PowerupWeights.Keys.Last();
        }

        private void SelectPowerupType(PowerupCodes powerupID)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Powerups/{(int)powerupID}-Powerup");
            PowerupType = powerupID;
        }

        private void Update()
        {
            gameObject.GetComponent<Rigidbody2D>().linearVelocity = new(0, sessionData.GamePaused ? 0 : fallSpeed);
        }
        
        /// <summary>
        /// Destroy the object if it reaches out of bounds.
        /// </summary>
        private void OnBecameInvisible()
        {
            Destroy(gameObject);
        }
    }
}
