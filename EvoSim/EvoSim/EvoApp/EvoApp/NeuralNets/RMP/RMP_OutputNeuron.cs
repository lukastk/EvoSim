using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using TakaGUI.IO;
using EvoApp;
using EvoSim;

namespace EvoApp.NeuralNets.RMP
{
	public class RMP_OutputNeuron : RMP_Neuron, IOutputNeuron
	{
		public RMP_OutputNeuron()
		{
		}
		public RMP_OutputNeuron(RMP_Net net)
			: base(net)
		{
		}

		public override void Update()
		{
			Output = SigmoidFunction.Sigmoid(Activation - Bias);

			Activation = 0;
		}
	}
}
