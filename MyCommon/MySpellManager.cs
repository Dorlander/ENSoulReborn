namespace Flowers_Yasuo.MyCommon
{
    #region

    using System;

    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.Evade;
    using Flowers_Yasuo.MyBase;

    #endregion

    public class MySpellManager
    {
        public static void Initializer()
        {
            try
            {
                MyLogic.Q = new Spell(SpellSlot.Q, 475f);
                MyLogic.Q.SetSkillshot(Q1Delay, 30f, float.MaxValue, false, SpellType.Line);

                MyLogic.Q3 = new Spell(SpellSlot.Q, 1000f);
                MyLogic.Q3.SetSkillshot(Q3Delay, 90f, 1200f, false, SpellType.Line);

                MyLogic.W = new Spell(SpellSlot.W, 400f);

                MyLogic.E = new Spell(SpellSlot.E, 475f) {Delay = 0.075f, Speed = 1025f};

                MyLogic.R = new Spell(SpellSlot.R, 1200f);

                MyLogic.IgniteSlot = ObjectManager.Player.GetSpellSlot("summonerdot");

                if (MyLogic.IgniteSlot != SpellSlot.Unknown)
                {
                    MyLogic.Ignite = new Spell(MyLogic.IgniteSlot, 600);
                }

                MyLogic.FlashSlot = ObjectManager.Player.GetSpellSlot("summonerflash");

                if (MyLogic.FlashSlot != SpellSlot.Unknown)
                {
                    MyLogic.Flash = new Spell(MyLogic.FlashSlot, 425);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MySpellManager.Initializer." + ex);
            }
        }

        private static float DefaultDelay => 1 - Math.Min((ObjectManager.Player.AttackSpeedMod - 1f) * 0.0058552631578947f, 0.6675f);

        public static float Q1Delay => 0.4f * DefaultDelay;

        public static float Q3Delay => 0.5f * DefaultDelay;
    }
}