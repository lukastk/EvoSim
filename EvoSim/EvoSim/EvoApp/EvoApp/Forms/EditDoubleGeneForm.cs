using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using EvoSim.Genes;
using TakaGUI;
using TakaGUI.Data;

namespace EvoApp.Forms
{
	public class EditDoubleGeneForm : Dialogue
	{
		public DoubleGene Result;

		public void Initialize(DoubleGene _gene, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			Result = _gene;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditDoubleGeneForm ShowDialogue(Window window, DoubleGene _gene, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditDoubleGeneForm();
			form.Initialize(_gene, closeFunction, "Edit Gene", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var gName = builder.AddTextField("Name: ");
			var gMin = builder.AddDoubleField("Min: ");
			var gMax = builder.AddDoubleField("Max: ");
			var gVal = builder.AddDoubleField("Value: ");
			var gMutable = builder.AddCheckBoxField("IsMutable: ");

			gName.Text = Result.Name;
			gMin.Value = Result.Min;
			gMax.Value = Result.Max;
			gVal.Value = Result.Value;
			gMutable.Checked = Result.IsMutable;

			builder.AddVerticalMargin(5);

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				Result.Name = gName.Text;
				Result.SetMinMaxValue(gMin.Value, gMax.Value, gVal.Value);
				Result.IsMutable = gMutable.Checked;

				Close();
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			CanResizeFormVertically = false;

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}
	}
}
