using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;
using TakaGUI.Data;
using TakaGUI.DrawBoxes;
using EvoApp.EntityPainters;
using System.Reflection;
using EvoSim;

namespace EvoApp.Menu.SimulationMenu
{
	public class EntityDrawingOptionsForm : Dialogue
	{
		Simulation simulation;

		Panel leftPanel;
		Panel rightPanel;

		ColumnListBox entitiesWithPainters;
		ColumnListBox entityPainters;

		const int panelMargin = 20;

		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}
		public void Initialize(Simulation _simulation, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			simulation = _simulation;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EntityDrawingOptionsForm ShowDialogue(Window window, Simulation _simulation, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EntityDrawingOptionsForm();
			form.Initialize(_simulation, closeFunction, "Entity Drawing-Options", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			leftPanel = new Panel();
			leftPanel.Initialize();
			AddDrawBox(leftPanel);

			rightPanel = new Panel();
			rightPanel.Initialize();
			AddDrawBox(rightPanel);

			var builder = new FieldBuilder();
			builder.BuildSessionStart(leftPanel);

			entitiesWithPainters = builder.AddColumnListBox("Entities being drawn:", 400, 2);
			entitiesWithPainters.SetColumnName(0, "Entity-Type");
			entitiesWithPainters.SetColumnName(1, "Painter-Type");

			builder.BuildSessionEnd();

			builder.BuildSessionStart(rightPanel);

			entityPainters = builder.AddColumnListBox("Entity-painters:", 400, 1);
			entityPainters.SetColumnName(0, "Painter-Type");

			builder.AddResizableButtonField("Add Painter", delegate(object sender)
			{
				AddEntityPainterForm.ShowDialogue(Parent, simulation, delegate(object _sender)
				{
					var form = (AddEntityPainterForm)_sender;

					form.Result.Initialize(DrawBox.DefaultSkinFile);
					simulation.EntityPainters.Add(form.Result);

					reloadEntitiesWithPaintersList();
					reloadPainterList();
				});
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("Remove Painter", delegate(object sender)
			{
				if (entityPainters.SelectedRow != null)
				{
					simulation.EntityPainters.Remove((IEntityPainter)entityPainters.SelectedRow.ExtraValues[0]);
					reloadEntitiesWithPaintersList();
					reloadPainterList();
				}
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.BuildSessionEnd();

			leftPanel.Width = Math.Max(leftPanel.Width, rightPanel.Width);
			leftPanel.Height = Math.Max(leftPanel.Height, rightPanel.Height);

			rightPanel.X = leftPanel.Width + panelMargin;
			rightPanel.Width = Math.Max(leftPanel.Width, rightPanel.Width);
			rightPanel.Height = Math.Max(leftPanel.Height, rightPanel.Height);

			Wrap();

			reloadEntitiesWithPaintersList();
			reloadPainterList();

			leftPanel.Alignment = DrawBoxAlignment.GetTopBottom();
			rightPanel.Alignment = DrawBoxAlignment.GetTopBottom();

			SizeChanged += new SizeChangedEvent(EntityDrawingOptionsForm_SizeChanged);

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}

		void EntityDrawingOptionsForm_SizeChanged(object sender, Microsoft.Xna.Framework.Point oldSize, Microsoft.Xna.Framework.Point newSize)
		{
			leftPanel.Width = Width / 2 - panelMargin / 2;

			rightPanel.X = leftPanel.Width + panelMargin;
			rightPanel.Width = leftPanel.Width;
		}

		void reloadEntitiesWithPaintersList()
		{
			entitiesWithPainters.ClearRows();

			foreach (var p in simulation.EntityPainters)
			{
				string painterName = p.GetType().Name;
				foreach (var e in p.EntityTypeList)
					entitiesWithPainters.AddRow(e.Name, painterName);
			}
		}

		void reloadPainterList()
		{
			entityPainters.ClearRows();

			foreach (var p in simulation.EntityPainters)
			{
				entityPainters.AddRow(p.GetType().Name);
				entityPainters.Values[entityPainters.Values.Count - 1].ExtraValues = new object[] { p };
			}
		}
	}

	public class AddEntityPainterForm : Dialogue
	{
		Simulation simulation;

		public IEntityPainter Result;

		ColumnListBox entitiesToDraw;

		int startY;
		Panel editPanel;

		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}
		public void Initialize(Simulation _simulation, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			simulation = _simulation;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static AddEntityPainterForm ShowDialogue(Window window, Simulation _simulation, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new AddEntityPainterForm();
			form.Initialize(_simulation, closeFunction, "Add Entity-Painter", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var painterTypesComboBox = builder.AddComboBoxField("Painter-type: ");
			var painterTypes = Globals.GetAllTypesDeriving(typeof(IEntityPainter), Assembly.GetExecutingAssembly());
			painterTypesComboBox.Items.AddRange(painterTypes.Select(s => s.Name));

			painterTypesComboBox.SelectedItemChanged += delegate(object sender, int newItemIndex, int oldItemIndex)
			{
				if (painterTypesComboBox.Index == -1)
					return;

				Result = (IEntityPainter)Activator.CreateInstance(painterTypes[painterTypesComboBox.Index]);
				
				//loadControls();
			};

			builder.AlignTop = true;
			builder.AlignBottom = true;
			entitiesToDraw = builder.AddColumnListBox("Entity-types to draw:", 200, 2);
			entitiesToDraw.SetColumnName(0, "Type-name");
			entitiesToDraw.SetColumnName(1, "State");
			entitiesToDraw.SetIntOrStringSort(false, false);

			builder.AlignTop = false;
			builder.AlignBottom = true;

			var entityTypesComboBox = builder.AddComboBoxField("Entity-type: ");
			var entityTypes = Globals.GetAllTypesDeriving(typeof(IEntity), Assembly.GetExecutingAssembly(), true);
			entityTypesComboBox.Items.AddRange(entityTypes.Select(s => s.Name));

			var addEntityButton = builder.AddResizableButtonField("Add Entity", delegate(object sender)
			{
				if (entityTypesComboBox.Index == -1)
					return;

				Result.EntityTypeList.Add(entityTypes[entityTypesComboBox.Index]);
				reloadList();
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			var okButton = builder.AddResizableButtonField("OK", delegate(object sender)
			{
				Close();
			}, FieldBuilder.ResizableButtonOrientation.Right);

			builder.BuildSessionEnd();

			startY = Height;

			painterTypesComboBox.Index = 0;

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = true;
		}

		void loadControls()
		{
			//if (editPanel != null)
			//    editPanel.Close();

			//editPanel = new Panel();
			//editPanel.Initialize();
			//editPanel.Y = startY;

			//AddDrawBox(editPanel);

			//var builder = new FieldBuilder();
			//builder.BuildSessionStart(editPanel);

			//var painterPanel = new Panel();
			//painterPanel.Initialize();
			//builder.AddDrawBoxAsField(painterPanel, DrawBoxAlignment.GetLeftRightBottom());

			//var isReadyFunc = Result.GUI_Edit(painterPanel);



			//builder.BuildSessionEnd();

			Wrap();

			editPanel.Alignment = DrawBoxAlignment.GetLeftRightBottom();
		}

		void reloadList()
		{
			entitiesToDraw.ClearRows();

			foreach (var t in Result.EntityTypeList)
				entitiesToDraw.AddRow(t.Name, "x");
		}
	}
}
