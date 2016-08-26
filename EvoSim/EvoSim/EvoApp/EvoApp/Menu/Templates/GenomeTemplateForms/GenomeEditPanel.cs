using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes;
using TakaGUI.DrawBoxes.Forms;
using EvoSim.Genes;
using TakaGUI;
using TakaGUI.Data;
using EvoSim;
using System.Reflection;
using EvoExt.Forms;

namespace EvoApp.Menu.Templates.GenomeTemplateForms
{
	public class GenomeEditPanel : Panel
	{
		EvoSim.Genes.Genome genome;

		public override void Initialize()
		{
			throw new NotImplementedException();
		}
		public void Initialize(EvoSim.Genes.Genome _genome)
		{
			genome = _genome;

			base.BaseInitialize();
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var isFirstGeneration = builder.AddCheckBoxField("IsFirstGeneration: ");
			isFirstGeneration.Checked = genome.IsFirstGeneration;
			isFirstGeneration.CanChangeValue = false;

			var parent1 = builder.AddIntegerField("Parent1: ");
			parent1.Value = genome.Parent1;
			parent1.CanChangeValue = false;

			var parent2 = builder.AddIntegerField("Parent2: ");
			parent2.Value = genome.Parent2;
			parent2.CanChangeValue = false;

			builder.AddResizableButtonField("Edit Body-genes", delegate(object sender)
			{
				EditDoubleGeneListForm.ShowDialogue(Parent, genome.BodyGenes);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("Edit Mutation-genes", delegate(object sender)
			{
				EditDoubleGeneListForm.ShowDialogue(Parent, genome.MutationGenes);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			var crossoverTypes = Globals.GetAllTypesDeriving(typeof(CrossoverFunction), Assembly.GetExecutingAssembly());
			var crossoverNames = new List<string>(crossoverTypes.Select(s => s.Name));

			var crossoverComboBox = builder.AddComboBoxField("Crossover Function: ", crossoverNames);
			if (genome.CrossoverFunction != null)
			{
				var crossoverType = genome.CrossoverFunction.GetType();
				foreach (var type in crossoverTypes)
					if (type.IsEquivalentTo(crossoverType))
					{
						crossoverComboBox.Index = crossoverTypes.IndexOf(type);
					}
			}

			crossoverComboBox.SelectedItemChanged += delegate(object sender, int newItemIndex, int oldItemIndex)
			{
				genome.CrossoverFunction = (CrossoverFunction)Activator.CreateInstance(crossoverTypes[newItemIndex]);
			};
			var globalSigmoidEditButton = builder.AddResizableButtonField("Edit Crossover Function", delegate(object sender)
			{
				if (genome.CrossoverFunction == null)
					return;

				var crossoverFunc = genome.CrossoverFunction;
				crossoverTypes[crossoverComboBox.Index].InvokeMember("GUI_Edit", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { Parent, crossoverFunc });
			});

			builder.BuildSessionEnd();
		}
	}
}
