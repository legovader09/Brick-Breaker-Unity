using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamComponent : MonoBehaviour
{
    public float speed;

    // Update is called once per frame
    void FixedUpdate()
    { // the only purpose of this gameobject is to move upwards towards the bricks.
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, Globals.GamePaused ? 0 : speed);
    }
}
