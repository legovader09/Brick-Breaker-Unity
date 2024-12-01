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
        void Awake()
        {
            PowerupCodes p = PowerupCodes.None;
            switch (Globals.Random.NextDouble())
            {
                case double n when (n <= i.Pts50):
                    p = PowerupCodes.Pts50;
                    break;
                case double n when (n > i.Pts50 && n <= i.Pts100):
                    p = PowerupCodes.Pts100;
                    break;
                case double n when (n > i.Pts100 && n <= i.Pts250):
                    p = PowerupCodes.Pts250;
                    break;
                case double n when (n > i.Pts250 && n <= i.Pts500):
                    p = PowerupCodes.Pts500;
                    break;
                case double n when (n > i.Pts500 && n <= i.SlowBall):
                    p = PowerupCodes.SlowBall;
                    break;
                case double n when (n > i.SlowBall && n <= i.FastBall):
                    p = PowerupCodes.FastBall;
                    break;
                case double n when (n > i.FastBall && n <= i.TripleBall):
                    p = PowerupCodes.TripleBall;
                    break;
                case double n when (n > i.TripleBall && n <= i.LifeUp):
                    p = Globals.EndlessMode ? PowerupCodes.Pts100 : PowerupCodes.LifeUp; //endless mode removes hearts powerup as this will drag out the longevity.
                    break;
                case double n when (n > i.LifeUp && n <= i.LaserBeam):
                    p = PowerupCodes.LaserBeam;
                    break;
                case double n when (n > i.LaserBeam && n <= i.GrowPaddle):
                    p = PowerupCodes.GrowPaddle;
                    break;
                case double n when (n > i.GrowPaddle && n <= i.ShrinkPaddle):
                    p = PowerupCodes.ShrinkPaddle;
                    break;
                case double n when (n > i.ShrinkPaddle && n <= i.SafetyNet):
                    p = PowerupCodes.SafetyNet;
                    break;
                case double n when (n > i.SafetyNet && n <= i.DoublePoints):
                    p = PowerupCodes.DoublePoints;
                    break;
                case double n when (n > i.DoublePoints && n <= i.RedFireBall):
                    p = PowerupCodes.RedFireBall;
                    break;
                case double n when (n > i.RedFireBall && n <= i.HalfPoints):
                    p = PowerupCodes.HalfPoints;
                    break;
                default:
                    p = PowerupCodes.Pts50;
                    break;
            }

            SelectPowerupType(p);
            Debug.Log($"Powerup Component \"{Enum.GetName(typeof(PowerupCodes), p)}\" Spawned! Powerup Code: " + PowerupType);
        }

        private void SelectPowerupType(PowerupCodes powerupID)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Powerups/{(int)powerupID}-Powerup");
            PowerupType = powerupID;
        }

        // Update is called once per frame
        void Update()
        {   // on every update tick, the powerup will slowly fall down towards the player.
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
