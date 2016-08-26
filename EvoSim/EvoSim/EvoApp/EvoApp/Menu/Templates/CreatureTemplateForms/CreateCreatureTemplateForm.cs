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

namespace EvoApp.Menu.Templates.CreatureTemplateForms
{
	public class CreateCreatureTemplateForm : Dialogue
	{
		public CreatureTemplate Result;

		public static CreateCreatureTemplateForm ShowDialogue(Window window, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new CreateCreatureTemplateForm();
			form.Initialize(closeFunction, "Create Creature-Template", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();

			builder.BuildSessionStart(this);

			var templateNameTextField = builder.AddTextField("Template Name: ");

			var creatureTypesComboBox = builder.AddComboBoxField("Creature Type: ");
			var creatureTypeList = Globals.GetAllTypesDeriving(typeof(ICreature), Assembly.GetExecutingAssembly());
			creatureTypesComboBox.Items.AddRange(creatureTypeList.Select(s => s.Name));
			creatureTypesComboBox.Index = 0;

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				if (creatureTypesComboBox.Index == -1)
					AlertForm.ShowDialogue(Parent, null, "Select a creature type.");
				else
				{
					Result = new CreatureTemplate();
					Result.TemplateName = templateNameTextField.Text;
					Result.Creature = (ICreature)Activator.CreateInstance(creatureTypeList[creatureTypesComboBox.Index]);

					EditCreatureTemplateForm.ShowDialogue(Parent, Result);

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
