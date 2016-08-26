using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI;
using EvoSim;
using TakaGUI.DrawBoxes;
using EvoApp.Menu.TemplatesMenu;
using TakaGUI.DrawBoxes.Forms;
using EvoApp.DrawBoxes;
using EvoApp.Menu.TemplatesMenu.CreatureTemplateForms;
using EvoApp.Menu.TemplatesMenu.GenomeTemplateForms;
using EvoApp.Menu.TemplatesMenu.AnnTemplateForms;
using EvoApp.Menu.TemplatesMenu.WorldTemplateForms;
using EvoApp.Menu.TemplatesMenu.EntityTemplateForms;
using EvoApp.Menu.EntitiesMenu;
using System.IO;
using EvoApp.Menu.SimulationMenu;
using EvoApp.Menu.WorldMenu;
using EvoApp.Menu.FileMenu;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace EvoApp.Menu
{
	public class Editor : SingleSlotBox
	{
		public delegate IEntity GetEntityFunctionDelegate();
		public delegate void SendPeremiterFunctionDelegate(Rectangle peremiter);

		public enum Modes { None, AddEntity, SetPeremiter }

		Simulation _simulation;
		public Simulation Simulation
		{
			get { return _simulation; }
			set
			{
				_simulation = value;
				loadEntityPaintersToWorldBox();
			}
		}

		public VerticalSplitPanel VSplit;

		public MenuBar MenuBar;
		public WorldBox WorldBox;

		public Modes Mode;
		GetEntityFunctionDelegate GetEntityFunction;
		Point p1, p2;
		SendPeremiterFunctionDelegate SendPeremiterFunction;

		public void Set_GetEntityFunction(GetEntityFunctionDelegate f)
		{
			GetEntityFunction = null;
			GetEntityFunction += f;
		}
		public void Set_SendPeremiterFunction(SendPeremiterFunctionDelegate f)
		{
			SendPeremiterFunction = null;
			SendPeremiterFunction += f;

			p1.X = (int)Math.Round((MouseInput.X - WorldBox.RealX + WorldBox.ViewCamera.LookX) / WorldBox.ViewCamera.Zoom, 0);
			p1.Y = (int)Math.Round((MouseInput.Y - WorldBox.RealY + WorldBox.ViewCamera.LookY) / WorldBox.ViewCamera.Zoom, 0);
			p2 = p1;
		}

		public void Initialize()
		{
			base.BaseInitialize();
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			FillWidth();
			FillHeight();

			setUpMenuBar();

			VSplit = new VerticalSplitPanel();
			VSplit.Initialize();
			AddDrawBox(VSplit);
			VSplit.Y = MenuBar.Y + MenuBar.Height;
			VSplit.FillWidth();
			VSplit.Height = GetDefaultBoundaries().Height - VSplit.Y;

			setUpWorldBox();
		}

		void setUpMenuBar()
		{
			MenuBar = new MenuBar();
			MenuBar.Initialize();
			AddDrawBox(MenuBar);
			MenuBar.FillWidth();
			MenuBar.Height = 40;

			setUpFileMenuItem();
			setUpEditMenuItem();
			setUpSimulationMenuItem();
			setUpWorldMenuItem();
			setUpEntitiesMenuItem();
			setUpTemplatesMenuItem();
			setUpToolsMenuItem();
			setUpHelpMenuItem();
		}
		//File
		void setUpFileMenuItem()
		{
			var menuItem = new MenuBar.MenuItem("File");
			MenuBar.MenuItems.Add(menuItem);

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("New Simulation", delegate(object sender)
			{
				if (Simulation != null)
				{
					YesNoAbortForm.ShowDialogue(Parent, null, "Do you want to save the current simulation?", delegate(object _sender)
					{
						var dialogue = (TakaGUI.DrawBoxes.Forms.YesNoAbortForm)_sender;

						if (dialogue.DialogResult == DialogResult.Yes)
							Simulation.Save();

						if (dialogue.DialogResult != DialogResult.Abort)
							showNewSimulationForm();
					});
				}
				else
				{
					showNewSimulationForm();
				}
			}));

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Open Simulation", delegate(object sender)
			{
				if (Simulation != null)
				{
					YesNoAbortForm.ShowDialogue(Parent, null, "Do you want to save the current simulation?", delegate(object _sender)
					{
						var dialogue = (TakaGUI.DrawBoxes.Forms.YesNoAbortForm)_sender;

						if (dialogue.DialogResult == DialogResult.Yes)
							Simulation.Save();

						if (dialogue.DialogResult != DialogResult.Abort)
							showOpenSimulationForm();
					});
				}
				else
				{
					showOpenSimulationForm();
				}
			}));

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Save Simulation", delegate(object sender)
			{
				if (Simulation != null)
					Simulation.Save();
			}));

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Save Simulation As", delegate(object sender)
			{
				if (Simulation == null)
					return;

				FileForm.ShowDialogue(Parent, FileForm.FileFormTypes.File, FileForm.OperationTypes.Save, delegate(object _sender)
				{
					var fileForm = (FileForm)_sender;

					if (fileForm.Result == DialogResult.OK)
					{
						Simulation.Save(Path.GetFileNameWithoutExtension(fileForm.SaveDirectory));
					}
				});

				if (Simulation != null)
					Simulation.Save();
			}));

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Exit Program", delegate(object sender)
			{
				if (Simulation != null)
				{
					YesNoAbortForm.ShowDialogue(Parent, null, "Do you want to save the current simulation?", delegate(object _sender)
					{
						var dialogue = (TakaGUI.DrawBoxes.Forms.YesNoAbortForm)_sender;

						if (dialogue.DialogResult == DialogResult.Yes)
							Simulation.Save();

						Globals.Game.Exit();
					});
				}
				else
				{
					Globals.Game.Exit();
				}
			}));
		}
		void showNewSimulationForm()
		{
			NewSimulationForm.ShowDialogue(Parent, delegate(object sender)
			{
				var dialogue = (NewSimulationForm)sender;
				if (dialogue.Result != null)
				{
					Simulation = dialogue.Result;

					WorldBox.World = Simulation.World;
				}
			});
		}
		void showOpenSimulationForm()
		{
			FileForm.ShowDialogue(Parent, FileForm.FileFormTypes.File, FileForm.OperationTypes.Open, delegate(object sender)
			{
				var fileForm = (FileForm)sender;

				if (fileForm.Result == DialogResult.OK)
				{
					Simulation = Simulation.Load(fileForm.SelectedFile.FullName);
					WorldBox.World = Simulation.World;
				}
			});
		}
		//Edit
		void setUpEditMenuItem()
		{
			var menuItem = new MenuBar.MenuItem("Edit");
			MenuBar.MenuItems.Add(menuItem);
		}
		//Simulation
		void setUpSimulationMenuItem()
		{
			var menuItem = new MenuBar.MenuItem("Simulation");
			MenuBar.MenuItems.Add(menuItem);

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Entity Drawing-Options", delegate(object sender)
			{
				EntityDrawingOptionsForm.ShowDialogue(Parent, Simulation, delegate(object _sender)
				{
					loadEntityPaintersToWorldBox();
				});
			}));
		}
		//World
		void setUpWorldMenuItem()
		{
			var menuItem = new MenuBar.MenuItem("World");
			MenuBar.MenuItems.Add(menuItem);

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Edit World", delegate(object sender)
			{
				EditWorldForm.ShowDialogue(Parent, Simulation.World);
			}));

			menuItem.Elements.Add(MenuBar.MenuItem.MenuElement.GetSeparator());

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Manage Spawnpoints", delegate(object sender)
			{
				ManageSpawnPointsForm.ShowDialogue(Parent, Simulation.World.SpawnPointList, Simulation.World);
			}));
		}
		//Entities
		void setUpEntitiesMenuItem()
		{
			var menuItem = new MenuBar.MenuItem("Entities");
			MenuBar.MenuItems.Add(menuItem);

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Add Entity", delegate(object sender)
			{
				AddEntitiesForm.ShowDialogue(Parent, Simulation.World);
			}));
			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Edit Entity", delegate(object sender)
			{
				if (WorldBox.SelectedEntity == null)
					return;

				if (typeof(ICreature).IsAssignableFrom(WorldBox.SelectedEntity.GetType()))
					return;

				EditEntityForm.ShowDialogue(Parent, WorldBox.SelectedEntity);
			}));
			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Delete Entity", delegate(object sender)
			{
				if (WorldBox.SelectedEntity == null)
					return;

				if (typeof(ICreature).IsAssignableFrom(WorldBox.SelectedEntity.GetType()))
					return;

				Simulation.World.RemoveEntity(WorldBox.SelectedEntity);
			}));
			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Delete All Entities", delegate(object sender)
			{
				YesNoForm.ShowDialogue(Parent, null, "Do you really want to remove all entities?", delegate(object _sender)
				{
					var form = (YesNoForm)_sender;

					if (form.DialogResult == DialogResult.Yes)
					{
						var creatureType = typeof(ICreature);
						foreach (var e in new List<IEntity>(WorldBox.World.EntityList))
						{
							var type = e.GetType();

							if (!creatureType.IsAssignableFrom(type))
								WorldBox.World.RemoveEntity(e);
						}
					}
				});
			}));

			menuItem.Elements.Add(MenuBar.MenuItem.MenuElement.GetSeparator());

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Add Creatures", delegate(object sender)
			{
				AddCreaturesForm.ShowDialogue(Parent, Simulation.World);
			}));
			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Edit Creature", delegate(object sender)
			{
				if (WorldBox.SelectedEntity == null)
					return;

				if (typeof(ICreature).IsAssignableFrom(WorldBox.SelectedEntity.GetType()))
					EditCreatureForm.ShowDialogue(Parent, (ICreature)WorldBox.SelectedEntity);
			}));
			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Delete Creature", delegate(object sender)
			{
				if (WorldBox.SelectedEntity == null)
					return;

				if (typeof(ICreature).IsAssignableFrom(WorldBox.SelectedEntity.GetType()))
					Simulation.World.RemoveEntity(WorldBox.SelectedEntity);
			}));
			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Delete All Creatures", delegate(object sender)
			{
				YesNoForm.ShowDialogue(Parent, null, "Do you really want to remove all creatures?", delegate(object _sender)
				{
					var form = (YesNoForm)_sender;

					if (form.DialogResult == DialogResult.Yes)
					{
						var creatureType = typeof(ICreature);
						foreach (var e in new List<IEntity>(WorldBox.World.EntityList))
						{
							var type = e.GetType();

							if (creatureType.IsAssignableFrom(type))
								WorldBox.World.RemoveEntity(e);
						}
					}
				});
			}));
		}
		//Templates
		void setUpTemplatesMenuItem()
		{
			var menuItem = new MenuBar.MenuItem("Templates");
			MenuBar.MenuItems.Add(menuItem);

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Manage World-Templates", delegate(object sender)
			{
				ManageWorldTemplatesForm.ShowDialogue(Parent);
			}));

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Manage Entity-Templates", delegate(object sender)
			{
				ManageEntityTemplatesForm.ShowDialogue(Parent);
			}));

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Manage Creature-Templates", delegate(object sender)
			{
				ManageCreatureTemplatesForm.ShowDialogue(Parent);
			}));

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Manage Genome-Templates", delegate(object sender)
			{
				ManageGenomeTemplatesForm.ShowDialogue(Parent);
			}));

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("Manage ANN-Templates", delegate(object sender)
			{
				ManageAnnTemplatesForm.ShowDialogue(Parent);
			}));
		}
		//Tools
		void setUpToolsMenuItem()
		{
			var menuItem = new MenuBar.MenuItem("Tools");
			MenuBar.MenuItems.Add(menuItem);
		}
		//Help
		void setUpHelpMenuItem()
		{
			var menuItem = new MenuBar.MenuItem("Help");
			MenuBar.MenuItems.Add(menuItem);

			menuItem.Elements.Add(new MenuBar.MenuItem.MenuElement("About EvoSim", delegate(object sender)
			{
				Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

				AlertForm.ShowDialogue(Parent, "About",
					"EvoSim" + "\n" +
					"Version: " + v.ToString() + "\n" +
					"Created by: Lukas Kikuchi" + "\n" +
					"\n" +
					"Copyright 2011 Lukas Kikuchi");
			}));
		}

		void setUpWorldBox()
		{
			WorldBox = new WorldBox();
			WorldBox.Initialize();
			VSplit.RightPanel.AddDrawBox(WorldBox);

			VSplit.UpdateSize();

			WorldBox.Fill();
		}

		void loadEntityPaintersToWorldBox()
		{
			var entityTypeList = Globals.GetAllTypesDeriving(typeof(IEntity), Assembly.GetExecutingAssembly());

			WorldBox.EntityPaintersDictionary.Clear();
			foreach (var painter in Simulation.EntityPainters)
			{
				foreach (var entityType in painter.EntityTypeList)
				{
					foreach (var t in entityTypeList)
					{
						if (entityType.IsAssignableFrom(t))
						{
							if (!WorldBox.EntityPaintersDictionary.ContainsKey(t))
								WorldBox.EntityPaintersDictionary.Add(t, new List<EntityPainters.IEntityPainter>());

							if (!WorldBox.EntityPaintersDictionary[t].Contains(painter))
								WorldBox.EntityPaintersDictionary[t].Add(painter);
						}
					}
				}
			}
		}

		public override void Idle(Microsoft.Xna.Framework.GameTime gameTime)
		{
			base.Idle(gameTime);

			if (Simulation == null)
			{
				MenuBar.GetItem("Edit").Enabled = false;
				MenuBar.GetItem("Simulation").Enabled = false;
				MenuBar.GetItem("Entities").Enabled = false;
				MenuBar.GetItem("World").Enabled = false;
			}
			else
			{
				MenuBar.GetItem("Edit").Enabled = true;
				MenuBar.GetItem("Simulation").Enabled = true;
				MenuBar.GetItem("Entities").Enabled = true;
				MenuBar.GetItem("World").Enabled = true;
			}

			if (Mode == Modes.AddEntity)
			{
				if (WorldBox.IsUnderMouse && MouseInput.IsClicked(TakaGUI.Services.MouseButtons.Left))
				{
					var entity = GetEntityFunction();

					var pos = new EntityPosition();
					pos.X = (MouseInput.X - WorldBox.RealX + WorldBox.ViewCamera.LookX) / WorldBox.ViewCamera.Zoom;
					pos.Y = (MouseInput.Y - WorldBox.RealY + WorldBox.ViewCamera.LookY) / WorldBox.ViewCamera.Zoom;
					entity.Position = pos;

					WorldBox.World.AddEntity(entity);
				}
				else if (WorldBox.IsUnderMouse && MouseInput.IsClicked(TakaGUI.Services.MouseButtons.Right))
				{
					Mode = Modes.None;
				}
			}
			else if (Mode == Modes.SetPeremiter)
			{
				if (MouseInput.IsClicked(TakaGUI.Services.MouseButtons.Left))
				{
					var c = p1;
					p1 = p2;
					p2 = c;
				}
				else if (MouseInput.IsClicked(TakaGUI.Services.MouseButtons.Right))
				{
					Mode = Modes.None;
					var peremiter = new Rectangle();
					peremiter.X = Math.Min(p1.X, p2.X);
					peremiter.Y = Math.Min(p1.Y, p2.Y);
					peremiter.Width = Math.Max(p1.X, p2.X) - peremiter.X;
					peremiter.Height = Math.Max(p1.Y, p2.Y) - peremiter.Y;

					SendPeremiterFunction(peremiter);
				}

				p1 = new Point(MouseInput.X, MouseInput.Y);
			}
		}

		public override TakaGUI.Services.ViewRect GetDefaultBoundaries(int newWidth, int newHeight)
		{
			return new TakaGUI.Services.ViewRect(RealX, RealY, newWidth, newHeight);
		}

		public override void Project(GameTime gameTime, int x, int y, TakaGUI.Services.IRender render)
		{
			base.Project(gameTime, x, y, render);

			render.Begin();

			if (Mode == Modes.SetPeremiter)
			{
				var peremiter = new Rectangle();
				peremiter.X = Math.Min(p1.X, p2.X);
				peremiter.Y = Math.Min(p1.Y, p2.Y);
				peremiter.Width = Math.Max(p1.X, p2.X) - peremiter.X;
				peremiter.Height = Math.Max(p1.Y, p2.Y) - peremiter.Y;

				render.DrawHorizontalLine(new Point(peremiter.X, peremiter.Y), peremiter.Width, Color.White);
				render.DrawHorizontalLine(new Point(peremiter.X, peremiter.Bottom), peremiter.Width, Color.White);
				render.DrawVerticalLine(new Point(peremiter.X, peremiter.Y), peremiter.Height, Color.White);
				render.DrawVerticalLine(new Point(peremiter.Right, peremiter.Y), peremiter.Height, Color.White);
			}

			render.End();
		}
	}
}
