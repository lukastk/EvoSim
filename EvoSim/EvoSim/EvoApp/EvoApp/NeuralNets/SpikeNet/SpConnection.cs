using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using EvoApp;
using EvoSim;

namespace EvoApp.NeuralNets.SpikeNet
{
	[DebuggerDisplay("Weight={Weight}, Delay={Delay}")]
	public class SpConnection : IConnection
	{
		[DebuggerDisplay("ID={Source.GeneID}")]
		public SpNeuron Source;
		[DebuggerDisplay("ID={Target.GeneID}")]
		public SpNeuron Target;

		public double Weight { get; set; } //A value between -1 and 1.
		public int Delay { get; set; } //Cycles it takes for the spike to traverse across the target.

		public static int MaxDelay = 60;

		public SpConnection()
		{
		}

		public void ImprintGene(SpConnectionGene gene, SpNeuron source, SpikeNet net)
		{
			SpNeuron target;
			if (gene.IsOutputConnection)
				target = net.OutputNeurons[gene.Target];
			else
				target = net.HiddenNeurons[gene.Target];

			Source = source;
			Target = target;

			Weight = gene.Strength.Value;
			Delay = (int)Math.Round(gene.Delay.Value * MaxDelay, 0);
		}
		public static SpConnection GetConnectionFromGene(SpConnectionGene gene, SpNeuron source, SpikeNet net)
		{
			var connection = new SpConnection();
			connection.ImprintGene(gene, source, net);

			return connection;
		}
	}
}
