using System.Collections.Generic;
using Game.Grid;
using Game.Pets;
using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// Runs the launcher pets: the aimer (and in solo the firer) watch the
    /// shooter, the shot clock, big pops, the ceiling warning and the level
    /// result, and each frame show the strip PetAnimSelector picks.
    /// </summary>
    public class PetStage : MonoBehaviour
    {
        private const int CheerPopCount = 5;
        private const float IdleSeconds = 10f;

        private readonly List<(PetRigView View, PetRole Role, PetLook Look)> _pets = new();
        private PetStageInputs _inputs;
        private float _sinceFire = IdleSeconds;
        private float _sinceCheer = IdleSeconds;
        private float _sinceOutcome;
        private PetOutcome _outcome;

        public static void Create(DeckUi ui, MechanismParts parts, PetStageInputs inputs)
        {
            var stage = parts.AimerLayer.gameObject.AddComponent<PetStage>();
            stage._inputs = inputs;
            stage.AddPet(ui, parts.AimerLayer, (PetRole.Aimer, inputs.Setup.Aimer));
            if (inputs.Setup.HasFirer) stage.AddPet(ui, parts.FirerLayer, (PetRole.Firer, inputs.Setup.Firer));
            stage.Subscribe();
        }

        private void AddPet(DeckUi ui, RectTransform layer, (PetRole Role, PetSpecies Species) pet)
        {
            var look = PetLook.From(_inputs.Catalog, pet.Species, PetProfileStore.Load(pet.Species));
            var artX = pet.Role == PetRole.Aimer ? DeckLayout.AimerArtX : DeckLayout.FirerArtX;
            _pets.Add((PetRigView.Create(ui, layer, (_inputs.Setup.Geometry, artX, look)), pet.Role, look));
        }

        private void Subscribe()
        {
            _inputs.Shooter.OnFireRequested += HandleFire;
            _inputs.Board.OnBubblesPopped += HandlePop;
            _inputs.Board.OnLevelLoaded += HandleLevelLoaded;
            if (_inputs.Game == null) return;
            _inputs.Game.OnLevelWon += HandleWon;
            _inputs.Game.OnLevelLost += HandleLost;
        }

        private void OnDestroy()
        {
            _inputs.Shooter.OnFireRequested -= HandleFire;
            _inputs.Board.OnBubblesPopped -= HandlePop;
            _inputs.Board.OnLevelLoaded -= HandleLevelLoaded;
            if (_inputs.Game == null) return;
            _inputs.Game.OnLevelWon -= HandleWon;
            _inputs.Game.OnLevelLost -= HandleLost;
        }

        private void HandleFire(Vector2 origin, float angleDegrees) => _sinceFire = 0f;

        private void HandlePop(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color)
        {
            if (cells.Count >= CheerPopCount) _sinceCheer = 0f;
        }

        private void HandleLevelLoaded(int levelNumber) => _outcome = PetOutcome.Playing;

        private void HandleWon() => EndWith(PetOutcome.Won);

        private void HandleLost() => EndWith(PetOutcome.Lost);

        private void EndWith(PetOutcome outcome)
        {
            _outcome = outcome;
            _sinceOutcome = 0f;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            _sinceFire = Mathf.Min(_sinceFire + deltaTime, IdleSeconds);
            _sinceCheer = Mathf.Min(_sinceCheer + deltaTime, IdleSeconds);
            _sinceOutcome += deltaTime;
            foreach (var pet in _pets) pet.View.Show(PetAnimSelector.Pick(MomentFor(pet.Role, pet.Look)));
        }

        private PetMoment MomentFor(PetRole role, PetLook look) => new PetMoment
        {
            Role = role,
            Species = look.Species,
            Shape = look.ShapeArtId,
            RightHanded = _inputs.Setup.Geometry.RightHanded,
            RotateDirection = _inputs.Shooter.enabled ? _inputs.Shooter.RotateDirection : 0,
            CrankStep = _inputs.Mechanism.Step,
            ShotTimeRemaining = _outcome == PetOutcome.Playing ? _inputs.Clock.ShotTimeRemaining : 0f,
            SinceFire = _sinceFire,
            SinceCheer = _sinceCheer,
            Panicking = _inputs.Game != null && _inputs.Game.IsCeilingWarning,
            Outcome = _outcome,
            SinceOutcome = _sinceOutcome,
            Time = Time.time
        };
    }
}
