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
using EvoExt.NeuralNets.RMP;
using EvoSim.Genes;

namespace EvoApp.Menu.Templates.AnnTemplateForms
{
	public class EditAnnTemplateForm : Dialogue
	{
		public bool ChromosomeIsComplete;

		ANNTemplate ann;

		TextField templateNameTextField;

		public void Initialize(ANNTemplate _ann, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			ann = _ann;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditAnnTemplateForm ShowDialogue(Window window, ANNTemplate _ann, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditAnnTemplateForm();
			form.Initialize(_ann, closeFunction, "Edit ANN-Template", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			templateNameTextField = builder.AddTextField("Template Name: ");
			templateNameTextField.Text = ann.TemplateName;

			Panel panel = new Panel();
			panel.Initialize();
			builder.AddDrawBoxAsField(panel, DrawBoxAlignment.GetFull());

			var isReadyFunc = (Func<bool>)ann.ANN.GetType().InvokeMember("GUI_Edit", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { panel, ann.ANN });

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				if (isReadyFunc())
					Close();
				else
					AlertForm.ShowDialogue(Parent, null, "All fields have not been filled out.");
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}

		public override void Close()
		{
			ann.TemplateName = templateNameTextField.Text;

			base.Close();
		}
	}
}
