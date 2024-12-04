using System;
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
        public bool IsActivated { get; set; }
        public float fallSpeed;
        internal PowerupCodes PowerupType;

        // Start is called before the first frame update
        private void Awake()
        {
            var powerupCode = Globals.Random.NextDouble() switch
            {
                <= ItemSpawnChance.Pts50 => PowerupCodes.Pts50,
                <= ItemSpawnChance.Pts100 and > ItemSpawnChance.Pts50 => PowerupCodes.Pts100,
                <= ItemSpawnChance.Pts250 and > ItemSpawnChance.Pts100 => PowerupCodes.Pts250,
                <= ItemSpawnChance.Pts500 and > ItemSpawnChance.Pts250 => PowerupCodes.Pts500,
                <= ItemSpawnChance.SlowBall and > ItemSpawnChance.Pts500 => PowerupCodes.SlowBall,
                <= ItemSpawnChance.FastBall and > ItemSpawnChance.SlowBall => PowerupCodes.FastBall,
                <= ItemSpawnChance.TripleBall and > ItemSpawnChance.FastBall => PowerupCodes.TripleBall,
                <= ItemSpawnChance.LifeUp and > ItemSpawnChance.TripleBall => PowerupCodes.LifeUp,
                <= ItemSpawnChance.LaserBeam and > ItemSpawnChance.LifeUp => PowerupCodes.LaserBeam,
                <= ItemSpawnChance.GrowPaddle and > ItemSpawnChance.LaserBeam => PowerupCodes.GrowPaddle,
                <= ItemSpawnChance.ShrinkPaddle and > ItemSpawnChance.GrowPaddle => PowerupCodes.ShrinkPaddle,
                <= ItemSpawnChance.SafetyNet and > ItemSpawnChance.ShrinkPaddle => PowerupCodes.SafetyNet,
                <= ItemSpawnChance.DoublePoints and > ItemSpawnChance.SafetyNet => PowerupCodes.DoublePoints,
                <= ItemSpawnChance.RedFireBall and > ItemSpawnChance.DoublePoints => PowerupCodes.RedFireBall,
                <= ItemSpawnChance.HalfPoints and > ItemSpawnChance.RedFireBall => PowerupCodes.HalfPoints,
                _ => PowerupCodes.Pts50
            };

            SelectPowerupType(powerupCode);
            Debug.Log($"Powerup Component \"{Enum.GetName(typeof(PowerupCodes), powerupCode)}\" Spawned! Powerup Code: " + PowerupType);
        }

        private void SelectPowerupType(PowerupCodes powerupID)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Powerups/{(int)powerupID}-Powerup");
            PowerupType = powerupID;
        }

        private void Update()
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = new(0, Globals.GamePaused ? 0 : fallSpeed);
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
