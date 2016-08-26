using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using EvoApp;
using EvoSim;

namespace EvoApp.NeuralNets.RMP
{
	public class RMP_Neuron : BinarySerializable
	{
		public RMP_Net Net;
		public uint GeneID { get; set; }

		public double Bias;

		public SigmoidFunction SigmoidFunction;

		public double Activation;
		double output;
		public double Output
		{
			get { return output; }
			set { output = value; }
		}
		public List<RMP_Connection> Connections { get; protected set; }

		public RMP_Neuron()
			: base("RMP_Neuron")
		{
			Connections = new List<RMP_Connection>();
		}
		public RMP_Neuron(RMP_Net net)
			: base("RMP_Neuron")
		{
			Net = net;
			Connections = new List<RMP_Connection>();
		}

		public virtual void Update()
		{
			Output = SigmoidFunction.Sigmoid(Activation - Bias);
			Activation = 0;

            foreach (var connection in Connections)
            {
                connection.OutputToTarget();
            }
		}

		public void ImprintGene(RMP_NeuronGene gene)
		{
			Name = gene.Name;
			GeneID = gene.ID;

			Bias = gene.Bias.Value;

			SigmoidFunction = (SigmoidFunction)gene.SigmoidFunction.Clone();
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			//Net
			Net.Save(w);
			w.Write(GeneID);

			w.Write(Bias);

			SigmoidFunction.Save(w);

			w.Write(Activation);
			w.Write(Output);

			WriteListInfo(Connections, w);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			Net = BinarySerializable.GetObject<RMP_Net>(r);
			GeneID = r.ReadUInt32();

			Bias = r.ReadDouble();

			SigmoidFunction = BinarySerializable.GetObject<SigmoidFunction>(r);

			Activation = r.ReadDouble();
			Output = r.ReadDouble();

			LoadListInfo(Connections, r);
		}
	}
}
