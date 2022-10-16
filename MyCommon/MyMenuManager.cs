namespace Flowers_Yasuo.MyCommon
{
    #region 

    using System;

    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;

    using Flowers_Yasuo.MyBase;



    #endregion

    public class MyMenuManager
    {
        public static void Initializer()
        {
            try
            {
                MyLogic.Menu = new Menu("FlowersYasuo", "Flowers Yasuo", true);
                {
                    MyLogic.Menu.Add(new MenuSeparator("MadebyNightMoon", "Made by NightMoon"));
                }

                MyLogic.ComboMenu = new Menu("FlowersYasuo.ComboMenu", ":: Combo Settings");
                {
                    MyLogic.ComboMenu.Add(new MenuBool("FlowersYasuo.ComboMenu.Q", "Use Q"));
                    MyLogic.ComboMenu.Add(new MenuBool("FlowersYasuo.ComboMenu.Q3", "Use Q3"));
                    MyLogic.ComboMenu.Add(new MenuBool("FlowersYasuo.ComboMenu.E", "Use E"));
                    MyLogic.ComboMenu.Add(new MenuBool("FlowersYasuo.ComboMenu.ETurret", "Use E| When Under Enemy Turret", false));
                    MyLogic.ComboMenu.Add(new MenuBool("FlowersYasuo.ComboMenu.EGapcloser", "Use E| Gapcloser to target"));
                    MyLogic.ComboMenu.Add(new MenuList("FlowersYasuo.ComboMenu.EGapcloserMode", "Use E Gapcloser Mode: ",
                        new[] { "To target", "To mouse" }));
                    MyLogic.ComboMenu.Add(new MenuBool("FlowersYasuo.ComboMenu.EQ", "Use EQ"));
                    MyLogic.ComboMenu.Add(new MenuBool("FlowersYasuo.ComboMenu.EQ3", "Use EQ3"));
                    MyLogic.ComboMenu.Add(new MenuKeyBind("FlowersYasuo.ComboMenu.R", "Use R", Keys.R,
                        KeyBindType.Toggle) { Active = true }).AddPermashow();
                    MyLogic.ComboMenu.Add(new MenuSliderButton("FlowersYasuo.ComboMenu.RTargetHP",
                        "Use R| When target HealthPercent <= x%", 65, 1, 101));
                    MyLogic.ComboMenu.Add(new MenuSliderButton("FlowersYasuo.ComboMenu.RHitCount",
                        "Use R| When Min Hit Count >= x", 3, 1, 5));
                    foreach (var hero in GameObjects.EnemyHeroes)
                    {
                        MyLogic.ComboMenu.Add(new MenuBool("FlowersYasuo.ComboMenu.RTargetFor" + hero.CharacterName,
                            "Use On " + hero.CharacterName));
                    }
                    MyLogic.ComboMenu.Add(new MenuKeyBind("FlowersYasuo.ComboMenu.EQFlash", "Use EQ Flash",
                        (EnsoulSharp.SDK.MenuUI.Keys)Keys.H, KeyBindType.Toggle)).AddPermashow();
                    MyLogic.ComboMenu.Add(new MenuBool("FlowersYasuo.ComboMenu.EQFlashKS", "Use EQ Flash| When Can KillAble in 1v1 or 1v2"));
                    MyLogic.ComboMenu.Add(new MenuSliderButton("FlowersYasuo.ComboMenu.EQFlashCount",
                        "Use EQ Flash| When Min Hit Count >= x", 3, 1, 5));
                    MyLogic.ComboMenu.Add(new MenuBool("FlowersYasuo.ComboMenu.Ignite", "Use Ignite"));
                }
                MyLogic.Menu.Add(MyLogic.ComboMenu);

                MyLogic.HarassMenu = new Menu("FlowersYasuo.HarassMenu", ":: Harass Settings");
                {
                    MyLogic.HarassMenu.Add(new MenuSeparator("FlowersYasuo.HarassMenu.QSettings", "-- Q Settings"));
                    MyLogic.HarassMenu.Add(new MenuBool("FlowersYasuo.HarassMenu.Q", "Use Q"));
                    MyLogic.HarassMenu.Add(new MenuBool("FlowersYasuo.HarassMenu.Q3", "Use Q3"));

                    MyLogic.HarassMenu.Add(new MenuSeparator("FlowersYasuo.HarassMenu.AutoHarassSettings", "-- Auto Harass Settings"));
                    MyLogic.HarassMenu.Add(new MenuKeyBind("FlowersYasuo.HarassMenu.AutoQ", "Auto Q Harass",
                        (EnsoulSharp.SDK.MenuUI.Keys)Keys.N, KeyBindType.Toggle){ Active = true }).AddPermashow();
                    MyLogic.HarassMenu.Add(new MenuBool("FlowersYasuo.HarassMenu.AutoQ3", "Auto Q3 Harass", false));
                }
                MyLogic.Menu.Add(MyLogic.HarassMenu);

                MyLogic.ClearMenu = new Menu("FlowersYasuo.ClearMenu", ":: Clear Settings");
                {
                    MyLogic.ClearMenu.Add(new MenuSeparator("FlowersYasuo.ClearMenu.LaneClearSettings", "-- LaneClear Settings"));
                    MyLogic.ClearMenu.Add(new MenuBool("FlowersYasuo.ClearMenu.LaneClearQ", "Use Q"));
                    MyLogic.ClearMenu.Add(new MenuBool("FlowersYasuo.ClearMenu.LaneClearQ3", "Use Q3"));
                    MyLogic.ClearMenu.Add(new MenuBool("FlowersYasuo.ClearMenu.LaneClearE", "Use E"));
                    MyLogic.ClearMenu.Add(new MenuBool("FlowersYasuo.ClearMenu.LaneClearEQ", "Use EQ"));
                    MyLogic.ClearMenu.Add(new MenuBool("FlowersYasuo.ClearMenu.LaneClearTurret", "Allow Under Turret Farm", false));

                    MyLogic.ClearMenu.Add(new MenuSeparator("FlowersYasuo.ClearMenu.JungleClearSettings", "-- JungleClear Settings"));
                    MyLogic.ClearMenu.Add(new MenuBool("FlowersYasuo.ClearMenu.JungleClearQ", "Use Q"));
                    MyLogic.ClearMenu.Add(new MenuBool("FlowersYasuo.ClearMenu.JungleClearQ3", "Use Q3"));
                    MyLogic.ClearMenu.Add(new MenuBool("FlowersYasuo.ClearMenu.JungleClearE", "Use E"));
                }
                MyLogic.Menu.Add(MyLogic.ClearMenu);

                MyLogic.LastHitMenu = new Menu("FlowersYasuo.LastHitMenu", ":: LastHit Settings");
                {
                    MyLogic.LastHitMenu.Add(new MenuSeparator("FlowersYasuo.LastHitMenu.QSettings", "-- Q Settings"));              
                    MyLogic.LastHitMenu.Add(new MenuBool("FlowersYasuo.LastHitMenu.Q", "Use Q"));
                    MyLogic.LastHitMenu.Add(new MenuBool("FlowersYasuo.LastHitMenu.Q3", "Use Q3", false));

                    MyLogic.LastHitMenu.Add(new MenuSeparator("FlowersYasuo.LastHitMenu.ESettings", "-- E Settings"));
                    MyLogic.LastHitMenu.Add(new MenuBool("FlowersYasuo.LastHitMenu.E", "Use E", false));
                }
                MyLogic.Menu.Add(MyLogic.LastHitMenu);

                MyLogic.FleeMenu = new Menu("FlowersYasuo.FleeMenu", ":: Flee Settings");
                {
                    MyLogic.FleeMenu.Add(new MenuSeparator("FlowersYasuo.FleeMenu.KeySettings", "-- Key Settings"));
                    MyLogic.FleeMenu.Add(new MenuKeyBind("FlowersYasuo.FleeMenu.FleeKey", "Flee Key",
                        Keys.Z, KeyBindType.Press));

                    MyLogic.FleeMenu.Add(new MenuSeparator("FlowersYasuo.FleeMenu.QSettings", "-- Q Settings"));
                    MyLogic.FleeMenu.Add(new MenuBool("FlowersYasuo.FleeMenu.Q3", "Use Q3"));

                    MyLogic.FleeMenu.Add(new MenuSeparator("FlowersYasuo.FleeMenu.ESettings", "-- E Settings"));
                    MyLogic.FleeMenu.Add(new MenuBool("FlowersYasuo.FleeMenu.E", "Use E"));

                    MyLogic.FleeMenu.Add(new MenuSeparator("FlowersYasuo.FleeMenu.EQSettings", "-- EQ Settings"));
                    MyLogic.FleeMenu.Add(new MenuBool("FlowersYasuo.FleeMenu.EQ", "Use EQ"));

                    //MyLogic.FleeMenu.Add(new MenuSeparator("FlowersYasuo.FleeMenu.WallJumpSettings", "-- Wall Jump Settings"));
                    //MyLogic.FleeMenu.Add(new MenuSeparator("FlowersYasuo.FleeMenu.WallJumpTODO", ""));
                }
                MyLogic.Menu.Add(MyLogic.FleeMenu);

                MyLogic.KillStealMenu = new Menu("FlowersYasuo.KillStealMenu", ":: KillSteal Settings");
                {
                    MyLogic.KillStealMenu.Add(new MenuSeparator("FlowersYasuo.KillStealMenu.QSettings", "-- Q Settings"));
                    MyLogic.KillStealMenu.Add(new MenuBool("FlowersYasuo.KillStealMenu.Q", "Use Q"));
                    MyLogic.KillStealMenu.Add(new MenuBool("FlowersYasuo.KillStealMenu.Q3", "Use Q3"));

                    MyLogic.KillStealMenu.Add(new MenuSeparator("FlowersYasuo.KillStealMenu.ESettings", "-- E Settings"));
                    MyLogic.KillStealMenu.Add(new MenuBool("FlowersYasuo.KillStealMenu.E", "Use E"));
                }
                MyLogic.Menu.Add(MyLogic.KillStealMenu);

                MyLogic.MiscMenu = new Menu("FlowersYasuo.MiscMenu", ":: Misc Settings");
                {
                    MyManaManager.AddFarmToMenu(MyLogic.MiscMenu);

                    MyLogic.MiscMenu.Add(new MenuSeparator("FlowersYasuo.MiscMenu.QSettings", "-- Q Settings"));
                    //MyLogic.MiscMenu.Add(new MenuBool("FlowersYasuo.MiscMenu.Q3Interrupt", "Use Q3| Interrupt Danger Spell"));
                    //MyLogic.MiscMenu.Add(new MenuBool("FlowersYasuo.MiscMenu.Q3AntiGapcloser", "Use Q3| Anti Gapcloser"));
                    MyLogic.MiscMenu.Add(new MenuKeyBind("FlowersYasuo.MiscMenu.StackQ", "Stack Q Key",
                        Keys.T, KeyBindType.Toggle)).AddPermashow();

                    //MyLogic.MiscMenu.Add(new MenuSeparator("FlowersYasuo.MiscMenu.ESettings", "-- E Settings"));
                    //MyLogic.MiscMenu.Add(new MenuBool("FlowersYasuo.MiscMenu.CheckESafe", "Check Safe"));

                    MyLogic.MiscMenu.Add(new MenuSeparator("FlowersYasuo.MiscMenu.RSettings", "-- R Settings"));
                    MyLogic.MiscMenu.Add(new MenuBool("FlowersYasuo.MiscMenu.AutoR", "Auto R"));
                    MyLogic.MiscMenu.Add(new MenuSlider("FlowersYasuo.MiscMenu.AutoRCount", "Auto R| And Min Hit Count >= x", 3, 1, 5));
                    MyLogic.MiscMenu.Add(new MenuSlider("FlowersYasuo.MiscMenu.AutoRAlly", "Auto R| And My Allies Count >= x", 2, 1, 5));
                    MyLogic.MiscMenu.Add(new MenuSlider("FlowersYasuo.MiscMenu.AutoRHP", "Auto R| And My HealthPercednt >= x", 50, 1, 99));

                    MyLogic.MiscMenu.Add(new MenuSeparator("FlowersYasuo.MiscMenu.EQSettings", "-- EQ Settings"));
                    MyLogic.MiscMenu.Add(new MenuKeyBind("FlowersYasuo.MiscMenu.EQFlashKey", "EQ Flash Key",
                        Keys.A, KeyBindType.Press));
                }
                MyLogic.Menu.Add(MyLogic.MiscMenu);

                MyLogic.EvadeMenu = new Menu("FlowersYasuo.EvadeMenu", ":: Evade Settings");
                {
                    MyLogic.EvadeMenu.Add(new MenuSeparator("FlowersYasuo.EvadeMenu.WaitEvade", "Wait MoonEvade xD"));
                }
                MyLogic.Menu.Add(MyLogic.EvadeMenu);

                MyLogic.DrawMenu = new Menu("FlowersYasuo.DrawMenu", ":: Draw Settings");
                {
                    MyLogic.DrawMenu.Add(new MenuSeparator("FlowersYasuo.DrawMenu.RangeSettings", "-- Spell Range"));
                    MyLogic.DrawMenu.Add(new MenuBool("FlowersYasuo.DrawMenu.Q", "Draw Q Range", false));
                    MyLogic.DrawMenu.Add(new MenuBool("FlowersYasuo.DrawMenu.Q3", "Draw Q3 Range", false));
                    MyLogic.DrawMenu.Add(new MenuBool("FlowersYasuo.DrawMenu.E", "Draw E Range", false));
                    MyLogic.DrawMenu.Add(new MenuBool("FlowersYasuo.DrawMenu.R", "Draw R Range", false));
                }
                MyLogic.Menu.Add(MyLogic.DrawMenu);

                MyLogic.Menu.Attach();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyMenuManager.Initializer." + ex);
            }
        }
    }
}