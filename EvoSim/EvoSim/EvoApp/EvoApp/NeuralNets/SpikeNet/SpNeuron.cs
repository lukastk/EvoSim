using System;
using System.Collections.Generic;
using System.Diagnostics;
using EvoSim.RandomNumberGenerators;
using EvoSim.Genes;
using System.Collections.ObjectModel;
using EvoSim;

namespace EvoApp.NeuralNets.SpikeNet
{
	public class SpNeuron : BinarySerializable
	{
		public SpikeNet Net;

		public uint GeneID { get; set; }

		public double Excitation { get; protected set; }
		public double Bias { get; set; }
		public double ExcitationDecayRate { get; set; }

		public int AbsoluteRefactoryPeriod { get; set; }
		public int RelativeRefactoryPeriod { get; set; }
		public double RefactoryPenalty { get; set; }

		public double RefactoryTime { get; protected set; }

		public NeuronMode Mode { get; protected set; }
		public bool NewSpike;

		public List<SpConnection> Connections { get; protected set; }

		public static double MaxExcitationDecayRate = 1 / 20;
		public static int MaxAbsouluteRefactoryPeriod = 60;
		public static int MaxRelativeRefactoryPeriod = 60;

		public SpNeuron(SpikeNet spikeNet)
			: base("SpNeuron")
		{
			Net = spikeNet;
			Mode = NeuronMode.Normal;
			Connections = new List<SpConnection>();
		}

		public void Spike()
		{
			foreach (var connection in Connections)
				Net.Spikes.Add(new Spike(this, connection));

			Excitation = 0;
			Mode = NeuronMode.AbsoluteRefactory;

			NewSpike = true;
		}

		public void Excite(double amount)
		{
			if (Mode == NeuronMode.AbsoluteRefactory)
				return;

			Excitation += amount;

			if (Excitation < 0)
				Excitation = 0;
		}

		public virtual void Update()
		{
			NewSpike = false;

			foreach (SpConnection connection in Connections)
				connection.Weight -= Net.HebbianConnectionDecay;

			if ((Mode == NeuronMode.Normal && Excitation > Bias) ||
				(Mode == NeuronMode.RelativeRefactory && Excitation > Bias + RefactoryPenalty))
			{
				Spike();
			}

			if (Mode != NeuronMode.Normal)
			{
				RefactoryTime += 1;

				if (Mode == NeuronMode.AbsoluteRefactory)
				{
					if (RefactoryTime > AbsoluteRefactoryPeriod)
					{
						Mode = NeuronMode.RelativeRefactory;
						RefactoryTime = 0;
					}
				}
				else if (Mode == NeuronMode.RelativeRefactory)
				{
					if (RefactoryTime > RelativeRefactoryPeriod)
					{
						Mode = NeuronMode.Normal;
						RefactoryTime = 0;
					}
				}
			}

			Excite(-ExcitationDecayRate);
		}

		public void ImprintGene(SpNeuronGene gene)
		{
			Name = gene.Name;
			GeneID = gene.ID;

			Bias = gene.Threshold.Value;
			ExcitationDecayRate = MaxExcitationDecayRate * gene.ExcitationDecayRate.Value;

			AbsoluteRefactoryPeriod = (int)Math.Round(gene.AbsoluteRefactoryPeriod.Value * MaxAbsouluteRefactoryPeriod, 0);
			RelativeRefactoryPeriod = (int)Math.Round(gene.RelativeRefactoryPeriod.Value * MaxRelativeRefactoryPeriod, 0);
		}

		public enum NeuronMode
		{
			Normal, AbsoluteRefactory, RelativeRefactory
		}
	}
}
