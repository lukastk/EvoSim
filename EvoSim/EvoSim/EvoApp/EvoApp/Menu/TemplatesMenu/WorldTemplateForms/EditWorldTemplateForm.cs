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

namespace EvoApp.Menu.TemplatesMenu.WorldTemplateForms
{
	public class EditWorldTemplateForm : Dialogue
	{
		WorldTemplate template;

		Dictionary<Type, Dictionary<string, object>> valueHolders = new Dictionary<Type, Dictionary<string, object>>();

		public void Initialize(WorldTemplate _template, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			template = _template;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditWorldTemplateForm ShowDialogue(Window window, WorldTemplate _template, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditWorldTemplateForm();
			form.Initialize(_template, closeFunction, "Edit World-Template", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var templateNameTextField = builder.AddTextField("Template Name: ");
			templateNameTextField.Text = template.TemplateName;

			Panel panel = new Panel();
			panel.Initialize();
			builder.AddDrawBoxAsField(panel, DrawBoxAlignment.GetFull());

			var isReadyFunc = (Func<bool>)template.World.GetType().InvokeMember("GUI_Edit", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { panel, template.World });

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				if (isReadyFunc())
				{
					template.TemplateName = templateNameTextField.Text;
					Close();
				}
				else
					AlertForm.ShowDialogue(Parent, null, "All fields have not been filled out.");
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}
	}
}
