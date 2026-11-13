using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// The cartoon game guide's deck (Assets/Art/Game/Spec/layout.json), in pt,
    /// left-hand layout. Mechanism parts are relative to the mechanism's top
    /// (y 640 solo, 250 in a Battle half); the right hand mirrors x. The
    /// pixel rig behind it is in art px at 5/3 pt per art px.
    /// </summary>
    public static class DeckLayout
    {
        public const float PtPerArtPx = 5f / 3f;
        public const float SoloMechanismTop = 640f;
        public const float BattleMechanismTop = 250f;
        public const float SoloStageHeight = 780f;
        public const float BattleStageHeight = 390f;

        public static readonly Rect Pipes = new(0f, 0f, 360f, 140f);
        public static readonly Vector2 Pointer = new(180f, 76.67f);
        public const float PipRadius = 36.67f;
        public const float PointerHitRadius = 27.5f;
        public static readonly Vector2 NextRing = new(300f, 101.67f);
        public static readonly Rect NextSign = new(280f, 53.33f, 40f, 26f);
        public static readonly Rect NextLabel = new(280f, 54.33f, 40f, 16f);

        public const float SlabTop = 28f;
        public static readonly Vector2 SlabSize = new(43.33f, 76f);
        public const float LeftHandSlabsX = 0f;
        public const float RightHandSlabsX = 273.33f;

        public static readonly Vector2 Crank = new(126.67f, 116.67f);
        public const float CrankKnobArtPx = 2f;
        public static readonly Vector2 Wheel = new(143.33f, 123.33f);
        public static readonly Vector2 LeverPivot = new(228.33f, 135f);
        public static readonly float[] LeverRadians = { -0.5f, -0.15f, 0.25f };
        public static readonly int[] LeverFrames = { 0, 1, 2, 0 };
        public static readonly Rect FireButton = new(208.33f, 116.67f, 33.33f, 23.33f);
        public static readonly int[] ButtonDownFrames = { 0, 0, 1, 0 };

        public const float AimerArtX = 50f;
        public const float FirerArtX = 124f;
        public const float PetArtY = 52f;
        public const float FrontPoseRaiseArtPx = 12f;

        public static readonly Vector2 SoloHurryBurst = new(180f, 567f);
        public static readonly Vector2 BattleHurryBurst = new(180f, 233.33f);

        public static readonly DeckGear[] Gears =
        {
            new("gear_24", new Vector2(160f, 96.67f), 14, 1),
            new("gear_9", new Vector2(110f, 70f), 8, -1),
            new("gear_8", new Vector2(120f, 126.67f), 8, -1),
            new("gear_8", new Vector2(213.33f, 60f), 8, -1),
            new("gear_11", new Vector2(226.67f, 103.33f), 10, -1),
            new("gear_7", new Vector2(196.67f, 130f), 8, 1),
            new("gear_8", new Vector2(263.33f, 73.33f), 8, 1)
        };
    }
}
