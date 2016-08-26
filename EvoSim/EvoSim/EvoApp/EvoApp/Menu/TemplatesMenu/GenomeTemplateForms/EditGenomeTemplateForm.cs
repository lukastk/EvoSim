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
//TODO: Be able to set gene-order.
namespace EvoApp.Menu.TemplatesMenu.GenomeTemplateForms
{
	public class EditGenomeTemplateForm : Dialogue
	{
		GenomeTemplate template;
		
		Dictionary<Type, Dictionary<string, object>> valueHolders = new Dictionary<Type, Dictionary<string, object>>();

		public void Initialize(GenomeTemplate _template, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			template = _template;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditGenomeTemplateForm ShowDialogue(Window window, GenomeTemplate _template, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditGenomeTemplateForm();
			form.Initialize(_template, closeFunction, "Edit Genome-Template", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var nameField = builder.AddTextField("Name: ");
			nameField.Text = template.TemplateName;

			var genomeEditPanel = new GenomeEditPanel();
			genomeEditPanel.Initialize(template.Genome);
			builder.AddDrawBoxAsField(genomeEditPanel, DrawBoxAlignment.GetFull());

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				template.TemplateName = nameField.Text;

				Close();
			}, FieldBuilder.ResizableButtonOrientation.Right);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}
	}
}
