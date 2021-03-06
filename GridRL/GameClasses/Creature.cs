﻿using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System;

namespace GridRL {
    /// <summary> Determines how a creature acts. </summary>
    public enum AIType { None, Monster, NPC }

    /// <summary>A base class for all living things.</summary>
    public partial class Creature : Actor {
        #region Constructors

        public Creature(Creature other) : base(other.Image) {
            Health = other.Health;
            Attack = other.Attack;
            Defense = other.Defense;
            DeathMessage = other.DeathMessage;
            Inventory = other.Inventory;
            AI = other.AI;
            Abilities = other.Abilities;
            HeldItem = other.HeldItem;
            HeldWeapon = other.HeldWeapon;
            WornArmor = other.WornArmor;
            IsCollidable = other.IsCollidable;
            Description = other.Description;
            Name = other.Name;
        }

        public Creature(Image image) : base(image) {
            Name = "Dummy Creature";
            Description = "If you can see this, file a bug report for an improperly initialized creature.";
            DeathMessage = "The " + Name + " dies";
            IsCollidable = true;
        }

        /// <summary> A constructor that creates a creature with a given image and x/y coordinates. </summary>
        /// <param name="image"> The image used for this creature's sprite. </param>
        /// <param name="y"> The creature's Y position on the world data. </param>
        /// <param name="x"> The creature's X position on the world data. </param>
        public Creature(Image image, int y, int x) : base(image, y, x) {
            Name = "Dummy Creature";
            Description = "If you can see this, file a bug report for an improperly initialized creature.";
            DeathMessage = "The " + Name + " dies!";
            IsCollidable = true;
        }

        #endregion
        #region Properties

        /// <summary> Creature's HP stat. </summary>
        public int Health { get; set; } = 0;

        /// <summary> Creature's Attack Stat. </summary>
        public int Attack { get; set; } = 1;

        /// <summary> Creature's Base Defense stat. </summary>
        public int BaseDefense { get; set; } =1;

        /// <summary> Creature's modified Defense stat. </summary>
        public int Defense { get; set; } = 0;

        /// <summary> A message to be printed when the creature dies. </summary>
        public string DeathMessage { get; set; }

        /// <summary> The items held by this creature. </summary>
        public Inventory Inventory { get; set; } = new Inventory();

        /// <summary> The kind of behavior followed by the creature. </summary>
        public AIType AI { get; set; } = AIType.Monster;

        /// <summary> The abilities currently known by this monster. </summary>
        public List<Ability> Abilities { get; set; } = new List<Ability>();

        public Weapon HeldWeapon { get; set; }

        public Armor WornArmor { get; set; }

        public Item HeldItem { get; set; }

        #endregion
        #region Methods

        /// <summary> Attempt to add an item to the invetory of this creature. </summary>
        /// <param name="i"> The item being obtained. </param>
        /// <returns> A boolean indicating whether the item was successfully obtained. </returns>
        public bool PickUp(Item i) {
            if(Program.world.Data[CoordY, CoordX].Inventory.RemoveItem(i)) {
                Inventory.AddItem(i);
                Program.console.SetText(Name + " picked up the " + i.Name);
                return true;
            }
            return false;
        }

        /// <summary> Attempt to remove the item held by this creature. </summary>
        /// <param name="i"> The item being dropped. </param>
        /// <returns> A boolean indicating whether the item was successfully dropped. </returns>
        public bool Drop(Item i) {
            if(Program.world.Data[CoordY, CoordX].Inventory.AddItem(i)) {
                Inventory.RemoveItem(i);
                Program.console.SetText(Name + " dropped the " + i.Name);
                return true;
            }
            return false;
        }

