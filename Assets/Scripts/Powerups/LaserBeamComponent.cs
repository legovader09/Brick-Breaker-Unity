using LevelData;
using UnityEngine;

namespace Powerups
{
    public class LaserBeamComponent : MonoBehaviour
    {
        public float speed;
        public GameSessionData sessionData;
        
        private void FixedUpdate()
        {
            gameObject.GetComponent<Rigidbody2D>().linearVelocity = new(0, sessionData.GamePaused ? 0 : speed);
        }
    }
}
