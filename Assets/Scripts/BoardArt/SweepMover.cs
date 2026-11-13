using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>Slides a world sprite right at a constant speed until it passes an end x, then removes it (the row-clear sweep head).</summary>
    public class SweepMover : MonoBehaviour
    {
        private float _speed;
        private float _endX;

        public static void Launch(GameObject target, float speed, float endX)
        {
            var mover = target.AddComponent<SweepMover>();
            mover._speed = speed;
            mover._endX = endX;
        }

        private void Update()
        {
            transform.position += Vector3.right * (_speed * Time.deltaTime);
            if (transform.position.x > _endX) Destroy(gameObject);
        }
    }
}
