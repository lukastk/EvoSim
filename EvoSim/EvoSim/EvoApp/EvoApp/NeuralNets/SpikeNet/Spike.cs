using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace EvoApp.NeuralNets.SpikeNet
{
	public class Spike
	{
		public SpNeuron Source { get; private set; }
		public SpNeuron Target { get; private set; }
		public SpConnection Connection { get; private set; }
		public double Strength;
		public int CyclesToImpact { get; private set; }

		public bool IsGone
		{
			get { return CyclesToImpact <= 0; }
		}

		public Spike(SpNeuron source, SpConnection connection)
		{
			Source = source;
			Target = connection.Target;
			Connection = connection;
			Strength = connection.Weight;
			CyclesToImpact = connection.Delay;
		}

		public void Update()
		{
			CyclesToImpact -= 1;

			if (CyclesToImpact == 0)
				Connection.Target.Excite(Strength);
		}
	}
}
