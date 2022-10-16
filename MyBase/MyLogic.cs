namespace Flowers_Yasuo.MyBase
{
    #region

    using SharpDX;

    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;

    #endregion

    public class MyLogic
    {
        public static Spell Q { get; set; }
        public static Spell Q3 { get; set; }
        public static Spell W { get; set; }
        public static Spell E { get; set; }
        public static Spell R { get; set; }
        public static Spell Flash { get; set; }
        public static Spell Ignite { get; set; }


        public static SpellSlot IgniteSlot { get; set; } = SpellSlot.Unknown;
        public static SpellSlot FlashSlot { get; set; } = SpellSlot.Unknown;

        public static AIHeroClient Me => ObjectManager.Player;

        public static Menu Menu { get; set; }
        public static Menu ComboMenu { get; set; }
        public static Menu HarassMenu { get; set; }
        public static Menu ClearMenu { get; set; }
        public static Menu LastHitMenu { get; set; }
        public static Menu FleeMenu { get; set; }
        public static Menu KillStealMenu { get; set; }
        public static Menu MiscMenu { get; set; }
        public static Menu EvadeMenu { get; set; }
        public static Menu DrawMenu { get; set; }

        public static Vector3 YasuolastEPos { get; set; } = Vector3.Zero;
        public static int YasuolastETime { get; set; } = 0;
        public static int lastWTime { get; set; } = 0;
        public static bool isYasuoDashing { get; set; } = false;
        public static bool HaveQ3 => ObjectManager.Player.HasBuff("YasuoQ2");
        public static bool IsMyDashing { get; set; } = false;
        public static int YasuolastEQFlashTime { get; set; } = 0;
    }
}
