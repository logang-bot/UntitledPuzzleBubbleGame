using System;
using System.Collections.Generic;
using Game.Grid;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Fixed-speed rotating gun (arcade Puzzle Bobble-style): holding the
    /// rotate zones turns the aim angle at a constant rate; the fire zone
    /// raises OnFireRequested with no subscriber yet (Milestone 3 hooks in
    /// there). See docs/features/core-gameplay/shooter-and-trajectory.md.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class ShooterController : MonoBehaviour
    {
        [SerializeField] private int cols = 8;
        [SerializeField] private float cellWidth = 1f;
        [SerializeField] private float maxAimAngleDegrees = 60f;
        [SerializeField] private float rotateSpeedDegreesPerSecond = 90f;
        [SerializeField] private int maxBounces = 10;
        [SerializeField] private float lineWidth = 0.08f;
        [SerializeField] private Material lineMaterial;
        [SerializeField] private HoldInputZone rotateLeftZone;
        [SerializeField] private HoldInputZone rotateRightZone;
        [SerializeField] private HoldInputZone fireZone;

        public event Action<Vector2, float> OnFireRequested;

        private LineRenderer _lineRenderer;
        private TrajectoryPredictor _predictor;
        private Vector2 _shooterOrigin;
        private float _aimAngleDegrees;
        private bool _firePressedLastFrame;

        private void Awake()
        {
            ConfigureLineRenderer();
        }

        private void Start()
        {
            InitializeBoardGeometry();
        }

        private void Update()
        {
            UpdateAimAngle();
            UpdateFireInput();
            DrawPreview();
        }

        private void ConfigureLineRenderer()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _lineRenderer.widthMultiplier = lineWidth;
            _lineRenderer.useWorldSpace = true;
        }

        private void InitializeBoardGeometry()
        {
            var camera = Camera.main;
            var boardWidth = cols * cellWidth;
            camera.orthographicSize = PlayfieldSizer.OrthographicSizeForWidth(boardWidth, Screen.width, Screen.height);
            _shooterOrigin = ShooterOrigin(camera);
            _predictor = new TrajectoryPredictor(BoardBoundsCalculator.Compute(camera.transform.position, boardWidth, camera.orthographicSize));
        }

        private Vector2 ShooterOrigin(Camera camera)
        {
            var pos = camera.transform.position;
            return new Vector2(pos.x, pos.y - camera.orthographicSize + cellWidth * 0.5f);
        }

        private void UpdateAimAngle()
        {
            var direction = (rotateRightZone.IsPressed ? 1 : 0) - (rotateLeftZone.IsPressed ? 1 : 0);
            _aimAngleDegrees += direction * rotateSpeedDegreesPerSecond * Time.deltaTime;
            _aimAngleDegrees = Mathf.Clamp(_aimAngleDegrees, -maxAimAngleDegrees, maxAimAngleDegrees);
        }

        private void UpdateFireInput()
        {
            if (fireZone.IsPressed && !_firePressedLastFrame)
                OnFireRequested?.Invoke(_shooterOrigin, _aimAngleDegrees);
            _firePressedLastFrame = fireZone.IsPressed;
        }

        private void DrawPreview()
        {
            List<Vector2> points = _predictor.Simulate(_shooterOrigin, _aimAngleDegrees, maxBounces);
            _lineRenderer.positionCount = points.Count;
            for (var i = 0; i < points.Count; i++)
                _lineRenderer.SetPosition(i, points[i]);
        }
    }
}
