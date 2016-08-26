using System;
using System.Collections.Generic;
using System.Diagnostics;
using EvoSim.RandomNumberGenerators;
using EvoSim.Genes;
using System.Collections.ObjectModel;
using System.IO;
using EvoSim;

namespace EvoApp.NeuralNets.RMP
{
	[DebuggerDisplay("Weight={Weight}")]
	public class RMP_Connection : BinarySerializable, IConnection
	{
		[DebuggerDisplay("ID={Source.GeneID}")]
		public RMP_Neuron Source;
		[DebuggerDisplay("ID={Target.GeneID}")]
		public RMP_Neuron Target;

		public double Weight { get; set; }

		public RMP_Connection()
			 : base("Connection")
		{
		}

		public void OutputToTarget()
		{
			Target.Activation += Source.Output * Weight;
		}

		public void ImprintGene(RMP_ConnectionGene gene, RMP_Neuron source, RMP_Net net)
		{
			RMP_Neuron target;
			if (gene.IsOutputConnection)
				target = net.OutputNeurons[gene.Target];
			else
				target = net.HiddenNeurons[gene.Target];

			Source = source;
			Target = target;

			Weight = gene.Weight.Value;
		}
		public static RMP_Connection GetConnectionFromGene(RMP_ConnectionGene gene, RMP_Neuron source, RMP_Net net)
		{
			var connection = new RMP_Connection();
			connection.ImprintGene(gene, source, net);

			return connection;
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			Source.Save(w);
			Target.Save(w);

			w.Write(Weight);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			Source = BinarySerializable.GetObject<RMP_Neuron>(r);
			Target = BinarySerializable.GetObject<RMP_Neuron>(r);

			Weight = r.ReadDouble();
		}
	}
}
