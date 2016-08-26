using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EvoSim;
using EvoSim.Genes;
using TakaGUI;
using TakaGUI.DrawBoxes.Forms;

namespace EvoApp
{
	public class DefaultSigmoid : SigmoidFunction
	{
		public double P
		{
			get;
			private set;
		}

		public double OutputMin
		{
			get;
			private set;
		}
		public double OutputMax
		{
			get;
			private set;
		}

		public bool SymmetricalBorders
		{
			get { return geneList.Count == 2; }
		}

		public DefaultSigmoid()
		{
		}
		public DefaultSigmoid(bool symmetricalBorders)
		{
			Initialize(symmetricalBorders);
		}
		void Initialize(bool symmetricalBorders)
		{
			DoubleGene gene;

			if (symmetricalBorders)
			{
				gene = new DoubleGene("OutputBorder", 0, 1);
				gene.Value = 1;
				geneList.Add(gene);
			}
			else
			{
				gene = new DoubleGene("OutputBorder1", -1, 1);
				gene.Value = -1;
				geneList.Add(gene);

				gene = new DoubleGene("OutputBorder2", -1, 1);
				gene.Value = 1;
				geneList.Add(gene);
			}

			gene = new DoubleGene("P", 0, 1);
			gene.Value = 1;
			gene.IsMutable = false;
			geneList.Add(gene);

			GenesHasChanged();
		}

		public override double Sigmoid(double a)
		{
			double diff = OutputMax - OutputMin;
			return diff * BarebonesSigmoid(4 * P * a / diff) + OutputMin;
		}

		public double BarebonesSigmoid(double a)
		{
			return 1 / (1 + Math.Exp(-a));
		}

		public override void GenesHasChanged()
		{
			if (SymmetricalBorders)
			{
				OutputMin = -((DoubleGene)geneList[0]).Value;
				OutputMax = ((DoubleGene)geneList[0]).Value;
				P = ((DoubleGene)geneList[1]).Value;
			}
			else
			{
				OutputMin = Math.Min(((DoubleGene)geneList[0]).Value, ((DoubleGene)geneList[1]).Value);
				OutputMax = Math.Max(((DoubleGene)geneList[0]).Value, ((DoubleGene)geneList[1]).Value);
				P = ((DoubleGene)geneList[2]).Value;
			}
		}

		protected override GeneModule GetNewThis()
		{
			return new DefaultSigmoid();
		}

		public override GeneModule Clone()
		{
			var clone = (DefaultSigmoid)GetNewThis();

			foreach (var gene in geneList)
				clone.geneList.Add(gene.Clone());

			clone.GenesHasChanged();

			return clone;
		}

		public override GeneModule CrossoverModule(GeneModule g2, CrossoverFunction crossoverFunction)
		{
			var child = (DefaultSigmoid)GetNewThis();

			DefaultSigmoid s2 = (DefaultSigmoid)g2;

			crossoverFunction.Crossover(geneList, s2.geneList, child.geneList);
			child.GenesHasChanged();

			return child;
		}

		public override void Save(BinaryWriter w)
		{
			base.Save(w);
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			GenesHasChanged();
		}

		public static void GUI_Edit(Window parent, DefaultSigmoid sigmoid)
		{
			Forms.EditDefaultSigmoid.ShowDialogue(parent, sigmoid, sigmoid.geneList);
		}
	}

	public class BarebonesSigmoid : SigmoidFunction
	{
		public override double Sigmoid(double a)
		{
			return 1 / (1 + Math.Exp(-a));
		}

		protected override GeneModule GetNewThis()
		{
			return new BarebonesSigmoid();
		}

		public static void GUI_Edit(Window parent, BarebonesSigmoid sigmoid)
		{
			AlertForm.ShowDialogue(parent, "Alert", "No parameters to change.");
		}
	}
}
