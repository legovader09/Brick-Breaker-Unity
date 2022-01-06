using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i = ItemSpawnChance;

/// <summary>
/// This component handles a spawned powerup's data.
/// </summary>
public partial class PowerupComponent : MonoBehaviour
{
    public float fallSpeed;
    internal PowerupCodes powerupType;

    // Start is called before the first frame update
    void Awake()
    {
        PowerupCodes p = PowerupCodes.None;
        switch (Globals.RNG.NextDouble())
        {
            case double n when (n <= i.Pts_50):
                p = PowerupCodes.Pts_50;
                break;
            case double n when (n > i.Pts_50 && n <= i.Pts_100):
                p = PowerupCodes.Pts_100;
                break;
            case double n when (n > i.Pts_100 && n <= i.Pts_250):
                p = PowerupCodes.Pts_250;
                break;
            case double n when (n > i.Pts_250 && n <= i.Pts_500):
                p = PowerupCodes.Pts_500;
                break;
            case double n when (n > i.Pts_500 && n <= i.SlowBall):
                p = PowerupCodes.SlowBall;
                break;
            case double n when (n > i.SlowBall && n <= i.FastBall):
                p = PowerupCodes.FastBall;
                break;
            case double n when (n > i.FastBall && n <= i.TripleBall):
                p = PowerupCodes.TripleBall;
                break;
            case double n when (n > i.TripleBall && n <= i.LifeUp):
                p = Globals.EndlessMode ? PowerupCodes.Pts_100 : PowerupCodes.LifeUp; //endless mode removes hearts powerup as this will drag out the longevity.
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
                p = PowerupCodes.Pts_50;
                break;
        }

        SelectPowerupType(p);
        Debug.Log($"Powerup Component \"{Enum.GetName(typeof(PowerupCodes), p)}\" Spawned! Powerup Code: " + powerupType);
    }

    private void SelectPowerupType(PowerupCodes powerup_ID)
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Powerups/{(int)powerup_ID}-Powerup");
        powerupType = powerup_ID;
    }

    // Update is called once per frame
    void Update()
    {   // on every update tick, the powerup will slowly fall down towards the player.
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, Globals.GamePaused ? 0 : fallSpeed);
    }


    /// <summary>
    /// Destroy the object if it reaches out of bounds.
    /// </summary>
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
