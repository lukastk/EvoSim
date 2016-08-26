using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;
using TakaGUI.Data;
using TakaGUI.DrawBoxes;
using EvoSim.Genes;

namespace EvoApp.Forms
{
	public class EditDefaultSigmoid : Dialogue
	{
		DefaultSigmoid sigmoid;
		List<Gene> geneList;

		public void Initialize(DefaultSigmoid _sigmoid, List<Gene> _geneList, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			sigmoid = _sigmoid;
			geneList = _geneList;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditDefaultSigmoid ShowDialogue(Window window, DefaultSigmoid sigmoid, List<Gene> sigmoidGeneList, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditDefaultSigmoid();
			form.Initialize(sigmoid, sigmoidGeneList, closeFunction, "Edit Neuron-list", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var symmetricalBordersCheckBox = builder.AddCheckBoxField("Symmetrical Borders: ");
			symmetricalBordersCheckBox.Checked = sigmoid.SymmetricalBorders;

			builder.AddLabelField("P: ");
			var pMin = builder.AddDoubleField("Min: ");
			var pMax = builder.AddDoubleField("Max: ");
			var pVal = builder.AddDoubleField("Value: ");
			var pMutable = builder.AddCheckBoxField("IsMutable: ");

			builder.AddVerticalMargin(4);

			builder.AddLabelField("Output-Border: ");
			var o1Min = builder.AddDoubleField("Min: ");
			var o1Max = builder.AddDoubleField("Max: ");
			var o1Val = builder.AddDoubleField("Value: ");
			var o1Mutable = builder.AddCheckBoxField("IsMutable: ");

			var switchBox = new SwitchBox();
			switchBox.Initialize();
			builder.AddDrawBoxAsField(switchBox, DrawBoxAlignment.GetLeftRight());

			var emptyPanel = new Panel();
			emptyPanel.Initialize();

			switchBox.AddDrawBox(emptyPanel, "symmetrical");

			var panel = new Panel();
			panel.Initialize();

			var builder2 = new FieldBuilder();
			builder2.BuildSessionStart(panel);
			builder2.AddLabelField("Output-Border 2: ");
			var o2Min = builder2.AddDoubleField("Min: ");
			var o2Max = builder2.AddDoubleField("Max: ");
			var o2Val = builder2.AddDoubleField("Value: ");
			var o2Mutable = builder2.AddCheckBoxField("IsMutable: ");
			builder2.BuildSessionEnd();

			switchBox.AddDrawBox(panel, "non-symmetrical");

			int largestHeight = 0;
			foreach (var p in switchBox.DrawBoxList)
				if (p.Height > largestHeight)
					largestHeight = panel.Height;

			switchBox.Width = builder.FieldWidth;
			switchBox.Height = largestHeight;

			if (sigmoid.GeneList.Count != 0)
			{
				var o1Gene = (DoubleGene)sigmoid.GeneList[0];
				o1Min.Value = o1Gene.Min;
				o1Max.Value = o1Gene.Max;
				o1Val.Value = o1Gene.Value;
				o1Mutable.Checked = o1Gene.IsMutable;

				int n = 1;

				if (!sigmoid.SymmetricalBorders)
				{
					var o2Gene = (DoubleGene)sigmoid.GeneList[n++];
					o2Min.Value = o2Gene.Min;
					o2Max.Value = o2Gene.Max;
					o2Val.Value = o2Gene.Value;
					o2Mutable.Checked = o2Gene.IsMutable;
				}

				var pGene = (DoubleGene)sigmoid.GeneList[n++];
				pMin.Value = pGene.Min;
				pMax.Value = pGene.Max;
				pVal.Value = pGene.Value;
				pMutable.Checked = pGene.IsMutable;
			}

			if (sigmoid.SymmetricalBorders)
				switchBox.SelectDrawBoxWithKey("symmetrical");
			else
				switchBox.SelectDrawBoxWithKey("non-symmetrical");

			symmetricalBordersCheckBox.CheckedChanged += delegate(object sender, bool newValue)
			{
				if (newValue)
					switchBox.SelectDrawBoxWithKey("symmetrical");
				else
					switchBox.SelectDrawBoxWithKey("non-symmetrical");
			};

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				Close();
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			IsClosing += delegate(object sender)
			{
				geneList.Clear();

				var _o1Gene = new DoubleGene();
				_o1Gene.SetMinMaxValue(o1Min.Value, o1Max.Value, o1Val.Value);
				_o1Gene.IsMutable = o1Mutable.Checked;
				geneList.Add(_o1Gene);

				if (!symmetricalBordersCheckBox.Checked)
				{
					var _o2Gene = new DoubleGene();
					_o2Gene.SetMinMaxValue(o2Min.Value, o2Max.Value, o2Val.Value);
					_o2Gene.IsMutable = pMutable.Checked;
					geneList.Add(_o2Gene);
				}

				var _pGene = new DoubleGene();
				_pGene.SetMinMaxValue(pMin.Value, pMax.Value, pVal.Value);
				_pGene.IsMutable = pMutable.Checked;
				geneList.Add(_pGene);
			};

			CanResizeFormVertically = false;

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}
	}
}
