using LevelData;
using UnityEngine;

namespace Powerups
{
    public class LaserBeamComponent : MonoBehaviour
    {
        public float speed;
        
        private void FixedUpdate()
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = new(0, Globals.GamePaused ? 0 : speed);
        }
    }
}
