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

namespace EvoApp.Menu.Templates.WorldTemplateForms
{
	public class CreateWorldTemplateForm : Dialogue
	{
		public WorldTemplate Result;

		public static CreateWorldTemplateForm ShowDialogue(Window window, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new CreateWorldTemplateForm();
			form.Initialize(closeFunction, "Create World-Template", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();

			builder.BuildSessionStart(this);

			var templateNameTextField = builder.AddTextField("Template Name: ");

			var worldTypesComboBox = builder.AddComboBoxField("World Type: ");
			var worldTypeList = Globals.GetAllTypesDeriving(typeof(IWorld), Assembly.GetExecutingAssembly());
			worldTypesComboBox.Items.AddRange(worldTypeList.Select(s => s.Name));
			worldTypesComboBox.Index = 0;

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				if (worldTypesComboBox.Index == -1)
					AlertForm.ShowDialogue(Parent, null, "Select a world type.");
				else
				{
					Result = new WorldTemplate();
					Result.TemplateName = templateNameTextField.Text;
					Result.World = (IWorld)Activator.CreateInstance(worldTypeList[worldTypesComboBox.Index]);

					EditWorldTemplateForm.ShowDialogue(Parent, Result);

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
