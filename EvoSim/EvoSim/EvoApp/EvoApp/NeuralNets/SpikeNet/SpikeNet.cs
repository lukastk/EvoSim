using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using EvoApp;
using EvoSim;
using EvoSim.Genes;

namespace EvoApp.NeuralNets.SpikeNet
{
	public class SpikeNet : NeuralNet<SpNeuron, SpInputNeuron, SpOutputNeuron>
	{
		public List<Spike> Spikes = new List<Spike>();

		//Used when randomizing brain
		public double MinThreshold = 1;
		public double MaxThreshold = 3;

		public double MinExcitationDecayRate = 0.005;
		public double MaxExcitationDecayRate = 0.05;

		public int MinAbsoluteRefactoryPeriod = 0;
		public int MaxAbsoluteRefactoryPeriod = 60;

		public int MinRelativeRefactoryPeriod = 0;
		public int MaxRelativeRefactoryPeriod = 60;

		public double MinRefactoryPenalty= 0;
		public double MaxRefactoryPenalty = 5;

		public int MinConnectionLength = 1;
		public int MaxConnectionLength = 60;

		public double HebbianConnectionIncrease = 0.001;
		public double HebbianConnectionDecay = 4.62962963E-6; //It'll take an hour for a target with the weight 1 to decay to 0.

		public override void Update()
		{
			List<Spike> removeSpikes = new List<Spike>();

			foreach (var spike in Spikes)
			{
				spike.Update();

				if (spike.IsGone)
					removeSpikes.Add(spike);
			}

			foreach (var spike in removeSpikes)
				Spikes.Remove(spike);

			List<SpNeuron> spikingNeurons = new List<SpNeuron>();

			foreach (var neuron in HiddenNeurons)
			{
				neuron.Update();

				if (neuron.NewSpike)
					spikingNeurons.Add(neuron);
			}

			foreach (var neuron in InputNeurons)
			{
				neuron.Update();
				//neuronGene.Spike();

				if (neuron.NewSpike)
					spikingNeurons.Add(neuron);
			}

			foreach (var neuron in OutputNeurons)
				neuron.Update();

			if (spikingNeurons.Count > 1)
				foreach (var neuron in spikingNeurons)
				{
					foreach (SpConnection connection in neuron.Connections)
						if (spikingNeurons.Contains(connection.Target))
							connection.Weight += HebbianConnectionIncrease;
				}
		}

		public override void ImprintGenome(INeuralNetChromosome _genome)
		{
			var genome = (SpikeNetChromosome)_genome;

			#region Creating the neurons
			foreach (var gene in genome.EyeRNeuronGenes)
			{
				var neuron = new SpInputNeuron(this);
				neuron.ImprintGene(gene);
				AddInputNeuron(neuron);
			}
			foreach (var gene in genome.EyeGNeuronGenes)
			{
				var neuron = new SpInputNeuron(this);
				neuron.ImprintGene(gene);
				AddInputNeuron(neuron);
			}
			foreach (var gene in genome.EyeBNeuronGenes)
			{
				var neuron = new SpInputNeuron(this);
				neuron.ImprintGene(gene);
				AddInputNeuron(neuron);
			}
			foreach (var gene in genome.DistanceNeuronGenes)
			{
				var neuron = new SpInputNeuron(this);
				neuron.ImprintGene(gene);
				AddInputNeuron(neuron);
			}

			foreach (var gene in genome.InputNeuronGenes)
			{
				var neuron = new SpInputNeuron(this);
				neuron.ImprintGene(gene);
				AddInputNeuron(neuron);
			}

			foreach (var gene in genome.HiddenNeuronGenes)
			{
				var neuron = new SpNeuron(this);
				neuron.ImprintGene(gene);
				AddHiddenNeuron(neuron);
			}
			foreach (var gene in genome.OutputNeuronGenes)
			{
				var neuron = new SpOutputNeuron(this);
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
					neuron.Connections.Add(SpConnection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			foreach (var gene in genome.EyeGNeuronGenes)
			{
				var neuron = InputNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(SpConnection.GetConnectionFromGene(connectionGene, neuron, this));
			}
			foreach (var gene in genome.EyeBNeuronGenes)
			{
				var neuron = InputNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(SpConnection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			foreach (var gene in genome.DistanceNeuronGenes)
			{
				var neuron = InputNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(SpConnection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			foreach (var gene in genome.InputNeuronGenes)
			{
				var neuron = InputNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(SpConnection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			count = 0;
			foreach (var gene in genome.HiddenNeuronGenes)
			{
				var neuron = HiddenNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(SpConnection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			count = 0;
			foreach (var gene in genome.OutputNeuronGenes)
			{
				var neuron = OutputNeurons[count++];

				foreach (var connectionGene in gene.Connections)
					neuron.Connections.Add(SpConnection.GetConnectionFromGene(connectionGene, neuron, this));
			}

			#endregion
		}
	}
}