        /// <summary> Executes an attack on the given creature. </summary>
        /// <param name="attacked"> The creature to be attacked. </param>
        protected virtual void PerformAttack(Creature attacked) {
            double HitChance = (double)Attack / (double)(2 * attacked.BaseDefense);
            HitChance += GetRandomNumber(-.2, .2);
            int damage = 0;
            if(HeldWeapon != null) {
                damage = (int)Math.Round((1.5 * Attack + HeldWeapon.GetDamage()) / Math.Sqrt(attacked.BaseDefense));
            }
            else {
                damage = (int)Math.Round((1.5 * Attack) / Math.Sqrt(attacked.BaseDefense));
            }
            attacked.OnAttack(this, HitChance, damage +1);
        }

        /// <summary> Called when a creature is attacked. </summary>
        /// <param name="attacker"> The creature attacking this. </param>
        protected virtual void OnAttack(Creature attacker, double chance, int damage) {
            double bar = GetRandomNumber(0,1);
            if(chance > bar) {
                if(damage > 0) {
                    Health -= damage;
                    if(attacker.Visibility == Vis.Visible) {
                        Program.console.SetText(Name + " was hit!");
                    }
                }

                if(Health <= 0) {
                    Remove(this);
                    Program.world.Creatures.Remove(this);
                    if(attacker.Visibility == Vis.Visible) {
                        Program.console.SetText(Name + " was killed!");
                    }
                    Visibility = Vis.Unseen;
                    IsCollidable = false;
                }
            }
        }

        public double GetRandomNumber(double min, double max) {
            Random r = new Random();
            return r.NextDouble() * (max - min) + min;
        }

        private void calculatePositionsFor(Ability a) {
            bool[,] grid = new bool[3, 3];
            foreach(Ability ab in Abilities) {
                for(int y = ab.GridY; y < ab.GridY + ab.GridHeight; ++y) {
                    for(int x = ab.GridX; x < ab.GridX + ab.GridWidth; ++x) {
                        grid[y, x] = true;
                    }
                }
            }
            for(int y = 0; y < 4 - a.GridHeight; ++y) {
                for(int x = 0; x < 4 - a.GridWidth; ++x) {
                    bool canUseSquare = true;
                    for(int j = 0; j < a.GridHeight; ++j) {
                        for(int i = 0; i < a.GridWidth; ++i) {
                            if(grid[y + j, x + i] == true) {
                                canUseSquare = false;
                            }
                        }
                    }
                    if(canUseSquare) {
                        Program.AbilityPlacePoints.Add(new List<int>(new int[] { y, x }));
                    }
                }
            }
        }

        public virtual bool AddNewAbility(Ability a) {
            int y = a.GridY;
            int x = a.GridX;
            int w = a.GridWidth;
            int h = a.GridHeight;
            Program.waitState = 3;
            calculatePositionsFor(a);
            if(Program.AbilityPlacePoints.Count == 0) {
                Program.console.SetText("The orb's glow fades");
                return false;
            }
            while(Program.waitState == 3) {
                Application.DoEvents();
                Engine.form.Refresh();
            }
            if(Program.waitState == -1) {
                Program.console.SetText("The orb's glow fades");
                return false;
            }
            foreach(List<int> points in Program.AbilityPlacePoints) {
                if(Program.GridClickCoords[0] == points[0] && Program.GridClickCoords[1] == points[1]) {
                    a.GridY = Program.GridClickCoords[0];
                    a.GridX = Program.GridClickCoords[1];
                    Program.player.Abilities.Add(a);
                    Program.console.SetText("The orb vanishes in a flash of light!");
                    Program.console.SetText("You gain the skill of " + a.Name + "!");
                    Program.AbilityPlacePoints = new List<List<int>>();
                    a.OnAddToGrid();
                    return true;
                }
            }
            Program.console.SetText("The orb's glow fades.");
            Program.AbilityPlacePoints = new List<List<int>>();
            return true;
        }

        #endregion
        #region Overrides

        /// <summary> Makes decisions based on AI. </summary>
        protected override void Act() {
            base.Act();
            if(AI == AIType.Monster) {
                FollowPlayer();
            }
            if(WornArmor != null)Defense = BaseDefense + WornArmor.Defense;
        }

        #endregion
    }
}
