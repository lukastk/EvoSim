using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Xml;
using System.IO;

namespace EvoSim.Genes
{
	public class Genome : BinarySerializable, IGeneInfoHolder
	{
		public bool IsFirstGeneration = true;
		public uint Parent1;
		public uint Parent2;

		public List<DoubleGene> BodyGenes { get; private set; }
		public List<DoubleGene> MutationGenes { get; private set; }

		public INeuralNetChromosome NetChromosome { get; private set; }

		public DoubleGene CrossoverRate { get { return MutationGenes[0]; } }
		public DoubleGene MutationChance { get { return MutationGenes[1]; } }

		public CrossoverFunction CrossoverFunction;

		public Genome()
			: base("Genome")
		{
			BodyGenes = new List<DoubleGene>();
			MutationGenes = new List<DoubleGene>();
		}

		public Genome CrossoverWith(Genome g2)
		{
			Genome child = new Genome();
			child.IsFirstGeneration = false;
			child.Parent1 = this.ID;
			child.Parent2 = g2.ID;

			CrossoverFunction.Crossover(BodyGenes, g2.BodyGenes, child.BodyGenes);
			CrossoverFunction.Crossover(MutationGenes, g2.MutationGenes, child.MutationGenes);

			child.SetNetChromosome(NetChromosome.CrossoverWith(g2.NetChromosome, CrossoverFunction));

			child.CrossoverFunction = (CrossoverFunction)CrossoverFunction.CrossoverModule(g2.CrossoverFunction, CrossoverFunction);
			
			return child;
		}
		public void Mutate()
		{
			MutateGeneList(BodyGenes,			MutationChance.Value);
			MutateGeneList(MutationGenes,		MutationChance.Value);

			CrossoverFunction.TryToMutate(MutationChance.Value);

			NetChromosome.Mutate();
		}

		public Genome GetChildGenome(Genome g2)
		{
			var child = CrossoverWith(g2);

			child.Mutate();

			return child;
		}

		public void Randomize()
		{
			foreach (var g in MutationGenes)
				g.Randomize();

			foreach (var g in BodyGenes)
				g.Randomize();

			NetChromosome.Randomize();

			CrossoverFunction.Randomize();
		}

		public void SetNetChromosome(INeuralNetChromosome netChromosome)
		{
			NetChromosome = netChromosome;
			netChromosome.Initialize(this);
		}

		static void MutateGeneList<T>(List<T> geneList, double mutationChance) where T : Gene
		{
			foreach (var gene in geneList)
				gene.TryToMutate(mutationChance);
		}

		public GeneInfo GetGeneInfo()
		{
			var dict = new GeneInfo();

			dict.AddGeneList("Body", BodyGenes);
			dict.AddGeneList("Mutation", MutationGenes);

			dict.AddMiscGeneInfo(NetChromosome.GetGeneInfo());

			return dict;
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(IsFirstGeneration);
			w.Write(Parent1);
			w.Write(Parent2);

			WriteListInfo(BodyGenes, w);

			WriteListInfo(MutationGenes, w);

			WriteNullableObject(NetChromosome, w);
			WriteNullableObject(CrossoverFunction, w);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			IsFirstGeneration = r.ReadBoolean();
			Parent1 = r.ReadUInt32();
			Parent2 = r.ReadUInt32();

			LoadListInfo(BodyGenes, r);

			LoadListInfo(MutationGenes, r);

			NetChromosome = ReadNullableObject<INeuralNetChromosome>(r);
			CrossoverFunction = ReadNullableObject<CrossoverFunction>(r);
		}

		public Genome Clone()
		{
			var clone = new Genome();

			clone.IsFirstGeneration = IsFirstGeneration;
			clone.Parent1 = Parent1;
			clone.Parent2 = Parent2;

			foreach (var gene in BodyGenes)
				clone.BodyGenes.Add((DoubleGene)gene.Clone());

			foreach (var gene in MutationGenes)
				clone.MutationGenes.Add((DoubleGene)gene.Clone());

			if (NetChromosome != null)
				clone.NetChromosome = NetChromosome.Clone();

			if (CrossoverFunction != null)
				clone.CrossoverFunction = (CrossoverFunction)CrossoverFunction.Clone();

			return clone;
		}
	}
}