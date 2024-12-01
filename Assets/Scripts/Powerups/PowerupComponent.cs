using System;
using LevelData;
using UnityEngine;
using i = Powerups.ItemSpawnChance;

namespace Powerups
{
    /// <summary>
    /// This component handles a spawned powerup's data.
    /// </summary>
    public partial class PowerupComponent : MonoBehaviour
    {
        public float fallSpeed;
        internal PowerupCodes PowerupType;

        // Start is called before the first frame update
        private void Awake()
        {
            var powerupCodes = Globals.Random.NextDouble() switch
            {
                <= i.Pts50 => PowerupCodes.Pts50,
                <= i.Pts100 and > i.Pts50 => PowerupCodes.Pts100,
                <= i.Pts250 and > i.Pts100 => PowerupCodes.Pts250,
                <= i.Pts500 and > i.Pts250 => PowerupCodes.Pts500,
                <= i.SlowBall and > i.Pts500 => PowerupCodes.SlowBall,
                <= i.FastBall and > i.SlowBall => PowerupCodes.FastBall,
                <= i.TripleBall and > i.FastBall => PowerupCodes.TripleBall,
                <= i.LifeUp and > i.TripleBall => PowerupCodes.LifeUp,
                <= i.LaserBeam and > i.LifeUp => PowerupCodes.LaserBeam,
                <= i.GrowPaddle and > i.LaserBeam => PowerupCodes.GrowPaddle,
                <= i.ShrinkPaddle and > i.GrowPaddle => PowerupCodes.ShrinkPaddle,
                <= i.SafetyNet and > i.ShrinkPaddle => PowerupCodes.SafetyNet,
                <= i.DoublePoints and > i.SafetyNet => PowerupCodes.DoublePoints,
                <= i.RedFireBall and > i.DoublePoints => PowerupCodes.RedFireBall,
                <= i.HalfPoints and > i.RedFireBall => PowerupCodes.HalfPoints,
                _ => PowerupCodes.Pts50
            };

            SelectPowerupType(powerupCodes);
            Debug.Log($"Powerup Component \"{Enum.GetName(typeof(PowerupCodes), powerupCodes)}\" Spawned! Powerup Code: " + PowerupType);
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
