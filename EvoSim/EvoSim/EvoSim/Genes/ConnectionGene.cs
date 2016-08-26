using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;

//TODO:Remove the IntGenes, all the int-values will be decided when the doubles are mutliplied by a max.

namespace EvoSim.Genes
{
	[DebuggerDisplay("ID={ID}, Strength={Strength.Value}, Delay={Delay.Value}")]
	public class ConnectionGene : BinarySerializable
	{
		public int Target;
		public bool IsOutputConnection;

		public DoubleMinMax Strength;
		public DoubleMinMax Delay;

		public ConnectionGene()
			: base("ConnectionGene")
		{
			initialize();
		}

		void initialize()
		{
			Strength = new DoubleMinMax(-1, 1);
			Delay = new DoubleMinMax(0, 1);
		}

		public ConnectionGene Clone()
		{
			var gene = new ConnectionGene();

			gene.Target = Target;

			gene.Strength = Strength;
			gene.Delay = Delay;

			return gene;
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(Target);
			w.Write(IsOutputConnection);

			Strength.WriteTo(w);
			Delay.WriteTo(w);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			Target = r.ReadInt32();
			IsOutputConnection = r.ReadBoolean();

			Strength = DoubleMinMax.Read(r);
			Delay = DoubleMinMax.Read(r);
		}
	}
}
