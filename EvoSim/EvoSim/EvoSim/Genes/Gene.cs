using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace EvoSim.Genes
{
	public abstract class Gene : BinarySerializable, ICloneable, IGeneInfoHolder
	{
		public string GeneHeritage;
		public bool IsMutable = true;

		public Gene(string name)
			: base(name)
		{
			GeneHeritage = ID.ToString();
		}
		public Gene(string name, string geneHeritage)
			: base(name)
		{
			GeneHeritage = geneHeritage;
		}

		public void TryToMutate(double mutationChance)
		{
			if (IsMutable)
				Mutate(mutationChance);
		}

		public abstract void Mutate(double mutationChance);

		public abstract Gene Clone();

		object ICloneable.Clone()
		{
			return Clone();
		}

		public abstract GeneInfo GetGeneInfo();

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(GeneHeritage);
			w.Write(IsMutable);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			GeneHeritage = r.ReadString();
			IsMutable = r.ReadBoolean();
		}
	}
}
