namespace Flowers_Yasuo.MyCommon
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SharpDX;
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using EnsoulSharp.SDK.Utility;
    using Flowers_Yasuo.MyBase;
    using Color = System.Drawing.Color;
    using EnsoulSharp.SDK.Rendering;

    #endregion

    public class MyEventManager : MyLogic
    {
        public static void Initializer()
        {
            try
            {
                Game.OnUpdate += Args => OnUpdate();
                AIBaseClient.OnBuffAdd += OnBuffGain;
                AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;
                AIBaseClient.OnPlayAnimation += OnPlayAnimation;
                Orbwalker.OnAfterAttack += (sender, Args) => OnAction(Args);
                Drawing.OnDraw += Args => OnRender();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.Initializer." + ex);
            }
        }

        private static void OnUpdate()
        {
            try
            {
                ResetToDefalut();

                if (Me.IsDead || Me.IsRecalling())
                {
                    return;
                }

                if (FleeMenu["FlowersYasuo.FleeMenu.FleeKey"].GetValue<MenuKeyBind>().Active && Me.CanMoveMent())
                {
                    FleeEvent();
                }

                if (MiscMenu["FlowersYasuo.MiscMenu.EQFlashKey"].GetValue<MenuKeyBind>().Active && Me.CanMoveMent())
                {
                    EQFlashEvent();
                }

                KillStealEvent();
                AutoUseEvent();
                
                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Combo:
                        ComboEvent();
                        break;
                    case OrbwalkerMode.Harass:
                        HarassEvent();
                        break;
                    case OrbwalkerMode.LaneClear:
                        ClearEvent();
                        break;
                    case OrbwalkerMode.LastHit:
                        LastHitEvent();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnUpdate." + ex);
            }
        }

        private static void ResetToDefalut()
        {
            try
            {
                Q.Delay = MySpellManager.Q1Delay;
                Q3.Delay = MySpellManager.Q3Delay;

                IsMyDashing = isYasuoDashing || Me.IsDashing();

                if (Variables.GameTimeTickCount - YasuolastETime - (Game.Ping / 2) > 500)
                {
                    isYasuoDashing = false;
                    YasuolastEPos = Vector3.Zero;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.ResetToDefalut." + ex);
            }
        }

        private static void FleeEvent()
        {
            try
            {
                Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                if (IsMyDashing)
                {
                    if (FleeMenu["FlowersYasuo.FleeMenu.EQ"].GetValue<MenuBool>().Enabled && Q.IsReady() && !HaveQ3)
                    {
                        var qMinion =
                            GameObjects.EnemyMinions.FirstOrDefault(
                                x =>
                                    x.IsValidTarget(220, true, YasuolastEPos) && x.Health > 5 &&
                                    !x.Name.ToLower().Contains("plant"));

                        if (qMinion != null && qMinion.IsValidTarget())
                        {
                            Q.Cast(Me.PreviousPosition);
                        }
                    }
                }
                else
                {
                    if (FleeMenu["FlowersYasuo.FleeMenu.Q3"].GetValue<MenuBool>().Enabled && HaveQ3 && Q3.IsReady() &&
                        GameObjects.EnemyHeroes.Any(x => x.IsValidTarget(Q3.Range)))
                    {
                        CastQ3();
                    }

                    if (FleeMenu["FlowersYasuo.FleeMenu.E"].GetValue<MenuBool>().Enabled && E.IsReady())
                    {
                        var obj = MyExtraManager.GetNearObj();

                        if (obj != null && obj.IsValidTarget(E.Range))
                        {
                            E.CastOnUnit(obj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.FleeEvent." + ex);
            }
        }

        private static void EQFlashEvent()
        {
            try
            {
                if (Orbwalker.ActiveMode == OrbwalkerMode.None && FlashSlot != SpellSlot.Unknown && Flash.IsReady())
                {
                    Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                    if (!HaveQ3)
                    {
                        if (Q.IsReady())
                        {
                            var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.IsValidTarget(Q.Range) && x.MaxHealth > 5);

                            if (minion != null && minion.IsValidTarget(Q.Range))
                            {
                                var pred = Q.GetPrediction(minion);
                                if (pred.Hitchance >= HitChance.Medium)
                                {
                                    Q.Cast(pred.CastPosition);
                                }
                            }
                        }
                    }
                    else if (HaveQ3 && Q3.IsReady())
                    {
                        if (IsMyDashing && FlashSlot != SpellSlot.Unknown && Flash.IsReady())
                        {
                            var bestPos =
                                MyExtraManager.FlashPoints().ToArray()
                                    .Where(x => GameObjects.EnemyHeroes.Count(a => a.IsValidTarget(600f, true, x)) > 0)
                                    .OrderByDescending(x => GameObjects.EnemyHeroes.Count(i => i.Distance(x) <= 220))
                                    .FirstOrDefault();

                            if (bestPos != Vector3.Zero && bestPos.CountEnemyHeroesInRange(220) > 0 && Q.Cast(Me.PreviousPosition))
                            {
                                DelayAction.Add(10 + (Game.Ping / 2 - 5), () =>
                                {
                                    Flash.Cast(bestPos);
                                    YasuolastEQFlashTime = Variables.GameTimeTickCount;
                                });
                            }
                        }

                        if (E.IsReady())
                        {
                            var allTargets = new List<AIBaseClient>();

                            allTargets.AddRange(GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range) && x.MaxHealth > 5));
                            allTargets.AddRange(GameObjects.EnemyHeroes.Where(x => !x.IsDead && x.IsValidTarget(E.Range)));

                            if (allTargets.Any())
                            {
                                var eTarget =
                                    allTargets.Where(x => x.IsValidTarget(E.Range) && MyExtraManager.CanCastE(x))
                                        .OrderByDescending(
                                            x =>
                                                GameObjects.EnemyHeroes.Count(
                                                    t => t.IsValidTarget(600f, true, MyExtraManager.PosAfterE(x))))
                                        .FirstOrDefault();

                                if (eTarget != null)
                                {
                                    E.CastOnUnit(eTarget);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.EQFlashEvent." + ex);
            }
        }

        private static void KillStealEvent()
        {
            try
            {
                if (IsMyDashing)
                {
                    return;
                }

                if (KillStealMenu["FlowersYasuo.KillStealMenu.Q"].GetValue<MenuBool>().Enabled && Q.IsReady() && !HaveQ3)
                {
                    foreach (
                        var target in
                        GameObjects.EnemyHeroes.Where(
                            x =>
                                x.IsValidTarget(Q.Range) && x.Health < Me.GetSpellDamage(x, SpellSlot.Q)))
                    {
                        if (target.IsValidTarget(Q.Range) && !target.IsUnKillable())
                        {
                            CastQ(target);
                            return;
                        }
                    }
                }

                if (KillStealMenu["FlowersYasuo.KillStealMenu.Q3"].GetValue<MenuBool>().Enabled && Q3.IsReady() && HaveQ3)
                {
                    foreach (
                        var target in
                        GameObjects.EnemyHeroes.Where(
                            x =>
                                x.IsValidTarget(Q3.Range) && x.Health < Me.GetSpellDamage(x, SpellSlot.Q)))
                    {
                        if (target.IsValidTarget(Q3.Range) && !target.IsUnKillable())
                        {
                            Q3.Cast(target);
                            return;
                        }
                    }
                }

                if (KillStealMenu["FlowersYasuo.KillStealMenu.E"].GetValue<MenuBool>().Enabled && E.IsReady())
                {
                    foreach (
                        var target in
                        GameObjects.EnemyHeroes.Where(
                            x =>
                                x.IsValidTarget(E.Range) &&
                                x.Health <
                                Me.GetSpellDamage(x, SpellSlot.E) + Me.GetSpellDamage(x, SpellSlot.E)))
                    {
                        if (target.IsValidTarget(E.Range) && !target.IsUnKillable())
                        {
                            E.CastOnUnit(target);
                            return;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.KillStealEvent." + ex);
            }
        }

        private static void AutoUseEvent()
        {
            try
            {
                if (Q.IsReady())
                {
                    if (HarassMenu["FlowersYasuo.HarassMenu.AutoQ"].GetValue<MenuKeyBind>().Active)
                    {
                        AutoQHarassEvent();
                    }

                    if (MiscMenu["FlowersYasuo.MiscMenu.StackQ"].GetValue<MenuKeyBind>().Active)
                    {
                        StackQEvent();
                    }
                }

                if (MiscMenu["FlowersYasuo.MiscMenu.AutoR"].GetValue<MenuBool>().Enabled && R.IsReady())
                {
                    if (Variables.GameTimeTickCount - YasuolastEQFlashTime < 800)
                    {
                        var enemiesKnockedUp =
                            GameObjects.EnemyHeroes
                                .Where(x => x.IsValidTarget(R.Range))
                                .Where(x => !x.IsInvulnerable)
                                .Where(x => x.HasBuffOfType(BuffType.Knockup));
                        var enemies = enemiesKnockedUp as IList<AIHeroClient> ?? enemiesKnockedUp.ToList();

                        if (enemies.Count > 0)
                        {
                            var enemy = enemies.FirstOrDefault();
                            if (enemy != null)
                            {
                                R.Cast(enemy.Position);
                            }
                        }
                    }
                    else
                    {
                        var enemiesKnockedUp =
                            GameObjects.EnemyHeroes
                                .Where(x => x.IsValidTarget(R.Range))
                                .Where(x => !x.IsInvulnerable)
                                .Where(x => x.HasBuffOfType(BuffType.Knockup));
                        var enemies = enemiesKnockedUp as IList<AIHeroClient> ?? enemiesKnockedUp.ToList();
                        var allallies =
                            GameObjects.AllyHeroes
                                .Where(x => x.IsValidTarget(R.Range, false) && !x.IsMe)
                                .Where(x => !x.IsInvulnerable);
                        var allies = allallies as IList<AIHeroClient> ?? allallies.ToList();

                        if (enemies.Count >= MiscMenu["FlowersYasuo.MiscMenu.AutoRCount"].GetValue<MenuSlider>().Value &&
                            Me.HealthPercent >= MiscMenu["FlowersYasuo.MiscMenu.AutoRHP"].GetValue<MenuSlider>().Value &&
                            (MiscMenu["FlowersYasuo.MiscMenu.AutoRAlly"].GetValue<MenuSlider>().Value == 0 || 
                            allies.Count >= MiscMenu["FlowersYasuo.MiscMenu.AutoRAlly"].GetValue<MenuSlider>().Value))
                        {
                            var enemy = enemies.FirstOrDefault();
                            if (enemy != null)
                            {
                                R.Cast(enemy.Position);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.AutoUseEvent." + ex);
            }
        }

        private static void AutoQHarassEvent()
        {
            try
            {
                if (IsMyDashing || Me.CountEnemyHeroesInRange(Q.Range) == 0 || Me.IsUnderEnemyTurret() ||
                    Orbwalker.ActiveMode == OrbwalkerMode.Combo || Orbwalker.ActiveMode == OrbwalkerMode.Harass ||
                    FleeMenu["FlowersYasuo.FleeMenu.FleeKey"].GetValue<MenuKeyBind>().Active)
                {
                    return;
                }

                if (HarassMenu["FlowersYasuo.HarassMenu.AutoQ3"].GetValue<MenuBool>().Enabled && HaveQ3)
                {
                    CastQ3();
                }
                else if (!HaveQ3)
                {
                    var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

                    if (target != null && target.IsValidTarget(Q.Range))
                    {
                        CastQ(target);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.AutoQHarassEvent." + ex);
            }
        }

        private static void StackQEvent()
        {
            try
            {
                if (IsMyDashing || HaveQ3 || Me.CountEnemyHeroesInRange(Q.Range) > 0 || Me.IsUnderEnemyTurret() ||
                    Orbwalker.ActiveMode != OrbwalkerMode.None ||
                    FleeMenu["FlowersYasuo.FleeMenu.FleeKey"].GetValue<MenuKeyBind>().Active)
                {
                    return;
                }

                var minion = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) && x.MaxHealth > 5).FirstOrDefault(x => x.IsValidTarget(Q.Range));

                if (minion != null && minion.IsValidTarget(Q.Range))
                {
                    var pred = Q.GetPrediction(minion);
                    if (pred.Hitchance >= HitChance.Medium)
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.StackQEvent." + ex);
            }
        }

        private static void ComboEvent()
        {
            try
            {
                var target = TargetSelector.GetTarget(1200,DamageType.Physical);

                if (target != null && target.IsValidTarget(1200))
                {
                    if (ComboMenu["FlowersYasuo.ComboMenu.Ignite"].GetValue<MenuBool>().Enabled && IgniteSlot != SpellSlot.Unknown &&
                        Ignite.IsReady() && target.IsValidTarget(600) && 
                        (target.HealthPercent <= 25 || target.Health <= Me.GetIgniteDamage(target)))
                    {
                        Ignite.CastOnUnit(target);
                    }

                    if (ComboMenu["FlowersYasuo.ComboMenu.EQFlash"].GetValue<MenuKeyBind>().Active)
                    {
                        ComboEQFlashEvent(target);
                    }

                    if (ComboMenu["FlowersYasuo.ComboMenu.R"].GetValue<MenuKeyBind>().Active && R.IsReady())
                    {
                        foreach (var rTarget in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(1200) && MyExtraManager.CanCastR(x)))
                        {
                            if (ComboMenu["FlowersYasuo.ComboMenu.RHitCount"].GetValue<MenuSliderButton>().Enabled)
                            {
                                if (rTarget.IsValidTarget(1200))
                                {
                                    var enemiesKnockedUp =
                                        GameObjects.EnemyHeroes
                                            .Where(x => x.IsValidTarget(R.Range))
                                            .Where(MyExtraManager.CanCastR);

                                    var enemiesKnocked = enemiesKnockedUp as IList<AIHeroClient> ?? enemiesKnockedUp.ToList();

                                    if (enemiesKnocked.Count >= ComboMenu["FlowersYasuo.ComboMenu.RHitCount"].GetValue<MenuSliderButton>().Value)
                                    {
                                        R.Cast(rTarget.Position);
                                    }
                                }
                            }

                            if (ComboMenu["FlowersYasuo.ComboMenu.RTargetHP"].GetValue<MenuSliderButton>().Enabled)
                            {
                                if (rTarget.IsValidTarget(R.Range))
                                {
                                    if (ComboMenu["FlowersYasuo.ComboMenu.RTargetFor" + rTarget.CharacterName].GetValue<MenuBool>().Enabled &&
                                        MyExtraManager.CanCastR(rTarget) &&
                                        rTarget.HealthPercent <=
                                        ComboMenu["FlowersYasuo.ComboMenu.RTargetHP"].GetValue<MenuSliderButton>().Value)
                                    {
                                        R.Cast(rTarget.Position);
                                    }
                                }
                            }
                        }
                    }

                    if (E.IsReady())
                    {
                        if (ComboMenu["FlowersYasuo.ComboMenu.E"].GetValue<MenuBool>().Enabled && target.IsValidTarget(E.Range))
                        {
                            var dmg = Me.GetSpellDamage(target, SpellSlot.Q) * 2 + Me.GetSpellDamage(target, SpellSlot.E) +
                                      Me.GetAutoAttackDamage(target) * 2 +
                                      (R.IsReady()
                                          ? Me.GetSpellDamage(target, SpellSlot.R)
                                          : Me.GetSpellDamage(target, SpellSlot.Q));

                            if (target.DistanceToPlayer() >= Me.BoundingRadius + Me.AttackRange + 65 &&
                                (dmg >= target.Health || HaveQ3 && Q.IsReady()) && MyExtraManager.CanCastE(target) &&
                                (ComboMenu["FlowersYasuo.ComboMenu.ETurret"].GetValue<MenuBool>().Enabled) ||
                                 !MyExtraManager.UnderTower(MyExtraManager.PosAfterE(target)))
                            {
                                E.CastOnUnit(target);
                            }
                        }

                        if (ComboMenu["FlowersYasuo.ComboMenu.EGapcloser"].GetValue<MenuBool>().Enabled)
                        {
                            if (!target.InAutoAttackRange())
                            {
                                if (ComboMenu["FlowersYasuo.ComboMenu.EGapcloserMode"].GetValue<MenuList>().Index == 0)
                                {
                                    MyExtraManager.EGapTarget(target,
                                        ComboMenu["FlowersYasuo.ComboMenu.ETurret"].GetValue<MenuBool>().Enabled,
                                        Me.BoundingRadius + Me.AttackRange + target.BoundingRadius - 50, HaveQ3);
                                }
                                else
                                {
                                    MyExtraManager.EGapMouse(target,
                                        ComboMenu["FlowersYasuo.ComboMenu.ETurret"].GetValue<MenuBool>().Enabled,
                                        Me.BoundingRadius + Me.AttackRange + target.BoundingRadius - 50, HaveQ3);
                                }
                            }
                        }
                    }

                    if (Q.IsReady())
                    {
                        if (IsMyDashing)
                        {
                            if (ComboMenu["FlowersYasuo.ComboMenu.EQ"].GetValue<MenuBool>().Enabled && !HaveQ3)
                            {
                                if (GameObjects.EnemyHeroes.Any(x => x.IsValidTarget(220f, true, YasuolastEPos)) && Me.Distance(YasuolastEPos) <= 250)
                                {
                                    Q.Cast(Me.PreviousPosition);
                                }
                            }

                            if (ComboMenu["FlowersYasuo.ComboMenu.EQ3"].GetValue<MenuBool>().Enabled && HaveQ3)
                            {
                                if (YasuolastEPos.CountEnemyHeroesInRange(220) > 0 && Me.Distance(YasuolastEPos) <= 250)
                                {
                                    Q.Cast(Me.PreviousPosition);
                                }
                            }
                        }
                        else
                        {
                            if (ComboMenu["FlowersYasuo.ComboMenu.Q"].GetValue<MenuBool>().Enabled && !HaveQ3 &&
                                target.IsValidTarget(Q.Range))
                            {
                                CastQ(target);
                            }

                            if (ComboMenu["FlowersYasuo.ComboMenu.Q3"].GetValue<MenuBool>().Enabled && HaveQ3 &&
                                target.IsValidTarget(Q3.Range))
                            {
                                CastQ3();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.ComboEvent." + ex);
            }
        }

        private static void ComboEQFlashEvent(AIHeroClient target)
        {
            try
            {
                if (FlashSlot == SpellSlot.Unknown || !Flash.IsReady() || !R.IsReady())
                {
                    return;
                }

                if (ComboMenu["FlowersYasuo.ComboMenu.EQFlashKS"].GetValue<MenuBool>().Enabled &&
                    GameObjects.EnemyHeroes.Count(x => x.IsValidTarget(1200) && x.NetworkId != target.NetworkId) <= 2&&
                    GameObjects.AllyHeroes.Count(x => x.IsValidTarget(1200, false) && x.NetworkId != Me.NetworkId) <= 1)
                {
                    if (target.Health + target.HPRegenRate * 2 <
                        Me.GetSpellDamage(target, SpellSlot.Q) +
                        (MyExtraManager.CanCastE(target) ? Me.GetSpellDamage(target, SpellSlot.E) : 0) +
                        Me.GetAutoAttackDamage(target) * 2 + Me.GetSpellDamage(target, SpellSlot.R))
                    {
                        var bestPos = MyExtraManager.FlashPoints().ToArray().FirstOrDefault(x => target.Distance(x) <= 220);

                        if (bestPos != Vector3.Zero && bestPos.CountEnemyHeroesInRange(220) > 0 && Q.Cast(Me.PreviousPosition))
                        { 
                            DelayAction.Add(10 + (Game.Ping / 2 - 5),
                                () =>
                                {
                                    Flash.Cast(bestPos);
                                    YasuolastEQFlashTime = Variables.GameTimeTickCount;
                                });
                        }
                    }
                }

                if (ComboMenu["FlowersYasuo.ComboMenu.EQFlashCount"].GetValue<MenuSliderButton>().Enabled &&
                    GameObjects.EnemyHeroes.Count(x => x.IsValidTarget(1200)) >=
                    ComboMenu["FlowersYasuo.ComboMenu.EQFlashCount"].GetValue<MenuSliderButton>().Value &&
                    GameObjects.AllyHeroes.Count(x => x.IsValidTarget(1200, false) && x.NetworkId != Me.NetworkId) >=
                    ComboMenu["FlowersYasuo.ComboMenu.EQFlashCount"].GetValue<MenuSliderButton>().Value - 1)
                {
                    var bestPos =
                        MyExtraManager.FlashPoints().ToArray()
                            .Where(
                                x =>
                                    GameObjects.EnemyHeroes.Count(a => a.IsValidTarget(600f, true, x)) >=
                                    ComboMenu["FlowersYasuo.ComboMenu.EQFlashCount"].GetValue<MenuSliderButton>().Value)
                            .OrderByDescending(x => GameObjects.EnemyHeroes.Count(i => i.Distance(x) <= 220))
                            .FirstOrDefault();

                    if (bestPos != Vector3.Zero &&
                        bestPos.CountEnemyHeroesInRange(220) >=
                        ComboMenu["FlowersYasuo.ComboMenu.EQFlashCount"].GetValue<MenuSliderButton>().Value && Q.Cast(Me.PreviousPosition))
                    {
                        DelayAction.Add(10 + (Game.Ping / 2 - 5),
                            () =>
                            {
                                Flash.Cast(bestPos);
                                YasuolastEQFlashTime = Variables.GameTimeTickCount;
                            });
                
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.ComboEQFlashEvent." + ex);
            }
        }

        private static void HarassEvent()
        {
            try
            {
                if (Me.IsUnderEnemyTurret())
                {
                    return;
                }

                if (HarassMenu["FlowersYasuo.HarassMenu.Q"].GetValue<MenuBool>().Enabled && Q.IsReady() && !HaveQ3)
                {
                    var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

                    if (target != null && target.IsValidTarget(Q.Range))
                    {
                        CastQ(target);
                    }
                }

                if (HarassMenu["FlowersYasuo.HarassMenu.Q3"].GetValue<MenuBool>().Enabled && Q3.IsReady() && HaveQ3)
                {
                    CastQ3();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.HarassEvent." + ex);
            }
        }

        private static void ClearEvent()
        {
            try
            {
                if (MyManaManager.SpellHarass)
                {
                    HarassEvent();
                }

                if (MyManaManager.SpellFarm)
                {
                    LaneClearEvent();
                    JungleClearEvent();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.ClearEvent." + ex);
            }
        }

        private static void LaneClearEvent()
        {
            try
            {
                if (ClearMenu["FlowersYasuo.ClearMenu.LaneClearTurret"].GetValue<MenuBool>().Enabled && Me.IsUnderEnemyTurret())
                {
                    return;
                }

                var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q3.Range) && x.Health > 5).ToArray();

                if (minions.Any())
                {
                    if (ClearMenu["FlowersYasuo.ClearMenu.LaneClearE"].GetValue<MenuBool>().Enabled && E.IsReady())
                    {
                        foreach (
                            var minion in
                            minions.Where(
                                x =>
                                    x.DistanceToPlayer() <= E.Range && MyExtraManager.CanCastE(x) &&
                                    x.Health <=
                                    (Q.IsReady()
                                        ? Me.GetSpellDamage(x, SpellSlot.Q) + Me.GetSpellDamage(x, SpellSlot.E)
                                        : Me.GetSpellDamage(x, SpellSlot.E))))
                        {
                            if (minion != null && minion.IsValidTarget(E.Range) && 
                                (!ClearMenu["FlowersYasuo.ClearMenu.LaneClearTurret"].GetValue<MenuBool>().Enabled && 
                                !MyExtraManager.UnderTower(MyExtraManager.PosAfterE(minion)) ||
                                MyExtraManager.UnderTower(MyExtraManager.PosAfterE(minion))) && 
                                MyExtraManager.PosAfterE(minion).IsSafePosition())
                            {
                                E.CastOnUnit(minion);
                            }
                        }
                    }

                    if (IsMyDashing)
                    {
                        if (ClearMenu["FlowersYasuo.ClearMenu.LaneClearEQ"].GetValue<MenuBool>().Enabled && 
                            Q.IsReady() && !HaveQ3)
                        {
                            if (minions.Count(x => x.Health > 0 && x.IsValidTarget(220, true, YasuolastEPos)) >= 1)
                            {
                                Q.Cast(Me.PreviousPosition);
                            }
                        }
                    }
                    else
                    {
                        foreach (var minion in minions.Where(x => x.IsValidTarget(Q3.Range)))
                        {
                            if (minion != null && minion.Health > 0)
                            {
                                if (ClearMenu["FlowersYasuo.ClearMenu.LaneClearQ"].GetValue<MenuBool>().Enabled && 
                                    Q.IsReady() && !HaveQ3 && minion.IsValidTarget(Q.Range))
                                {
                                    var pred = Q.GetPrediction(minion);
                                    if (pred.Hitchance >= HitChance.Medium)
                                    {
                                        Q.Cast(pred.CastPosition);
                                    }
                                }

                                if (ClearMenu["FlowersYasuo.ClearMenu.LaneClearQ3"].GetValue<MenuBool>().Enabled && 
                                    Q3.IsReady() && HaveQ3 && minion.IsValidTarget(Q3.Range))
                                {
                                    var pred = Q3.GetPrediction(minion);
                                    if (pred.Hitchance >= HitChance.Medium)
                                    {
                                        Q3.Cast(pred.CastPosition);
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.LaneClearEvent." + ex);
            }
        }

        private static void JungleClearEvent()
        {
            try
            {
                var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(Q3.Range) && 
                                                         x.Health > 5 && x.GetJungleType() != JungleType.Unknown).ToArray();

                if (mobs.Any())
                {
                    var mob = mobs.OrderBy(x => x.MaxHealth).FirstOrDefault();

                    if (mob != null)
                    {
                        if (ClearMenu["FlowersYasuo.ClearMenu.JungleClearE"].GetValue<MenuBool>().Enabled && 
                            E.IsReady() &&  mob.IsValidTarget(E.Range) && MyExtraManager.CanCastE(mob))
                        {
                            E.CastOnUnit(mob);
                        }

                        if (ClearMenu["FlowersYasuo.ClearMenu.JungleClearQ"].GetValue<MenuBool>().Enabled && 
                            Q.IsReady() && !HaveQ3 && mob.IsValidTarget(Q.Range))
                        {
                            var pred = Q.GetPrediction(mob);
                            if (pred.Hitchance >= HitChance.Medium)
                            {
                                Q.Cast(pred.CastPosition);
                            }
                        }

                        if (ClearMenu["FlowersYasuo.ClearMenu.JungleClearQ3"].GetValue<MenuBool>().Enabled && 
                            Q3.IsReady() && HaveQ3 &&
                            mob.IsValidTarget(Q3.Range))
                        {
                            var pred = Q3.GetPrediction(mob);
                            if (pred.Hitchance >= HitChance.Medium)
                            {
                                Q3.Cast(pred.CastPosition);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.JungleClearEvent." + ex);
            }
        }

        private static void LastHitEvent()
        {
            try
            {
                if (IsMyDashing)
                {
                    return;
                }

                var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q3.Range) && x.Health > 5).ToArray();

                if (minions.Any())
                {
                    foreach (var minion in minions)
                    {
                        if (LastHitMenu["FlowersYasuo.LastHitMenu.Q"].GetValue<MenuBool>().Enabled && 
                            !HaveQ3 && Q.IsReady())
                        {
                            if (minion.IsValidTarget(Q.Range)  && minion.Health < Me.GetSpellDamage(minion, SpellSlot.Q))
                            {
                                var pred = Q.GetPrediction(minion);
                                if (pred.Hitchance >= HitChance.Medium)
                                {
                                    Q.Cast(pred.CastPosition);
                                }
                            }
                        }

                        if (LastHitMenu["FlowersYasuo.LastHitMenu.Q3"].GetValue<MenuBool>().Enabled && 
                            HaveQ3 && Q.IsReady())
                        {
                            if (minion.IsValidTarget(Q3.Range) && HaveQ3 &&
                                minion.Health < Me.GetSpellDamage(minion, SpellSlot.Q))
                            {
                                var pred = Q3.GetPrediction(minion);
                                if (pred.Hitchance >= HitChance.Medium)
                                {
                                    Q3.Cast(pred.CastPosition);
                                }
                            }
                        }

                        if (LastHitMenu["FlowersYasuo.LastHitMenu.E"].GetValue<MenuBool>().Enabled && 
                            E.IsReady())
                        {
                            if (minion.IsValidTarget(E.Range) &&
                                minion.Health <
                                Me.GetSpellDamage(minion, SpellSlot.E) +
                                Me.GetSpellDamage(minion, SpellSlot.E) &&
                                MyExtraManager.CanCastE(minion) &&
                                !MyExtraManager.UnderTower(MyExtraManager.PosAfterE(minion)) &&
                                MyExtraManager.PosAfterE(minion).IsSafePosition())
                            {
                                E.CastOnUnit(minion);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.LastHitEvent." + ex);
            }
        }

        private static void OnBuffGain(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
        {
            try
            {
                if (args.Buff == null)
                {
                    return;
                }

                switch (args.Buff.Name.ToLower())
                {
                    case "yasuoe":
                        if (args.Buff.Caster != null && args.Buff.Caster.IsMe)
                        {
                            YasuolastETime = Variables.GameTimeTickCount;
                            isYasuoDashing = true;
                            DelayAction.Add(500 + (Game.Ping / 2), () => { isYasuoDashing = false; });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnBuffGain." + ex);
            }
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            try
            {
                if (sender.IsMe && Args.Target != null && Args.Target.IsEnemy)
                {
                    if (Args.SData.Name == "YasuoEDash")
                    {
                        var target = Args.Target as AIBaseClient;
                        if (target != null && target.IsValidTarget())
                        {
                            YasuolastEPos = MyExtraManager.PosAfterE(target);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnProcessSpellCast." + ex);
            }
        }

        private static void OnPlayAnimation(AIBaseClient sender, AIBaseClientPlayAnimationEventArgs Args)
        {
            try
            {
                if (sender.IsMe)
                {
                    if (Args.Animation == "Spell1_Dash")
                    {
                        Orbwalker.AttackEnabled = false;
                        DelayAction.Add(300 + (Game.Ping / 2 + 10), () =>
                        {
                            Orbwalker.ResetAutoAttackTimer();
                            Me.IssueOrder(GameObjectOrder.MoveTo, Me.Position.Extend(Game.CursorPos, 50));
                            Orbwalker.AttackEnabled = true;
                        });
                    }
                    if (Args.Animation == "Spell3")
                    {
                        YasuolastETime = Variables.GameTimeTickCount;
                        isYasuoDashing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnPlayAnimation." + ex);
            }
        }

        private static void OnAction(OrbwalkerEventArgs Args)
        {
            try
            {
               

                if (Args.Target == null || Args.Target.IsDead || !Args.Target.IsValidTarget() || HaveQ3 || !Q.IsReady())
                {
                    return;
                }

                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Combo:
                        if (ComboMenu["FlowersYasuo.ComboMenu.Q"].GetValue<MenuBool>().Enabled)
                        {
                            var target = Args.Target as AIHeroClient;
                            if (target != null && target.IsValidTarget(Q.Range) && target.Health > 0)
                            {
                                var pred = Q.GetPrediction(target);
                                if (pred.Hitchance >= HitChance.High)
                                {
                                    Q.Cast(pred.CastPosition);
                                }
                            }
                        }
                        break;
                    case OrbwalkerMode.LaneClear:
                        if (ClearMenu["FlowersYasuo.ClearMenu.JungleClearQ"].GetValue<MenuBool>().Enabled && MyManaManager.SpellFarm)
                        {
                            var mob = Args.Target as AIMinionClient;
                            if (mob != null && mob.Team == GameObjectTeam.Neutral && mob.IsValidTarget(Q.Range) && mob.Health > 0)
                            {
                                var pred = Q.GetPrediction(mob);
                                var pred2 = Q3.GetPrediction(mob);
                                if (pred.Hitchance >= HitChance.Medium && pred2.Hitchance >= HitChance.Medium)
                                {
                                    Q.Cast(pred.CastPosition);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnPostAttack." + ex);
            }
        }

        private static void OnRender()
        {
            try
            {
                if (DrawMenu["FlowersYasuo.DrawMenu.Q"].GetValue<MenuBool>().Enabled && 
                    Q.IsReady() && !HaveQ3)
                {
                    CircleRender.Draw(Me.Position, Q.Range, SharpDX.Color.Green, 1);
                }

                if (DrawMenu["FlowersYasuo.DrawMenu.Q3"].GetValue<MenuBool>().Enabled && 
                    Q3.IsReady() && HaveQ3)
                {
                    CircleRender.Draw(Me.Position, Q3.Range, SharpDX.Color.Red, 1);
                }

                if (DrawMenu["FlowersYasuo.DrawMenu.E"].GetValue<MenuBool>().Enabled && 
                    E.IsReady())
                {
                    CircleRender.Draw(Me.Position, E.Range, SharpDX.Color.Blue, 1);
                }

                if (DrawMenu["FlowersYasuo.DrawMenu.R"].GetValue<MenuBool>().Enabled && 
                    R.IsReady())
                {
                    CircleRender.Draw(Me.Position, R.Range, SharpDX.Color.Yellow, 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnRender." + ex);
            }
        }

        public static void CastQ3() //Made by Brian(Valve Sharp)
        {
            try
            {
                var targets = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(1200)).ToArray();
                var castPos = Vector3.Zero;

                if (!targets.Any())
                {
                    return;
                }

                foreach (var pred in
                    targets.Select(i => Q3.GetPrediction(i))
                        .Where(
                            i => i.Hitchance >= HitChance.High ||
                                 i.Hitchance >= HitChance.Medium && i.AoeTargetsHitCount > 1)
                        .OrderByDescending(i => i.AoeTargetsHitCount))
                {
                    castPos = pred.CastPosition;
                    break;
                }

                if (castPos != Vector3.Zero)
                {
                    Q3.Cast(castPos);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.CastQ3." + ex);
            }
        }

        public static void CastQ(AIBaseClient target)
        {
            try
            {
                var qPred = Q.GetPrediction(target);

                if (qPred.Hitchance >= Q.MinHitChance)
                {
                    Q.Cast(qPred.UnitPosition);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.CastQ3." + ex);
            }
        }
    }
}