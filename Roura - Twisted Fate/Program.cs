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
using CardSelect;


namespace AddonTemplate
{

    class Program
    {
        private static AIHeroClient Player { get { return ObjectManager.Player; } }

        private static Menu RootMenu, ComboMenu, HarassMenu, FarmingMenu, ksMenu, DrawingsMenu, interruptMenu;

        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Active R;

        static void Main(string[] args)
        {
            Chat.Print("<font color='#881df2'>Roura's Twisted Fate</font> Loaded.");
            Chat.Print("By <fontcolor='#881df2'>fxwfran</font>");
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 1450, SkillShotType.Linear, 0, 1000, 40) { AllowedCollisionCount = int.MaxValue };
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Active(SpellSlot.R, 5500);

            RootMenu = MainMenu.AddMenu("Roura - Twisted Fate", "Roura - Twisted Fate");

            ComboMenu = RootMenu.AddSubMenu("Combo", "Combo");

            ComboMenu.Add("UseQ", new CheckBox("Use Q"));
            ComboMenu.Add("UseQStun", new CheckBox("Only on stun"));
            ComboMenu.Add("Mana", new KeyBind("Blue card", false, KeyBind.BindTypes.HoldActive, 'E'));
            ComboMenu.Add("Stun", new KeyBind("Stun card", false, KeyBind.BindTypes.HoldActive, 'W'));
            ComboMenu.Add("Red", new KeyBind("Red card", false, KeyBind.BindTypes.HoldActive, 'T'));

            HarassMenu = RootMenu.AddSubMenu("Harass", "Harass");

            HarassMenu.Add("UseQ", new CheckBox("Use Q"));
            HarassMenu.Add("UseQStun", new CheckBox("Only on stun"));

            DrawingsMenu = RootMenu.AddSubMenu("Drawings", "Drawings");

            DrawingsMenu.Add("DrawQ", new CheckBox("Draw Q range"));
            DrawingsMenu.Add("DrawE", new CheckBox("Draw R range"));
            DrawingsMenu.Add("DrawQpred", new CheckBox("Draw Q prediction"));

            //Game.OnTick += Game_OnTick;
            Game.OnUpdate += Game_OnGameUpdate;
            //Drawing.OnDraw += Drawing_OnDraw;

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Cardcheck();
            Stuncheck();
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Combo();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    Harass();
                    break;
                case Orbwalker.ActiveModes.LaneClear:
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
            if (DrawingsMenu["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.Cyan, R.Range, Player);
            }
        }

        private static void Stuncheck()
        {
            foreach (var dude in EntityManager.Enemies.Where(dude => dude.Distance(Player) < Q.Range && dude.HasBuffOfType(BuffType.Stun)))
            {
                if(dude.IsAlive() && dude.Type == GameObjectType.AIHeroClient)
                    Q.Cast(dude);
            }
        }

        private static void Cardcheck()
        {
            if (ComboMenu["Stun"].Cast<KeyBind>().CurrentValue)
                CardSelect.CardSelect.SelectCard(Player, Cards.Yellow);
            if (ComboMenu["Mana"].Cast<KeyBind>().CurrentValue)
                CardSelect.CardSelect.SelectCard(Player, Cards.Blue);
            if (ComboMenu["Red"].Cast<KeyBind>().CurrentValue)
                CardSelect.CardSelect.SelectCard(Player, Cards.Red);
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            //CardSelect.SelectCard(Player, Cards.Yellow);

            if (target == null)
                return;
            if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && ComboMenu["UseQStun"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(ObjectManager.Player) <= Q.Range && Q.IsReady() && !(E.IsReady()) && target.HasBuffOfType(BuffType.Stun))
                {
                    if (Q.GetPrediction(target).HitChance >= HitChance.High && target.IsAlive() && target.Type == GameObjectType.AIHeroClient)
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }
                }
            }else if(ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue)
            {
                if (Q.GetPrediction(target).HitChance >= HitChance.High && target.IsAlive() && target.Type == GameObjectType.AIHeroClient)
                {
                    Q.Cast(Q.GetPrediction(target).CastPosition);
                }
            }
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            //CardSelect.SelectCard(Player, Cards.Blue);
            if (target == null)
                return;
            if (HarassMenu["UseQ"].Cast<CheckBox>().CurrentValue && HarassMenu["UseQStun"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(ObjectManager.Player) <= Q.Range && Q.IsReady() && !(E.IsReady()) && target.HasBuffOfType(BuffType.Stun))
                {
                    if (Q.GetPrediction(target).HitChance >= HitChance.High && target.IsAlive() && target.Type == GameObjectType.AIHeroClient)
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }
                }
            }
            else if (HarassMenu["UseQ"].Cast<CheckBox>().CurrentValue)
            {
                if (Q.GetPrediction(target).HitChance >= HitChance.High && target.IsAlive() && target.Type == GameObjectType.AIHeroClient)
                {
                    Q.Cast(Q.GetPrediction(target).CastPosition);
                }
            }
        }
    }
    }
