namespace Flowers_Yasuo.MyCommon
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharpDX;

    using EnsoulSharp;

    using Flowers_Yasuo.MyBase;
    using EnsoulSharp.SDK;

    #endregion

    public static class MyExtraManager
    {
        public static IEnumerable<Vector3> FlashPoints()
        {
            var points = new List<Vector3>();

            for (var i = 1; i <= 360; i++)
            {
                var angle = i * 2 * Math.PI / 360;
                var point = new Vector3(ObjectManager.Player.Position.X + 425f * (float)Math.Cos(angle),
                    ObjectManager.Player.Position.Y + 425f * (float)Math.Sin(angle), ObjectManager.Player.Position.Z);

                points.Add(point);
            }

            return points;
        }

        public static bool CanCastR(AIHeroClient target)
        {
            return target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Knockback);
        }

        public static AIBaseClient GetNearObj()
        {
            var pos = Game.CursorPos;
            var obj = new List<AIBaseClient>();

            obj.AddRange(GameObjects.EnemyMinions.Where(x => x.IsValidTarget(475) && x.MaxHealth > 5));
            obj.AddRange(GameObjects.EnemyHeroes.Where(i => i.IsValidTarget(475)));

            return obj.Where(i => CanCastE(i) && pos.Distance(PosAfterE(i)) < ObjectManager.Player.Distance(pos) && IsSafePosition(PosAfterE(i)))
                    .MinOrDefault(i => pos.Distance(PosAfterE(i)));
        }

        public static double GetIgniteDamage(this AIHeroClient source, AIHeroClient target)
        {
            return 50 + 20 * source.Level - target.HPRegenRate / 5 * 3;
        }

        public static bool IsSafePosition(this Vector3 pos)
        {
            // TODO
            return true;
        }

        public static void EGapTarget(AIHeroClient target, bool UnderTurret, float GapcloserDis,
            bool includeChampion = true)
        {
            var dashtargets = new List<AIBaseClient>();
            dashtargets.AddRange(
                GameObjects.EnemyHeroes.Where(
                    x =>
                        !x.IsDead && (includeChampion || x.NetworkId != target.NetworkId) && x.IsValidTarget(475f) &&
                        CanCastE(x)));
            dashtargets.AddRange(
                GameObjects.EnemyMinions.Where(x => x.IsValidTarget(475f) && x.MaxHealth > 5)
                    .Where(CanCastE));

            if (dashtargets.Any())
            {
                var dash = dashtargets.Where(x => x.IsValidTarget(475f) && IsSafePosition(PosAfterE(x)))
                    .OrderBy(x => target.Position.Distance(PosAfterE(x)))
                    .FirstOrDefault();

                if (dash != null && dash.IsValidTarget(475f) && CanCastE(dash) &&
                    target.DistanceToPlayer() >= GapcloserDis &&
                    target.Position.Distance(PosAfterE(dash)) <= target.DistanceToPlayer() &&
                    ObjectManager.Player.IsFacing(dash) && (UnderTurret || !UnderTower(PosAfterE(dash))))
                {
                    MyLogic.E.CastOnUnit(dash);
                }
            }
        }

        public static void EGapMouse(AIHeroClient target, bool UnderTurret, float GapcloserDis,
            bool includeChampion = true)
        {
            if (target.DistanceToPlayer() > (ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius) * 1.2 ||
                target.DistanceToPlayer() >
                (ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + target.BoundingRadius) * 0.8 ||
                Game.CursorPos.DistanceToPlayer() >=
                (ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius) * 1.2)
            {
                var dashtargets = new List<AIBaseClient>();
                dashtargets.AddRange(
                    GameObjects.EnemyHeroes.Where(
                        x =>
                            !x.IsDead && (includeChampion || x.NetworkId != target.NetworkId) && x.IsValidTarget(475) &&
                            CanCastE(x)));
                dashtargets.AddRange(
                    GameObjects.EnemyMinions.Where(x => x.IsValidTarget(475) && x.MaxHealth > 5)
                        .Where(CanCastE));

                if (dashtargets.Any())
                {
                    var dash =
                        dashtargets.Where(x => x.IsValidTarget(475f) && IsSafePosition(PosAfterE(x)))
                            .MinOrDefault(x => PosAfterE(x).Distance(Game.CursorPos));

                    if (dash != null && dash.IsValidTarget(475f) && CanCastE(dash) &&
                        target.DistanceToPlayer() >= GapcloserDis && ObjectManager.Player.IsFacing(dash) &&
                        (UnderTurret || !UnderTower(PosAfterE(dash))))
                    {
                        MyLogic.E.CastOnUnit(dash);
                    }
                }
            }
        }

        public static bool CanMoveMent(this AIBaseClient target)
        {
            return !(target.MoveSpeed < 50) && !target.HasBuffOfType(BuffType.Stun) &&
                   !target.HasBuffOfType(BuffType.Fear) && !target.HasBuffOfType(BuffType.Snare) &&
                   !target.HasBuffOfType(BuffType.Knockup) && !target.HasBuff("recall") &&
                   !target.HasBuffOfType(BuffType.Knockback)
                   && !target.HasBuffOfType(BuffType.Charm) && !target.HasBuffOfType(BuffType.Taunt) &&
                   !target.HasBuffOfType(BuffType.Suppression) &&
                   !target.HasBuff("zhonyasringshield") && !target.HasBuff("bardrstasis");
        }

        public static bool IsUnKillable(this AIBaseClient target)
        {
            if (target == null || target.IsDead || target.Health <= 0)
            {
                return true;
            }

            if (target.HasBuff("KindredRNoDeathBuff"))
            {
                return true;
            }

            if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3f &&
                target.Health <= target.MaxHealth * 0.10f)
            {
                return true;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return true;
            }

            if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3f &&
                target.Health <= target.MaxHealth * 0.10f)
            {
                return true;
            }

            if (target.HasBuff("VladimirSanguinePool"))
            {
                return true;
            }

            if (target.HasBuff("ShroudofDarkness"))
            {
                return true;
            }

            if (target.HasBuff("SivirShield"))
            {
                return true;
            }

            if (target.HasBuff("itemmagekillerveil"))
            {
                return true;
            }

            return target.HasBuff("FioraW");
        }

        public static bool CanCastE(AIBaseClient target)
        {
            return !target.HasBuff("YasuoE");
        }

        public static bool UnderTower(Vector3 pos)
        {
            return pos.IsUnderEnemyTurret();
        }

        public static Vector3 PosAfterE(AIBaseClient target)
        {
            if (target.IsValidTarget())
            {
                return ObjectManager.Player.PreviousPosition.Extend(target.PreviousPosition, 475f);
                //return ObjectManager.Player.IsFacing(target)
                //   ? ObjectManager.Player.PreviousPosition.Extend(target.PreviousPosition, 475f)
                //    : ObjectManager.Player.PreviousPosition.Extend(SpellPrediction.GetPrediction(target, 0.05f).UnitPosition, 475f);

            }

            return Vector3.Zero;
        }
    }
}