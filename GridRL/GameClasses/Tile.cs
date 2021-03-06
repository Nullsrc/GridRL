﻿using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GridRL {
    /// <summary> The basic definition of tiles, which make up a world. </summary>
    public class Tile : Actor {
        #region Constructors

        /// <summary> Constructs a tile with the given image and position. </summary>
        /// <param name="image">The image used for this tile's sprite. </param>
        /// <param name="y"> The vertical position of the tile in the world data. </param>
        /// <param name="x"> The horizontal position of the tile in the world data. </param>
        public Tile(Image image, int y, int x) : base(image, y, x) {
            Name = "Dummy Tile";
            Description = "If you can see this, file a bug report for an improperly initialized tile.";
        }

        #endregion
        #region Properties

        /// <summary> Property representing whether creatures and items can be on the tile. </summary>
        public bool IsWalkable { get; set; } = false;

        /// <summary> Identifies tiles connected to each other. Used during mapgen. </summary>
        public int Region { get; set; } = -1;

        /// <summary> The items located on this tile. </summary>
        public Inventory Inventory { get; set; } = new Inventory();

        public bool BlocksLight { get; set; } = false;

        #endregion
        #region Methods

        /// <summary> Called when a tile is stepped on. </summary>
        /// <param name="s">The sprite that stepped on this tile. </param>
        public virtual void OnStepOn(Actor a) {
            List<Item> items = new List<Item>();
            foreach(Item i in Inventory.Items) {
                if(i != null) {
                    items.Add(i);
                }
            }
            if(items.Count >= 1 && a.Visibility == Vis.Visible) {
                Program.console.SetText("There is a " + items[0].Name + " here.");
                if(items.Count > 1) {
                    Program.console.SetText("There are other items here as well.");
                }
            }
        }

        #endregion
        #region Overrides

        protected override void Paint(Graphics g) {
            base.Paint(g);
            if(Visibility == Vis.Visible) {
                Item i = Inventory.Items.FirstOrDefault();
                if(i != null) {
                    i.Visibility = Vis.Visible;
                    i.Render(g);
                }
            }
        }

        #endregion
    }
}
