using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using TakaGUI.IO;
using EvoApp;
using EvoSim;

namespace EvoApp.NeuralNets.RMP
{
	public class RMP_InputNeuron : RMP_Neuron, IInputNeuron
	{
		public double FireFrequency
		{
			get { return base.Output; }
			set { base.Output = value; }
		}

		CastingList<IConnection> iConnections;
		CastingList<IConnection> IInputNeuron.Connections { get { return iConnections; } }

		public RMP_InputNeuron()
		{
			iConnections = new CastingList<IConnection>(Connections);
		}
		public RMP_InputNeuron(RMP_Net net)
			: base(net)
		{
			iConnections = new CastingList<IConnection>(Connections);
		}

		public override void Update()
		{
			foreach (var connection in Connections)
				connection.OutputToTarget();
		}
	}
}
