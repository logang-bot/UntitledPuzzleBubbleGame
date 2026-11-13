using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// Follows a board camera's position but never its rotation, so sprites
    /// placed under it line up with the board the same way GameBoard does
    /// (position and size only; Battle Mode's 180° top camera just renders
    /// the whole half turned). Camera shake moves them along with the view.
    /// </summary>
    public class ViewAnchor : MonoBehaviour
    {
        public float OrthographicSize => _camera.orthographicSize;

        private Camera _camera;

        public static ViewAnchor Create(Camera camera, Transform parent)
        {
            var anchor = new GameObject("ViewAnchor").AddComponent<ViewAnchor>();
            anchor.transform.SetParent(parent, worldPositionStays: false);
            anchor._camera = camera;
            anchor.LateUpdate();
            return anchor;
        }

        private void LateUpdate()
        {
            var position = _camera.transform.position;
            transform.position = new Vector3(position.x, position.y, 0f);
        }
    }
}
