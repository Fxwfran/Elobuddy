using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;


namespace AddonTemplate
{

    class Program
    {
        private static AIHeroClient Player { get { return ObjectManager.Player; } }

        private static Menu RootMenu, ComboMenu, HarassMenu, FarmingMenu, ksMenu, DrawingsMenu, interruptMenu;

        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Targeted R;

        static void Main(string[] args)
        {

            Loading.OnLoadingComplete += Loading_OnLoadingComplete;

        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 800, SkillShotType.Circular, 550, int.MaxValue, 125)
            {
                AllowedCollisionCount = int.MaxValue
            };
            W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular, 350, 1500, 130)
            {
                AllowedCollisionCount = int.MaxValue
            };
            E = new Spell.Skillshot(SpellSlot.E, 700, SkillShotType.Cone, 250, 2500, 50)
            {
                AllowedCollisionCount = int.MaxValue
            };
            R = new Spell.Targeted(SpellSlot.R, 700);
                        
            RootMenu = MainMenu.AddMenu("Roura - Syndra", "Roura - Syndra");

            ComboMenu = RootMenu.AddSubMenu("Combo", "Combo");

            ComboMenu.Add("UseQ", new CheckBox("Use Q"));
            ComboMenu.Add("UseW", new CheckBox("Use W"));
            ComboMenu.Add("UseE", new CheckBox("Use E to stun"));
            ComboMenu.Add("UseR", new CheckBox("Use R to penetrate enemy anus"));

            HarassMenu = RootMenu.AddSubMenu("Harass", "Harass");

            HarassMenu.Add("UseQ", new CheckBox("Use Q"));
            HarassMenu.Add("UseW", new CheckBox("Use W"));
            HarassMenu.Add("UseE", new CheckBox("Use E (Though it's fucking idiotic to stun on harass)"));




            FarmingMenu = RootMenu.AddSubMenu("Farming", "farming");

            FarmingMenu.Add("Qclear", new CheckBox("Use Q to clear wave"));
            FarmingMenu.Add("Eclear", new CheckBox("Use E to clear wave"));
            FarmingMenu.Add("Qclearmana", new Slider("Q mana to clear %", 30, 0, 100));
            FarmingMenu.Add("Wclearmana", new Slider("W mana to last hit %", 30, 0, 100));


            DrawingsMenu = RootMenu.AddSubMenu("Drawings", "Drawings");

            DrawingsMenu.Add("DrawQ", new CheckBox("Draw Q range"));
            DrawingsMenu.Add("DrawW", new CheckBox("Draw W range"));
            DrawingsMenu.Add("DrawE", new CheckBox("Draw E range"));
            DrawingsMenu.Add("DrawE", new CheckBox("Draw R range"));
            DrawingsMenu.Add("DrawQpred", new CheckBox("Draw Q prediction"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            
        }

        private static void Game_OnTick(EventArgs args)
        {
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Combo();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    Harass();
                    break;
                case Orbwalker.ActiveModes.LaneClear:
                    LaneClear();
                    break;
                case Orbwalker.ActiveModes.LastHit:
                    break;
                case Orbwalker.ActiveModes.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();

            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (DrawingsMenu["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.Cyan, Q.Range, Player);
            }
            if (DrawingsMenu["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.Cyan, W.Range, Player);
            }
            if (DrawingsMenu["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.Cyan, E.Range, Player);
            }
            if (DrawingsMenu["DrawQpred"].Cast<CheckBox>().CurrentValue)
            {
                if (target == null)
                    return;
                Drawing.DrawCircle(Q.GetPrediction(target).CastPosition, 150, System.Drawing.Color.Cyan);

            }
            if (DrawingsMenu["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.Cyan, R.Range, Player);
            }
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null)
                return;
            if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(ObjectManager.Player) <= Q.Range && Q.IsReady() && !(E.IsReady()))
                {

                    if (Q.GetPrediction(target).HitChance >= HitChance.High)
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }


                }
            }
            if (ComboMenu["UseW"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(ObjectManager.Player) <= W.Range && W.IsReady())
                {

                    W.Cast(W.GetPrediction(target).CastPosition);



                }
            }
            if (ComboMenu["UseE"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(ObjectManager.Player) <= E.Range && E.IsReady())
                {
                    var pred = Q.GetPrediction(target);
                    Q.Cast(Player.Position.Extend(pred.CastPosition, E.Range - 10).To3D());
                    E.Cast(Player.Position.Extend(pred.CastPosition, E.Range - 10).To3D());
                    
                }
            }

            if (ComboMenu["UseR"].Cast<CheckBox>().CurrentValue)
            {
                if (R.IsReady() && target.IsValidTarget(R.Range) && Prediction.Health.GetPrediction(target, R.CastDelay) <= R.GetSpellDamage(target))
                {
                    R.Cast(target);
                }

            }


        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null)
                return;
            if (HarassMenu["UseQ"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(ObjectManager.Player) <= Q.Range && Q.IsReady())
                {

                    if (Q.GetPrediction(target).HitChance >= HitChance.High)
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }


                }
            }
            if (HarassMenu["UseE"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(ObjectManager.Player) <= E.Range && E.IsReady())
                {

                    E.Cast();

                }
            }
            if (HarassMenu["UseR"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(ObjectManager.Player) <= R.Range && R.IsReady())
                {
                    R.Cast(target);
                }
            }



        }

        private static void LaneClear()
        {
            if (FarmingMenu["Qclearmana"].Cast<Slider>().CurrentValue <= Player.ManaPercent && FarmingMenu["Qclear"].Cast<CheckBox>().CurrentValue)
            {
                var minions = EntityManager.MinionsAndMonsters.EnemyMinions.Where(t => t.IsEnemy && !t.IsDead && t.IsValid && !t.IsInvulnerable && t.IsInRange(Player.Position, Q.Range));
                foreach (var m in minions)
                {
                    if (Q.GetPrediction(m).CollisionObjects.Where(t => t.IsEnemy && !t.IsDead && t.IsValid && !t.IsInvulnerable).Count() >= 3)
                    {
                        Q.Cast(m);
                        break;
                    }
                }
                }
            if (FarmingMenu["Eclearmana"].Cast<Slider>().CurrentValue <= Player.ManaPercent && FarmingMenu["Eclear"].Cast<CheckBox>().CurrentValue)
            {
                var minions = EntityManager.MinionsAndMonsters.EnemyMinions.Where(t => t.IsEnemy && !t.IsDead && t.IsValid && !t.IsInvulnerable && t.IsInRange(Player.Position, Q.Range));
                foreach (var m in minions)
                {
                    if (m.Distance(Player) <= E.Range)
                    {
                        E.Cast();
                    }
                }
            }

            }

        

        }
    }
