using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoSim.Genes;
using System.Collections.ObjectModel;
using System.Xml;
using System.IO;

namespace EvoSim.Genes
{
	public abstract class GeneModule : BinarySerializable, IGeneInfoHolder
	{
		public bool IsMutable = true;

		public ReadOnlyCollection<Gene> GeneList;
		protected List<Gene> geneList = new List<Gene>();

		public GeneModule()
			: base("GeneModule")
		{
			GeneList = new ReadOnlyCollection<Gene>(geneList);
		}

		public virtual void TryToMutate(double mutationChance)
		{
			if (!IsMutable)
				return;

			foreach (var gene in geneList)
				gene.TryToMutate(mutationChance);

			GenesHasChanged();
		}

		public virtual void Randomize()
		{
			if (!IsMutable)
				return;

			foreach (DoubleGene gene in geneList)
				gene.Randomize();

			GenesHasChanged();
		}

		public virtual GeneModule CrossoverModule(GeneModule g2, CrossoverFunction crossoverFunction)
		{
			var child = GetNewThis();

			crossoverFunction.Crossover(geneList, g2.geneList, child.geneList);
			child.GenesHasChanged();

			return child;
		}

		public virtual GeneModule Clone()
		{
			var clone = GetNewThis();
			clone.geneList.Clear();
			foreach (var gene in geneList)
				clone.geneList.Add(gene.Clone());

			GenesHasChanged();

			return clone;
		}

		protected abstract GeneModule GetNewThis();

		public virtual void GenesHasChanged() { }

		public GeneInfo GetGeneInfo()
		{
			var dict = new GeneInfo();

			dict.AddGeneList("Genes", geneList);

			return dict;
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			w.Write(IsMutable);

			WriteListInfo(geneList, w); 
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			IsMutable = r.ReadBoolean();

			LoadListInfo(geneList, r);
		}
	}
}