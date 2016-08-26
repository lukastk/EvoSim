using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;
using TakaGUI.Data;
using EvoSim;
using TakaGUI.DrawBoxes;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace EvoApp.Menu.WorldMenu
{
	public class EditSpawnPointForm : Dialogue
	{
		SpawnPoint spawnPoint;

		Action reloadValues;

        ColumnListBox spawnEntitiesList;

		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}
		public void Initialize(SpawnPoint _spawnPoint, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			spawnPoint = _spawnPoint;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditSpawnPointForm ShowDialogue(Window window, SpawnPoint _spawnPoint, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditSpawnPointForm();
			form.Initialize(_spawnPoint, closeFunction, "Edit Spawnpoint", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();

			builder.BuildSessionStart(this);

			var nameField = builder.AddTextField("Name: ");
			nameField.Text = spawnPoint.Name;

			var posX = builder.AddDoubleField("Position X: ");
			posX.Value = spawnPoint.SpawnArea.X;

			var posY = builder.AddDoubleField("Position Y: ");
			posY.Value = spawnPoint.SpawnArea.Y;

			var width = builder.AddIntegerField("Width: ");
			width.Value = spawnPoint.SpawnArea.Width;

			var height = builder.AddIntegerField("Height: ");
			height.Value = spawnPoint.SpawnArea.Height;

			var timeInterval = builder.AddDoubleField("Time Interval: ");
			timeInterval.Value = spawnPoint.TimeInterval;

			reloadValues = delegate()
			{
				nameField.Text = spawnPoint.Name;
				posX.Value = spawnPoint.SpawnArea.X;
				posY.Value = spawnPoint.SpawnArea.Y;
				width.Value = spawnPoint.SpawnArea.Width;
				height.Value = spawnPoint.SpawnArea.Height;
				timeInterval.Value = spawnPoint.TimeInterval;
			};

			reloadValues();

			Action setValues = delegate()
			{
				spawnPoint.Name = nameField.Text;

				spawnPoint.SpawnArea.X = (int)posX.Value;
				spawnPoint.SpawnArea.Y = (int)posY.Value;
				spawnPoint.SpawnArea.Width = (int)width.Value;
				spawnPoint.SpawnArea.Height = (int)height.Value;

				spawnPoint.SetTimeInterval(timeInterval.Value);
			};

			builder.AddVerticalMargin(5);

			builder.AddResizableButtonField("Set Spawn-Area with mouse", delegate(object sender)
			{
				setValues();

				Parent.DialoguesAreHidden = true;

				Globals.Editor.Mode = Editor.Modes.SetPeremiter;
				Globals.Editor.Set_SendPeremiterFunction(sendPeremiter);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AlignTop = true;
			builder.AlignBottom = true;

			spawnEntitiesList = builder.AddColumnListBox("Spawn-entities", 300, 2);
			spawnEntitiesList.SetIntOrStringSort(false, true);
            spawnEntitiesList.SetColumnName(0, "Entity-Type");
            spawnEntitiesList.SetColumnName(1, "Amount");

			builder.AlignTop = false;
			builder.AlignBottom = true;

            builder.AddResizableButtonField("Add Entity", delegate(object sender)
            {
                AddSpawnEntityForm.ShowDialogue(Parent, spawnPoint, delegate(object _sender)
                {
                    reloadList();
                });
            }, FieldBuilder.ResizableButtonOrientation.Right);

            builder.AddResizableButtonField("Remove Entity", delegate(object sender)
            {
                if (spawnEntitiesList.SelectedRowIndex == -1)
                    return;

                spawnPoint.SpawnClones.Remove((IEntity)spawnEntitiesList.Values[spawnEntitiesList.SelectedRowIndex].ExtraValues[0]);
                reloadValues();
            }, FieldBuilder.ResizableButtonOrientation.Right);

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				setValues();
				Close();
			}, FieldBuilder.ResizableButtonOrientation.Right);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}

		void sendPeremiter(Rectangle peremiter)
		{
			spawnPoint.SpawnArea = peremiter;

			reloadValues();

			Parent.DialoguesAreHidden = false;
		}

        void reloadList()
        {
            spawnEntitiesList.ClearRows();

            foreach (var e in spawnPoint.SpawnClones)
            {
                spawnEntitiesList.AddRow(e.GetType().Name, 0);
                spawnEntitiesList.Values[spawnEntitiesList.Values.Count - 1].ExtraValues = new object[] { e };
            }
        }
	}

    public class AddSpawnEntityForm : Dialogue
    {
        SpawnPoint spawnPoint;

        public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
        {
            throw new NotImplementedException();
        }
        public void Initialize(SpawnPoint _spawnPoint, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
        {
            spawnPoint = _spawnPoint;

            base.Initialize(closeFunction, title, resizable, isDialog, category, file);
        }

        public static AddSpawnEntityForm ShowDialogue(Window window, SpawnPoint _spawnPoint, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
        {
            var form = new AddSpawnEntityForm();
            form.Initialize(_spawnPoint, closeFunction, "Add Entity to Spawn", true, true, category, file);
            form.Show(window);

            return form;
        }

        public override void AddedToContainer()
        {
            base.AddedToContainer();

            var builder = new FieldBuilder();
            builder.BuildSessionStart(this);

            var allEntities = new List<IEntity>();
            allEntities.AddRange(EditorData.EntityTemplates.Select(e => e.Entity));
            allEntities.AddRange(EditorData.CreatureTemplates.Select(e => e.Creature));

            var entityTypesComboBox = builder.AddComboBoxField("Entity-Templates: ");
            entityTypesComboBox.Items.AddRange(EditorData.EntityTemplates.Select(e => e.TemplateName));
            entityTypesComboBox.Items.AddRange(EditorData.CreatureTemplates.Select(e => e.TemplateName));

            var amountField = builder.AddIntegerField("Amount: ");

            var okButton = builder.AddResizableButtonField("OK", delegate(object sender)
            {
                if (entityTypesComboBox.Index == -1)
                    return;

                spawnPoint.SpawnClones.Add(allEntities[entityTypesComboBox.Index]);
                Close();
            }, FieldBuilder.ResizableButtonOrientation.Right);

            builder.BuildSessionEnd();

            X = (Parent.Width / 2) - (Width / 2);
            Y = (Parent.Height / 2) - (Height / 2);

            CanResizeFormVertically = false;
        }
    }
}
