﻿using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace GridRL {
    public enum MouseArea { Hidden, Sidebar, HoldBox, WearBox, WieldBox, PlayerInv, TileInv, World, Grid, Console}

    public partial class Program : Engine {
        #region Properties

        public static Player player = new Player(0, 0);
        public static ScreenOutput console = new ScreenOutput();
        public static Sidebar sidebar = new Sidebar();
        public static World world = new World();
        public static List<List<int>> AbilityPlacePoints = new List<List<int>>();
        public static int[] MouseCoords = new int[2];
        public static int[] GridClickCoords = new int[2];
        public static int[] PlayerInvClickCoords = new int[2];
        public static int[] TileInvClickCoords = new int[2];
        public static int[] GridHoverCoords = new int[2];
        public static int[] PlayerInvHoverCoords = new int[2];
        public static int[] TileInvHoverCoords= new int[2];
        public static int waitState = 0;
        public static Direction lastDirection = Direction.None;
        public static ColorMatrix grayMatrix = new ColorMatrix(new float[][] { new float[]{1, 0, 0, 0, 0},
                                                                               new float[]{0, 1, 0, 0, 0},
                                                                               new float[]{0, 0, 1, 0, 0},
                                                                               new float[]{0, 0, 0, .25f, 0},
                                                                               new float[]{0, 0, 0, 0, 1} });
        public static ImageAttributes gray = new ImageAttributes();
        public static MouseArea MA = MouseArea.Hidden;
        public static MouseArea LastMA = MouseArea.Hidden;
        public static MouseArea Exception = MouseArea.Hidden;
        public static string HoverString;

        #endregion
        #region Methods

        static void Main() {
            gray.SetColorMatrix(grayMatrix);
            InitializeCreatures();
            InitializeArmors();
            InitializeOrbs();
            InitializeWeapons();
            world.GenerateLevel();
            canvas.Add(console);
            canvas.Add(sidebar);
            world.UpdateVisibles();
            player.Inventory.AddItem(new Weapon(Engine.MasterWeapons[0]));
            player.Inventory.AddItem(new Armor(Engine.MasterArmors[0]));
            Application.Run(new Program());
        }

        public static void GameLoop() {
            turnCount++;
            canvas.Update();
            if(player.Health <= 0) {
                LoseGame = true;
            }
            world.CreaturesToRemove = new List<Creature>();
            world.EffectsToRemove = new List<Effect>();
            world.UpdateVisibles();
            form.Refresh();
        }

        private static void setMouseArea(bool isHovering = false) {
            int y = MouseCoords[0];
            int x = MouseCoords[1];
            if(x < 65 && x >= 0) {
                if(y < 41 && y >= 0) {
                    MA = MouseArea.World;
                }
                else if(y >= 41 && y <= 44) {
                    MA = MouseArea.Console;
                }
                else {
                    MA = MouseArea.Hidden;
                }
            }
            else if(x >= sidebar.CellsX && x < sidebar.CellsX + 11) {
                if(x == 74) {
                    if(y == 18) {
                        MA = MouseArea.HoldBox;
                    }
                    else if(y == 20) {
                        MA = MouseArea.WearBox;
                    }
                    else if(y == 22) {
                        MA = MouseArea.WieldBox;
                    }
                }
                else {
                    MA = MouseArea.Sidebar;
                }
                if(y >= sidebar.CellsY && y < sidebar.CellsY + 2) {
                    MA = MouseArea.PlayerInv;
                    if(isHovering) {
                        PlayerInvHoverCoords[0] = y - sidebar.CellsY;
                        PlayerInvHoverCoords[1] = x - sidebar.CellsX;
                    }
                    else {
                        PlayerInvClickCoords[0] = y - sidebar.CellsY;
                        PlayerInvClickCoords[1] = x - sidebar.CellsX;
                    }
                }
                else if(y >= sidebar.CellsY2 && y < sidebar.CellsY2 + 2) {
                    MA = MouseArea.TileInv;
                    if(isHovering) {
                        TileInvHoverCoords[0] = y - sidebar.CellsY2;
                        TileInvHoverCoords[1] = x - sidebar.CellsX;
                    }
                    else {
                        TileInvClickCoords[0] = y - sidebar.CellsY2;
                        TileInvClickCoords[1] = x - sidebar.CellsX;
                    }
                }
                else if(x >= sidebar.GridX && x < sidebar.GridX + 9 && y >= sidebar.GridY && y < sidebar.GridY + 9)  {
                    MA = MouseArea.Grid;
                    if(isHovering) {
                        GridHoverCoords[0] = (y - sidebar.GridY) / 3;
                        GridHoverCoords[1] = (x - sidebar.GridX) / 3;
                    }
                    else {
                        GridClickCoords[0] = (y - sidebar.GridY) / 3;
                        GridClickCoords[1] = (x - sidebar.GridX) / 3;
                    }
                }
            }
            else {
                MA = MouseArea.Sidebar;
            }
        }
        #endregion
        #region Overrides

        protected override void OnKeyDown(KeyEventArgs e) {
            if(!Engine.start) {
                Engine.start = true;
                canvas.Update();
            }
            else {
                Keys kc = e.KeyCode;
                // Do this if we're waiting for key input. 
                if(waitState == 1) {
                    if(kc == Keys.Up || kc == Keys.Down || kc == Keys.Left || kc == Keys.Right ||
                    kc == Keys.NumPad8 || kc == Keys.NumPad2 || kc == Keys.NumPad4 || kc == Keys.NumPad6 ||
                    kc == Keys.NumPad1 || kc == Keys.NumPad3 || kc == Keys.NumPad9 || kc == Keys.NumPad7) {
                        waitState = 0;
                        Direction d = player.KeyPressToDirection(e);
                        lastDirection = d;
                    }
                    else if(kc == Keys.Escape) {
                        waitState = -1;
                    }
                }
                else if(player.HandleKeyInput(e)) {
                    GameLoop();
                }
                else {
                    waitState = 0;
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            if(!Engine.start) {
                Engine.start = true;
                canvas.Update();
            }
            else {
                MouseCoords[0] = (int)Math.Floor((e.Y - canvas.Y) / 16f);
                MouseCoords[1] = (int)Math.Floor((e.X - canvas.X) / 16f);
                setMouseArea();
                form.Refresh();
                if(waitState == 2) {
                    if(MA == MouseArea.World || MA == MouseArea.Console || MA == MouseArea.Sidebar) {
                        waitState = -1;
                    }
                    else if(MA != LastMA || MA == Exception) {
                        waitState = 0;
                    }
                }
                else if(waitState == 3) {
                    if(MA == MouseArea.World || MA == MouseArea.Console || MA == MouseArea.Sidebar) {
                        waitState = -1;
                    }
                    else if(MA == MouseArea.Grid) {
                        waitState = 0;
                    }
                }
                else if(player.HandleMouseInput(e)) {
                    PlayerInvClickCoords = new int[] { -1, -1 };
                    TileInvClickCoords = new int[] { -1, -1 };
                    MouseCoords = new int[] { -1, -1 };
                    GridClickCoords = new int[] { -1, -1 };
                    Program.MA = MouseArea.Hidden;
                    GameLoop();
                }
                else {
                    form.Refresh();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            MouseCoords[0] = (int)Math.Floor((e.Y - canvas.Y) / 16f);
            MouseCoords[1] = (int)Math.Floor((e.X - canvas.X) / 16f);
            setMouseArea(true);
            if(MA == MouseArea.World) {
                if(world[MouseCoords[0], MouseCoords[1]] != null) {
                    if(world[MouseCoords[0], MouseCoords[1]].Visibility == Vis.Visible) {
                        HoverString = world[MouseCoords[0], MouseCoords[1]].Description;
                    }
                    else {
                        HoverString = "";
                    }
                    foreach(Creature c in world.Creatures) {
                        if(c.CoordY == MouseCoords[0] && c.CoordX == MouseCoords[1]) {
                            HoverString = c.Description;
                            break;
                        }
                    }
                    foreach(Effect ef in world.Effects) {
                        if(ef.CoordY == MouseCoords[0] && ef.CoordX == MouseCoords[1]) {
                            HoverString = ef.Description;
                            break;
                        }
                    }
                    form.Refresh();
                    return;
                }
            }
            else if(MA == MouseArea.PlayerInv || MA == MouseArea.TileInv) {
                int index = PlayerInvHoverCoords[0] * 11 + PlayerInvHoverCoords[1];
                if(player.Inventory.Items[index] != null) {
                    HoverString = player.Inventory.Items[index].Description;
                    form.Refresh();
                    return;
                }
                index = TileInvHoverCoords[0] * 11 + TileInvHoverCoords[1];
                if(world[player.CoordY, player.CoordX].Inventory.Items[index] != null) {
                    HoverString = world[player.CoordY, player.CoordX].Inventory.Items[index].Description;
                    form.Refresh();
                    return;
                }
            }
            else if(MA == MouseArea.Grid) {
                foreach(Ability a in player.Abilities) {
                    for(int y = a.GridY; y < a.GridY + a.GridHeight; ++y) {
                        for(int x = a.GridX; x < a.GridX + a.GridWidth; ++x) {
                            if(GridHoverCoords[0] == y && GridHoverCoords[1] == x) {
                                HoverString = a.Description;
                                form.Refresh();
                                return;
                            }
                        }
                    }
                }
            }
            else if(MA == MouseArea.HoldBox) {
                if(player.HeldItem != null) {
                    HoverString = player.HeldItem.Description;
                    form.Refresh();
                    return;
                }
            }
            else if(MA == MouseArea.WearBox) {
                if(player.WornArmor != null) {
                    HoverString = player.WornArmor.Description;
                    form.Refresh();
                    return;
                }
            }
            else if(MA == MouseArea.WieldBox) {
                if(player.HeldWeapon != null) {
                    HoverString = player.HeldWeapon.Description;
                    form.Refresh();
                    return;
                }
            }
            else {
                HoverString = "";
                form.Refresh();
            }
        }

        #endregion
        #region Utilities

        /// <summary> Shuffles a list. </summary>
        /// <typeparam name="T"> Template type parameter. </typeparam>
        /// <param name="list"> The list to be shuffle. </param>
        public static void Shuffle<T>(List<T> list) {
            int n = list.Count;
            for(int i = 0; i < n; i++) {
                int r = i + (int)(rand.NextDouble() * (n - i));
                T t = list[r];
                list[r] = list[i];
                list[i] = t;
            }
        }

        #endregion
    }
}
