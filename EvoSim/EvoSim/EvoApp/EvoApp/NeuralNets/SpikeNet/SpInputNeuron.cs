using System;
using System.Collections.Generic;
using System.Diagnostics;
using EvoSim.RandomNumberGenerators;
using EvoSim.Genes;
using System.Collections.ObjectModel;
using TakaGUI.IO;
using EvoSim;

namespace EvoApp.NeuralNets.SpikeNet
{
	public class SpInputNeuron : SpNeuron, IInputNeuron
	{
		double fireFrequency;
		public double FireFrequency
		{
			get { return fireFrequency; }
			set
			{
				fireFrequency = value;

				if (fireFrequency >= 0)
					SpikeInterval = (int)Math.Round(MinSpikingRate * (1 - fireFrequency), 0);
				else
				{
					fireFrequency = -1;
					spikeTime = 0;
				}
			}
		}
		public int SpikeInterval { get; private set; }
		int spikeTime;
		public static int MinSpikingRate = 60; //Every 1 second.

		CastingList<IConnection> iConnections;
		CastingList<IConnection> IInputNeuron.Connections { get { return iConnections; } }

		public SpInputNeuron(SpikeNet spikeNet)
			: base(spikeNet)
		{
			FireFrequency = -1;

			iConnections = new CastingList<IConnection>(Connections);
		}

		public override void Update()
		{
			base.Update();

			if (fireFrequency != -1)
			{
				spikeTime += 1;

				if (spikeTime >= SpikeInterval)
				{
					Spike();
					spikeTime = 0;
				}
			}
		}
	}
}
