using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;
using TakaGUI.Data;
using TakaGUI.DrawBoxes;
using EvoSim;

namespace EvoApp.Menu.WorldMenu
{
	public class ManageSpawnPointsForm : Dialogue
	{
		ColumnListBox columnListBox;

		List<SpawnPoint> spawnPointList;
		IWorld world;

		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}
		public void Initialize(List<SpawnPoint> _spawnPointList, IWorld _world, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			spawnPointList = _spawnPointList;
			world = _world;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static ManageSpawnPointsForm ShowDialogue(Window window, List<SpawnPoint> _spawnPointList, IWorld _world, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new ManageSpawnPointsForm();
			form.Initialize(_spawnPointList, _world, closeFunction, "Manage Spawnpoints", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			columnListBox = new ColumnListBox();
			columnListBox.Initialize(1);
			AddDrawBox(columnListBox);
			columnListBox.SetIntOrStringSort(false);
			columnListBox.SetColumnName(0, "Name");
			columnListBox.Width = 200;
			columnListBox.Height = 200;

			columnListBox.ItemDoubleClicked += delegate(object sender, ColumnListBox.ListBoxRow item, int index)
			{
				SpawnPoint spawnPoint = null;
				foreach (var sp in spawnPointList)
					if (sp.Name == (string)item.Values[0])
						spawnPoint = sp;

				string oldName = spawnPoint.Name;
				EditSpawnPointForm.ShowDialogue(Parent, spawnPoint, delegate(object _sender)
				{
					foreach (var sp in spawnPointList)
						if (sp.Name == spawnPoint.Name && spawnPoint != sp)
						{
							AlertForm.ShowDialogue(Parent, null, "There is a spawnpoint with that name already.");
							spawnPoint.Name = oldName;
						}

					ReloadListBox();
				});
			};

			ReloadListBox();

			var createTemplateButton = new ResizableButton();
			createTemplateButton.Initialize();
			AddDrawBox(createTemplateButton);
			createTemplateButton.Title = "Create New Spawnpoint";
			createTemplateButton.FitToText();
			Push.ToTheBottomSideOf(createTemplateButton, columnListBox, 3, Push.VerticalAlign.Left);
			createTemplateButton.Width = 200;
			createTemplateButton.Click += delegate(object sender)
			{
				var spawnPoint = new SpawnPoint(world);
				EditSpawnPointForm.ShowDialogue(Parent, spawnPoint, delegate(object _sender)
				{
					bool alreadyExists = false;
					foreach (var sp in spawnPointList)
						if (sp.Name == spawnPoint.Name && spawnPoint != sp)
						{
							alreadyExists = true;
						}

					if (alreadyExists)
						AlertForm.ShowDialogue(Parent, null, "There is a spawnpoint with that name already.");
					else
						spawnPointList.Add(spawnPoint);

					ReloadListBox();
				});
			};

			var deleteTemplateButton = new ResizableButton();
			deleteTemplateButton.Initialize();
			AddDrawBox(deleteTemplateButton);
			deleteTemplateButton.Title = "Delete Spawnpoint";
			deleteTemplateButton.FitToText();
			Push.ToTheBottomSideOf(deleteTemplateButton, createTemplateButton, 3, Push.VerticalAlign.Left);
			deleteTemplateButton.Width = 200;
			deleteTemplateButton.Click += delegate(object sender)
			{
				if (columnListBox.SelectedRow != null)
				{
					var findName = (string)columnListBox.SelectedRow.Values[0];

					foreach (var t in EditorData.EntityTemplates)
					{
						if (t.TemplateName == findName)
						{
							EditorData.EntityTemplates.Remove(t);
							ReloadListBox();
							break;
						}
					}
				}
			};

			var editTemplateButton = new ResizableButton();
			editTemplateButton.Initialize();
			AddDrawBox(editTemplateButton);
			editTemplateButton.Title = "Edit Spawnpoint";
			editTemplateButton.FitToText();
			Push.ToTheBottomSideOf(editTemplateButton, deleteTemplateButton, 3, Push.VerticalAlign.Left);
			editTemplateButton.Width = 200;
			editTemplateButton.Click += delegate(object sender)
			{
				if (columnListBox.SelectedRow == null)
					return;

				SpawnPoint spawnPoint = null;
				foreach (var sp in spawnPointList)
					if (sp.Name == (string)columnListBox.SelectedRow.Values[0])
						spawnPoint = sp;

				string oldName = spawnPoint.Name;
				EditSpawnPointForm.ShowDialogue(Parent, spawnPoint, delegate(object _sender)
				{
					foreach (var sp in spawnPointList)
						if (sp.Name == spawnPoint.Name && spawnPoint != sp)
						{
							AlertForm.ShowDialogue(Parent, null, "There is a spawnpoint with that name already.");
							spawnPoint.Name = oldName;
						}

					ReloadListBox();
				});
			};

			var okButton = new ResizableButton();
			okButton.Initialize();
			AddDrawBox(okButton);
			okButton.Title = "OK";
			okButton.FitToText();
			Push.ToTheBottomSideOf(okButton, editTemplateButton, 3, Push.VerticalAlign.Left);
			okButton.Width = 200;
			okButton.Click += delegate(object sender)
			{
				Close();
			};

			Wrap();

			columnListBox.Alignment = DrawBoxAlignment.GetFull();
			createTemplateButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			deleteTemplateButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			editTemplateButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			okButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			IsClosing += delegate(object sender)
			{
				EditorData.Save(Globals.EditorDataSaveDir);
			};
		}

		void ReloadListBox()
		{
			columnListBox.ClearRows();
			foreach (var sp in spawnPointList)
				columnListBox.AddRow(sp.Name);
		}
	}
}
