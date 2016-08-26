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

namespace EvoApp.Menu.TemplatesMenu.EntityTemplateForms
{
	public class CreateEntityTemplateForm : Dialogue
	{
		public EntityTemplate Result;

		public static CreateEntityTemplateForm ShowDialogue(Window window, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new CreateEntityTemplateForm();
			form.Initialize(closeFunction, "Create Entity-Template", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();

			builder.BuildSessionStart(this);

			var templateNameTextField = builder.AddTextField("Template Name: ");

			var entityTypesComboBox = builder.AddComboBoxField("Entity Type: ");
			var entityTypeList = Globals.GetAllTypesDeriving(typeof(IEntity), Assembly.GetExecutingAssembly());

			//Remove all creature-types
			var creatureType = typeof(ICreature);
			foreach (var t in new List<Type>(entityTypeList))
				if (creatureType.IsAssignableFrom(t))
					entityTypeList.Remove(t);

			entityTypesComboBox.Items.AddRange(entityTypeList.Select(s => s.Name));
			entityTypesComboBox.Index = 0;

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				if (entityTypesComboBox.Index == -1)
					AlertForm.ShowDialogue(Parent, null, "Select a entity type.");
				else
				{
					Result = new EntityTemplate();
					Result.TemplateName = templateNameTextField.Text;
					Result.Entity = (IEntity)Activator.CreateInstance(entityTypeList[entityTypesComboBox.Index]);

					EditEntityTemplateForm.ShowDialogue(Parent, Result);

					Close();
				}
			});

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}
	}
}
