using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace EvoSim.Genes
{
	public interface INeuralNetChromosome : IBinarySerializable, IGeneInfoHolder
	{
		int EyeNeuronsCount { get; }

		DoubleGene CrossoverRate { get; }
		DoubleGene MutationChance { get; }

		IList<BinarySerializable> Inputs { get; }
		IList<BinarySerializable> Hidden { get; }
		IList<BinarySerializable> Outputs { get; }

		void Initialize(Genome genome);
		INeuralNet GetNeuralNet();
		void Randomize();
		void CreateRandom(int eyeNeurons, int inputs, int outputs, int hidden);
		INeuralNetChromosome CrossoverWith(INeuralNetChromosome c2, CrossoverFunction crossoverFunction);
		void Mutate();

		INeuralNetChromosome Clone();
	}

	public abstract class NeuralNetChromosome : BinarySerializable, INeuralNetChromosome
	{
		protected Genome Genome;

		public abstract int EyeNeuronsCount { get; }

		public DoubleGene CrossoverRate { get { return Genome.CrossoverRate; } }
		public DoubleGene MutationChance { get { return Genome.MutationChance; } }

		public abstract IList<BinarySerializable> Inputs { get; }
		public abstract IList<BinarySerializable> Hidden { get; }
		public abstract IList<BinarySerializable> Outputs { get; }

		public NeuralNetChromosome()
			: base("NeuralNetChromosome")
		{
		}

		public void Initialize(Genome genome)
		{
			if (genome == null)
				throw new NullReferenceException();

			Genome = genome;
		}

		public abstract INeuralNet GetNeuralNet();

		public abstract void Randomize();
		public abstract void CreateRandom(int eyeNeurons, int inputs, int outputs, int hidden);

		public abstract INeuralNetChromosome CrossoverWith(INeuralNetChromosome c2, CrossoverFunction crossoverFunction);
		public abstract void Mutate();

		protected static void MutateGeneList<T>(List<T> geneList, double mutationChance) where T : Gene
		{
			foreach (var gene in geneList)
				gene.TryToMutate(mutationChance);
		}

		public abstract GeneInfo GetGeneInfo();

		public abstract INeuralNetChromosome Clone();

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);
		}
	}
}
