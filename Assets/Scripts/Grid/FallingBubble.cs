using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Attached to a bubble sprite that lost its connection to the ceiling, so
    /// it visibly falls instead of vanishing like a popped match. Simple
    /// constant-gravity fall, no physics components needed.
    /// </summary>
    public class FallingBubble : MonoBehaviour
    {
        private const float Gravity = 20f;
        private const float LifetimeSeconds = 1.2f;

        private float _velocityY;
        private float _elapsed;

        private void Update()
        {
            _velocityY -= Gravity * Time.deltaTime;
            transform.position += new Vector3(0f, _velocityY * Time.deltaTime, 0f);
            _elapsed += Time.deltaTime;
            if (_elapsed >= LifetimeSeconds) Destroy(gameObject);
        }
    }
}
