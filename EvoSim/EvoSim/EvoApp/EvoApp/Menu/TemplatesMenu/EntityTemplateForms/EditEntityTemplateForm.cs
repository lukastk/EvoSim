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
using EvoApp;

namespace EvoApp.Menu.TemplatesMenu.EntityTemplateForms
{
	public class EditEntityTemplateForm : Dialogue
	{
		EntityTemplate template;

		public void Initialize(EntityTemplate _template, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			template = _template;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}
		public static EditEntityTemplateForm ShowDialogue(Window window, EntityTemplate _template, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditEntityTemplateForm();
			form.Initialize(_template, closeFunction, "Edit Entity-Template", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();

			builder.BuildSessionStart(this);

			var templateNameTextField = builder.AddTextField("Template Name: ");
			templateNameTextField.Text = template.TemplateName;

			var panel = new Panel();
			panel.Initialize();
			builder.AddDrawBoxAsField(panel, DrawBoxAlignment.GetFull());

			var guiMethod = template.Entity.GetType().GetMethod("GUI_Edit");
			var isReadyFunc = (Func<bool>)guiMethod.Invoke(template.Entity, new object[] { panel });
			panel.Width = builder.FieldWidth;

			Wrap();

			var okButton = new ResizableButton();
			okButton.Initialize();
			AddDrawBox(okButton);
			okButton.Title = "OK";
			okButton.FitToText();
			okButton.X = builder.FieldWidth - okButton.Width;
			okButton.Y = Height + 5;
			okButton.Click += delegate(object sender)
			{
				if (isReadyFunc())
				{
					template.TemplateName = templateNameTextField.Text;
					Close();
				}
				else
				{
					AlertForm.ShowDialogue(Parent, null, "All fields have not been filled out.");
				}
			};

			builder.BuildSessionEnd();

			okButton.Alignment = DrawBoxAlignment.GetRightBottom();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = true;
		}
	}
}
