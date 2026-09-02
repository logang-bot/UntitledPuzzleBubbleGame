using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Hand-rolled camera jitter for the ceiling-push warning, matching
    /// FallingBubble's plain Update()-driven style (no coroutines, no tweening
    /// library). Shakes the camera rather than GameBoard.transform so it never
    /// perturbs live shot-aiming/collision math, which is anchored to the
    /// board, not the camera. See docs/features/core-gameplay/shot-timer-and-ceiling-descent.md.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.08f;

        private Vector3 _basePosition;
        private bool _isShaking;

        public void StartShaking()
        {
            _basePosition = transform.position;
            _isShaking = true;
        }

        public void StopShaking()
        {
            _isShaking = false;
            transform.position = _basePosition;
        }

        private void Update()
        {
            if (!_isShaking) return;
            var offset = new Vector3(Random.Range(-amplitude, amplitude), Random.Range(-amplitude, amplitude), 0f);
            transform.position = _basePosition + offset;
        }
    }
}
