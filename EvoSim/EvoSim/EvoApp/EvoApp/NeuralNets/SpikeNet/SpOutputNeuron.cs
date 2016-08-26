using System;
using System.Collections.Generic;
using System.Diagnostics;
using EvoSim.RandomNumberGenerators;
using EvoSim.Genes;
using System.Collections.ObjectModel;
using EvoSim;

namespace EvoApp.NeuralNets.SpikeNet
{
	public class SpOutputNeuron : SpNeuron, IOutputNeuron
	{
		public double Output { get; set; }
		int timeSinceLastSpike;
		public static int FrequencyMeasurement = 20;

		public SpOutputNeuron(SpikeNet spikeNet)
			: base(spikeNet)
		{
		}

		public override void Update()
		{
			timeSinceLastSpike += 1;

			double currentFrequency = Math.Pow(1 - (Math.Min(timeSinceLastSpike, FrequencyMeasurement) / (double)FrequencyMeasurement), 1);
			//double currentFrequency = Math.Pow(1 - (timeSinceLastSpike / (double)FrequencyMeasurement), 2);

			if (NewSpike)
			{
				timeSinceLastSpike = 0;

				Output = currentFrequency;

				if (Output < 0)
					Output = 0;
			}
			else if (currentFrequency < Output)
				Output = currentFrequency;

			base.Update();
		}
	}
}
