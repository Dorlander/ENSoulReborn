namespace Flowers_Yasuo.MyCommon
{
    #region 

    using System;

    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;

    #endregion

    public class MyManaManager
    {
        public static bool SpellFarm { get; set; } = true;
        public static bool SpellHarass { get; set; } = true;

        private static int limitTick { get; set; }

        public static void AddFarmToMenu(Menu mainMenu)
        {
            try
            {
                if (mainMenu != null)
                {
                    mainMenu.Add(new MenuSeparator("MyManaManager.SpellFarmSettings", ":: Spell Farm Logic"));
                    mainMenu.Add(new MenuBool("MyManaManager.SpellFarm", "Use Spell To Farm(Mouse Scrool)")).AddPermashow();
                    mainMenu.Add(new MenuKeyBind("MyManaManager.SpellHarass", "Use Spell To Harass(In Clear Mode)",
                       Keys.H, KeyBindType.Toggle){Active = true}).AddPermashow();

                    Game.OnWndProc += delegate (GameWndEventArgs Args)
                    {
                        try
                        {
                            if (Args.Msg == 519)
                            {
                                mainMenu["MyManaManager.SpellFarm"].GetValue<MenuBool>().Enabled = !mainMenu["MyManaManager.SpellFarm"].GetValue<MenuBool>().Enabled;
                                SpellFarm = mainMenu["MyManaManager.SpellFarm"].GetValue<MenuBool>().Enabled;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error in MyManaManager.OnWndProcEvent." + ex);
                        }
                    };

                    Game.OnUpdate += delegate
                    {
                        if (Variables.GameTimeTickCount - limitTick > 20 * Game.Ping)
                        {
                            limitTick = Variables.GameTimeTickCount;
                            SpellFarm = mainMenu["MyManaManager.SpellFarm"].GetValue<MenuBool>().Enabled;
                            SpellHarass = mainMenu["MyManaManager.SpellHarass"].GetValue<MenuKeyBind>().Active;
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyManaManager.AddFarmToMenu." + ex);
            }
        }
    }
}