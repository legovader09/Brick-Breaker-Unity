using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionListener : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("LaserBeam"))
        {
            Destroy(collider.gameObject);
        }
    }
}
