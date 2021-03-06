﻿using System.Windows.Forms;
using System.Collections.Generic;

namespace GridRL {
    public class FlameBurst : DirectedAbility {
        #region Constructors

        public FlameBurst() : base() {
            Name = "Flame Burst";
            Description = "Blasts a small jet of flame from your hand.";
            GridHeight = 1;
            GridWidth = 2;
            Effect = new FlameBurstEffect();
        }

        #endregion
        #region Overrides

        public override void CreateEffect(Creature user, Direction dir) {
            List<int> points = user.DirectionToPoints(dir);
            Effect.CoordY = points[0];
            Effect.CoordX = points[1];
            Effect.Activate(Effect.CoordY, Effect.CoordX);
            Program.world.Effects.Add(Effect);
        }

        #endregion
    }

    public class AttackBoost : PassiveAbility {
        #region Constructors

        public AttackBoost(Creature owner) : base(owner) {
            Name = "Attack Boost";
            Description = "A passive boost to your attack.";
            GridHeight = 1;
            GridWidth = 1;
        }

        #endregion
        #region Overrides

        public override void OnAddToGrid() {
            Owner.Attack += 2;
        }

        public override void OnRemoveFromGrid() {
            Owner.Attack -= 2;
        }

        #endregion
    }

    public class FireWall : DirectedAbility {
        #region Constructors

        public FireWall() : base() {
            Name = "Fire Wall";
            Description = "Creates a wall of pure flame.";
            GridHeight = 2;
            GridWidth = 1;
            Effect = new FireWallEffect();
        }
        #endregion

        public override void CreateEffect(Creature user, Direction dir) {
            List<int> points = user.DirectionToPoints(dir);
            Effect.CoordY = points[0];
            Effect.CoordX = points[1];
            bool horizontal = false;
            if(dir == Direction.Up || dir == Direction.Down) {
                horizontal = true;
            }
            else {
                horizontal = false;
            }
            Effect.Activate(Effect.CoordY, Effect.CoordX, horizontal);
            Program.world.Effects.Add(Effect);
        }
    }
}