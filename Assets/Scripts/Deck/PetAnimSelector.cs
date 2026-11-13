using Game.Pets;
using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// Picks a launcher pet's pixel strip (the guide's pixel_pets): knocked
    /// out or winning at the end; panic during the ceiling warning; a cheer
    /// for 1.4 s after a big pop; the firer's pull or press on every shot;
    /// hurry under 3 s left; the aimer cranking or steering while a slab is
    /// held; otherwise the idle holds. Side views mirror for the right hand
    /// (pull and press the other way); front views never mirror. Strips are
    /// the game pet sheets' game_&lt;species&gt;_&lt;shape&gt;_&lt;anim&gt;.
    /// </summary>
    public static class PetAnimSelector
    {
        public const float CheerSeconds = 1.4f;
        public const float HurrySeconds = 3f;
        public const float FireSeconds = 4f / 12f;
        public const float KnockSeconds = 0.5f;

        public static PetPose Pick(PetMoment moment)
        {
            var key = Key(moment);
            if (moment.Outcome == PetOutcome.Lost) return KnockedOut(moment, key);
            if (moment.Outcome == PetOutcome.Won) return Front(key + "_win", moment.SinceOutcome);
            if (moment.Panicking) return Front(key + "_panic", moment.Time);
            if (moment.SinceCheer < CheerSeconds) return Front(key + "_cheer", moment.SinceCheer);
            return moment.Role == PetRole.Firer ? Firer(moment, key) : Aimer(moment, key);
        }

        private static string Key(PetMoment moment) => $"game_{(moment.Species == PetSpecies.Cat ? "cat" : "dog")}_{moment.Shape}";

        private static PetPose KnockedOut(PetMoment moment, string key)
        {
            var role = moment.Role == PetRole.Aimer ? "aimer" : "firer";
            if (moment.SinceOutcome < KnockSeconds) return new PetPose($"{key}_knock_{role}", (moment.SinceOutcome, PetPose.NoFixedFrame, true), false);
            return Side($"{key}_ko_{role}", moment.Time, moment.RightHanded);
        }

        private static PetPose Firer(PetMoment moment, string key)
        {
            if (moment.SinceFire < FireSeconds) return Fire(moment, key);
            if (IsHurrying(moment)) return Front(key + "_hurry_firer", moment.Time);
            return Side(key + "_fire_hold", moment.Time, moment.RightHanded);
        }

        private static PetPose Fire(PetMoment moment, string key)
        {
            var frame = Mathf.Min(3, Mathf.FloorToInt(moment.SinceFire * 12f));
            var id = key + (moment.Species == PetSpecies.Cat ? "_pull" : "_press");
            return new PetPose(id, (0f, frame, false), !moment.RightHanded);
        }

        private static PetPose Aimer(PetMoment moment, string key)
        {
            if (IsHurrying(moment)) return Side(key + "_hurry_aimer", moment.Time * 2f, moment.RightHanded);
            if (moment.RotateDirection == 0) return Side(key + "_aim_hold", moment.Time, moment.RightHanded);
            if (moment.Species == PetSpecies.Dog) return new PetPose(key + "_aim_right", (0f, CrankFrame(moment.CrankStep), false), moment.RightHanded);
            var towardPointer = (moment.RotateDirection > 0) != moment.RightHanded;
            return Side(key + (towardPointer ? "_aim_right" : "_aim_left"), moment.Time, moment.RightHanded);
        }

        private static bool IsHurrying(PetMoment moment) => moment.ShotTimeRemaining > 0f && moment.ShotTimeRemaining < HurrySeconds;

        private static int CrankFrame(int step) => ((step % 8) + 8) % 8;

        private static PetPose Front(string id, float time) => new PetPose(id, (time, PetPose.NoFixedFrame, false), false);

        private static PetPose Side(string id, float time, bool rightHanded) => new PetPose(id, (time, PetPose.NoFixedFrame, false), rightHanded);
    }
}
