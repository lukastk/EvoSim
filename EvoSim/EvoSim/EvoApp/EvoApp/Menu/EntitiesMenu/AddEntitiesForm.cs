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
using EvoApp.NeuralNets.RMP;
using EvoSim.Genes;

namespace EvoApp.Menu.EntitiesMenu
{
	public class AddEntitiesForm : Dialogue
	{
		IWorld world;

		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}

		public void Initialize(IWorld _world, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			world = _world;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static AddEntitiesForm ShowDialogue(Window window, IWorld _world, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new AddEntitiesForm();
			form.Initialize(_world, closeFunction, "Add Entities", true, true, category, file);
			form.Show(window);

			return form;
		}

		IEntity entity;
		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();

			builder.BuildSessionStart(this);

			var entityTemplatesComboBox = builder.AddComboBoxField("Templates: ");
			entityTemplatesComboBox.Items.AddRange(EditorData.EntityTemplates.Select(s => s.TemplateName));

			entityTemplatesComboBox.SelectedItemChanged += delegate(object sender, int newItemIndex, int oldItemIndex)
			{
				if (entityTemplatesComboBox.Index == -1)
					return;

				string name = entityTemplatesComboBox.Items[entityTemplatesComboBox.Index];
				foreach (var c in EditorData.EntityTemplates)
					if (c.TemplateName == name)
					{
						entity = c.Entity.Clone();
						break;
					}
			};

			entityTemplatesComboBox.Index = 0;

			var editButton = builder.AddResizableButtonField("Edit Entity", delegate(object sender)
			{
				if (entity != null)
					EditEntityForm.ShowDialogue(Parent, entity);
			});

			var addByClicksCheckBox = builder.AddCheckBoxField("Add by clicks: ");

			var amountField = builder.AddIntegerField("Amount to add: ");
			amountField.MinValue = 1;

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				Editor.GetEntityFunctionDelegate getEntityFunc = delegate()
				{
					var clone = (IEntity)entity.Clone();

					return clone;
				};

				if (!addByClicksCheckBox.Checked)
				{
					for (int i = 0; i < amountField.Value; i++)
					{
						var clone = getEntityFunc();

						world.AddEntity(clone);

						clone.Position = new EntityPosition(Globals.Random.Next(world.Width), Globals.Random.Next(world.Height));
					}
				}
				else
				{
					Globals.Editor.Mode = Editor.Modes.AddEntity;
					Globals.Editor.Set_GetEntityFunction(getEntityFunc);
				}

				Close();
			});

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}
	}
}
