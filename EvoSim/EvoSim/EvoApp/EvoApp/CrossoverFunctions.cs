using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoSim;
using EvoSim.Genes;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;

namespace EvoApp
{
	public class SinglePointCrossover : CrossoverFunction
	{
		public override void Crossover<T>(List<T> list1, List<T> list2, List<T> putIn)
		{
			putIn.Clear();

			List<T> currentList;

			if (Pseudo.Random.Next(2) == 1)
				currentList = list1;
			else
				currentList = list2;

			var cloneList = new List<T>();

			int crossoverPoint = Pseudo.Random.Next(Math.Max(list1.Count, list2.Count));
			int leftOver = Math.Max(list1.Count, list2.Count) - crossoverPoint;
			cloneList.AddRange(currentList.Take(Math.Min(crossoverPoint, currentList.Count)));

			if (currentList == list2)
				currentList = list1;
			else
				currentList = list2;
			var listPart = new List<T>(currentList.Skip(crossoverPoint));
			cloneList.AddRange(listPart.Take(Math.Min(leftOver, listPart.Count)));

			if (list1.Count > cloneList.Count)
				cloneList.AddRange(list1.Skip(putIn.Count));
			else if (list2.Count > cloneList.Count)
				cloneList.AddRange(list2.Skip(putIn.Count));

			foreach (T elem in cloneList)
				putIn.Add((T)elem.Clone());
		}

		protected override GeneModule GetNewThis()
		{
			return new SinglePointCrossover();
		}

		public static void GUI_Edit(Window parent, SinglePointCrossover func)
		{
			AlertForm.ShowDialogue(parent, "Alert", "No parameters to change.");
		}
	}

	public class UniformCrossover : CrossoverFunction
	{
		public DoubleGene CrossoverRate { get { return (DoubleGene)geneList[0]; } }

		public override void Crossover<T>(List<T> list1, List<T> list2, List<T> putIn)
		{
			putIn.Clear();

			List<T> currentList;

			if (Pseudo.Random.Next(2) == 1)
				currentList = list1;
			else
				currentList = list2;

			//CrossoverMethod-MutationGenes
			int i = 0;
			bool last = false;
			while (true)
			{
				if (currentList.Count <= i)
				{
					if (last)
						break;
					else
					{
						last = true;

						if (currentList == list1)
							currentList = list2;
						else
							currentList = list1;
					}

					if (currentList.Count <= i)
						break;
				}

				putIn.Add((T)currentList[i].Clone());

				if (!last && Pseudo.Random.NextDouble() < CrossoverRate.Value)
				{
					if (currentList == list1)
						currentList = list2;
					else
						currentList = list1;
				}

				i++;
			}
		}

		public UniformCrossover()
		{
			var gene = new DoubleGene("CrossoverRate", 0, 1);
			gene.Value = 0.1;

			geneList.Add(gene);
		}

		protected override GeneModule GetNewThis()
		{
			return new UniformCrossover();
		}

		public static void GUI_Edit(Window parent, UniformCrossover func)
		{
			var form = Forms.EditDoubleGeneForm.ShowDialogue(parent, func.CrossoverRate);
			form.Title = "Edit Uniform Crossover Function";
		}
	}
}
