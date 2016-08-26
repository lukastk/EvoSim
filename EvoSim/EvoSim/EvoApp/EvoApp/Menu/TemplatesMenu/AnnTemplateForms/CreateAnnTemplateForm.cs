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

namespace EvoApp.Menu.TemplatesMenu.AnnTemplateForms
{
	public class CreateAnnTemplateForm : Dialogue
	{
		public ANNTemplate Result;

		ComboBox annTypesComboBox;

		Dictionary<Type, Dictionary<string, object>> valueHolders = new Dictionary<Type, Dictionary<string, object>>();

		public static CreateAnnTemplateForm ShowDialogue(Window window, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new CreateAnnTemplateForm();
			form.Initialize(closeFunction, "Create ANN-Template", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();

			builder.BuildSessionStart(this);

			var nameField = builder.AddTextField("Name: ");

			annTypesComboBox = builder.AddComboBoxField("ANN Type: ");
			var annTypeList = Globals.GetAllTypesDeriving(typeof(INeuralNetChromosome), Assembly.GetExecutingAssembly());
			annTypesComboBox.Items.AddRange(annTypeList.Select(s => s.Name));
			annTypesComboBox.Index = 0;

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				Result = new ANNTemplate();
				Result.TemplateName = nameField.Text;
				Result.ANN = (INeuralNetChromosome)Activator.CreateInstance(annTypeList[annTypesComboBox.Index]);

				Close();

				EditAnnTemplateForm.ShowDialogue(Parent, Result);
			}, FieldBuilder.ResizableButtonOrientation.Right);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}
	}
}
