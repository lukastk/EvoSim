using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using EvoApp;
using EvoSim;
using EvoSim.Genes;
using TakaGUI;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI.DrawBoxes;
using EvoApp.NeuralNets.RMP.Forms;

namespace EvoApp.NeuralNets.RMP
{
	public class RMP_Net : NeuralNet<RMP_Neuron, RMP_InputNeuron, RMP_OutputNeuron>
	{
		public override void Update()
		{
			foreach (var neuron in InputNeurons)
				neuron.Update();

			foreach (var neuron in HiddenNeurons)
				neuron.Update();

			foreach (var neuron in OutputNeurons)
				neuron.Update();
		}

		public override void ImprintGenome(INeuralNetChromosome _genome)
		{
			var genome = (RMP_Chromosome)_genome;

			#region Creating the neurons
			foreach (var gene in genome.EyeRNeuronGenes)
			{
				var neuron = new RMP_InputNeuron(this);
				neuron.ImprintGene(gene);
				AddInputNeuron(neuron);
			}
			foreach (var gene in genome.EyeGNeuronGenes)
			{
				var neuron = new RMP_InputNeuron(this);
				neuron.ImprintGene(gene);
				AddInputNeuron(neuron);
			}
			foreach (var gene in genome.EyeBNeuronGenes)
			{
				var neuron = new RMP_InputNeuron(this);
				neuron.ImprintGene(gene);
				AddInputNeuron(neuron);
			}
			foreach (var gene in genome.DistanceNeuronGenes)
			{
				var neuron = new RMP_InputNeuron(this);
				neuron.ImprintGene(gene);
				AddInputNeuron(neuron);
			}

			foreach (var gene in genome.InputNeuronGenes)
			{
				var neuron = new RMP_InputNeuron(this);
				neuron.ImprintGene(gene);
				AddInputNeuron(neuron);
			}

			foreach (var gene in genome.HiddenNeuronGenes)
			{
				var neuron = new RMP_Neuron(this);
				neuron.ImprintGene(gene);
				AddHiddenNeuron(neuron);
			}
			foreach (var gene in genome.OutputNeuronGenes)
			{
				var neuron = new RMP_OutputNeuron(this);
				neuron.ImprintGene(gene);
				AddOutputNeuron(neuron);
			}
			
			#endregion

			#region Connection the neurons
			int count = 0;
			foreach (var gene in genome.EyeRNeuronGenes)
			{
				var neuron = InputNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(RMP_Connection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			foreach (var gene in genome.EyeGNeuronGenes)
			{
				var neuron = InputNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(RMP_Connection.GetConnectionFromGene(connectionGene, neuron, this));
			}
			foreach (var gene in genome.EyeBNeuronGenes)
			{
				var neuron = InputNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(RMP_Connection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			foreach (var gene in genome.DistanceNeuronGenes)
			{
				var neuron = InputNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(RMP_Connection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			foreach (var gene in genome.InputNeuronGenes)
			{
				var neuron = InputNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(RMP_Connection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			count = 0;
			foreach (var gene in genome.HiddenNeuronGenes)
			{
				var neuron = HiddenNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(RMP_Connection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			count = 0;
			foreach (var gene in genome.OutputNeuronGenes)
			{
				var neuron = OutputNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(RMP_Connection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			#endregion
		}

		public override Func<bool> GUI_Edit(SingleSlotBox container)
		{
			var builder = new FieldBuilder();
			builder.BuildSessionStart(container);

			builder.AddResizableButtonField("Edit Input-Neurons", delegate(object sender)
			{
				EditNeuronListForm<RMP_InputNeuron>.ShowDialogue(container.Parent, InputNeurons);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("Edit Hidden-Neurons", delegate(object sender)
			{
				EditNeuronListForm<RMP_Neuron>.ShowDialogue(container.Parent, HiddenNeurons);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("Edit Output-Neurons", delegate(object sender)
			{
				EditNeuronListForm<RMP_OutputNeuron>.ShowDialogue(container.Parent, OutputNeurons);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddVerticalMargin(5);

			builder.BuildSessionEnd();

			return delegate()
			{
				return true;
			};
		}
	}
}
