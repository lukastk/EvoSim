using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using EvoSim.Genes;
using EvoSim;

namespace EvoApp.NeuralNets.SpikeNet
{
	public class SpikeNetChromosome : NeuralNetChromosome
	{
		public override int EyeNeuronsCount
		{
			get { return DistanceNeuronGenes.Count; }
		}

		public List<SpNeuronGene> EyeRNeuronGenes { get; private set; }
		public List<SpNeuronGene> EyeGNeuronGenes { get; private set; }
		public List<SpNeuronGene> EyeBNeuronGenes { get; private set; }
		public List<SpNeuronGene> DistanceNeuronGenes { get; private set; }

		public List<SpNeuronGene> HiddenNeuronGenes { get; private set; }
		public List<SpNeuronGene> InputNeuronGenes { get; private set; }
		public List<SpNeuronGene> OutputNeuronGenes { get; private set; }

		public List<DoubleGene> MutationGenes { get; private set; }
		public DoubleGene NeuronAddChance { get { return MutationGenes[0]; } }
		public DoubleGene NeuronRemoveChance { get { return MutationGenes[1]; } }
		public DoubleGene MaxNeuronRemoving { get { return MutationGenes[2]; } }
		public DoubleGene MaxNeuronAdding { get { return MutationGenes[3]; } }
		public DoubleGene ConnectionAddChance { get { return MutationGenes[4]; } }
		public DoubleGene ConnectionRemoveChance { get { return MutationGenes[5]; } }

		public static double InhibitoryConnectionChance = 0.3;

		public SpikeNetChromosome()
		{
			EyeRNeuronGenes = new List<SpNeuronGene>();
			EyeGNeuronGenes = new List<SpNeuronGene>();
			EyeBNeuronGenes = new List<SpNeuronGene>();
			DistanceNeuronGenes = new List<SpNeuronGene>();

			HiddenNeuronGenes = new List<SpNeuronGene>();
			InputNeuronGenes = new List<SpNeuronGene>();
			OutputNeuronGenes = new List<SpNeuronGene>();

			MutationGenes = new List<DoubleGene>();
		}

		public override IList<BinarySerializable> Inputs
		{
			get
			{
				var inputs = new List<BinarySerializable>();

				inputs.AddRange(EyeRNeuronGenes);
				inputs.AddRange(EyeGNeuronGenes);
				inputs.AddRange(EyeBNeuronGenes);
				inputs.AddRange(DistanceNeuronGenes);

				inputs.AddRange(InputNeuronGenes);

				return inputs;
			}
		}
		public override IList<BinarySerializable> Hidden
		{
			get
			{
				return new List<BinarySerializable>(InputNeuronGenes);
			}
		}
		public override IList<BinarySerializable> Outputs
		{
			get
			{
				return new List<BinarySerializable>(OutputNeuronGenes);
			}
		}

		public override INeuralNet GetNeuralNet()
		{
			var neuralNet = new SpikeNet();

			neuralNet.ImprintGenome(this);

			return neuralNet;
		}

		public override void CreateRandom(int eyeNeurons, int inputs, int outputs, int hidden)
		{
			MutationGenes.Clear();

			MutationGenes.Add(new DoubleGene("NeuronAddChance", 0, 1, 0.1));
			MutationGenes.Add(new DoubleGene("NeuronRemoveChance", 0, 1, 0.1));
			MutationGenes.Add(new DoubleGene("MaxNeuronRemoving", 0, 1, 0.1));
			MutationGenes.Add(new DoubleGene("MaxNeuronAdding", 0, 1, 0.1));
			MutationGenes.Add(new DoubleGene("ConnectionAddChance", 0, 1, 0.1));
			MutationGenes.Add(new DoubleGene("ConnectionRemoveChance", 0, 1, 0.1));

			EyeRNeuronGenes.Clear();
			EyeGNeuronGenes.Clear();
			EyeBNeuronGenes.Clear();
			DistanceNeuronGenes.Clear();

			for (int i = 0; i < eyeNeurons; i++)
				EyeRNeuronGenes.Add(GetRandomNeuronGene("Red"));
			for (int i = 0; i < eyeNeurons; i++)
				EyeGNeuronGenes.Add(GetRandomNeuronGene("Green"));
			for (int i = 0; i < eyeNeurons; i++)
				EyeBNeuronGenes.Add(GetRandomNeuronGene("Blue"));
			for (int i = 0; i < eyeNeurons; i++)
				DistanceNeuronGenes.Add(GetRandomNeuronGene("Distance"));

			HiddenNeuronGenes.Clear();
			InputNeuronGenes.Clear();
			OutputNeuronGenes.Clear();

			for (int n = 0; n < hidden; n++)
				HiddenNeuronGenes.Add(GetRandomNeuronGene("Hidden Neuron"));
			for (int n = 0; n < inputs; n++)
				InputNeuronGenes.Add(GetRandomNeuronGene("Input Neuron"));
			for (int n = 0; n < outputs; n++)
				OutputNeuronGenes.Add(GetRandomNeuronGene("Output Neuron"));

			foreach (var gene in EyeRNeuronGenes)
				ConnectRandomly(gene, true);
			foreach (var gene in EyeGNeuronGenes)
				ConnectRandomly(gene, true);
			foreach (var gene in EyeBNeuronGenes)
				ConnectRandomly(gene, true);
			foreach (var gene in DistanceNeuronGenes)
				ConnectRandomly(gene, true);

			foreach (var gene in HiddenNeuronGenes)
				ConnectRandomly(gene, false);
			foreach (var gene in InputNeuronGenes)
				ConnectRandomly(gene, true);
		}

		public override INeuralNetChromosome CrossoverWith(INeuralNetChromosome netC2, CrossoverFunction crossoverFunction)
		{
			var c2 = (SpikeNetChromosome)netC2;
			var child = new SpikeNetChromosome();

			//CrossoverTwoGeneLists(MutationGenes, c2.MutationGenes, CrossoverRate.Value, child.MutationGenes);

			//CrossoverTwoGeneLists(EyeRNeuronGenes, c2.EyeRNeuronGenes, CrossoverRate.Value, child.EyeRNeuronGenes);
			//CrossoverTwoGeneLists(EyeGNeuronGenes, c2.EyeGNeuronGenes, CrossoverRate.Value, child.EyeGNeuronGenes);
			//CrossoverTwoGeneLists(EyeBNeuronGenes, c2.EyeBNeuronGenes, CrossoverRate.Value, child.EyeBNeuronGenes);
			//CrossoverTwoGeneLists(DistanceNeuronGenes, c2.DistanceNeuronGenes, CrossoverRate.Value, child.DistanceNeuronGenes);

			//CrossoverTwoGeneLists(HiddenNeuronGenes, c2.HiddenNeuronGenes, CrossoverRate.Value, child.HiddenNeuronGenes);
			//CrossoverTwoGeneLists(InputNeuronGenes, c2.InputNeuronGenes, CrossoverRate.Value, child.InputNeuronGenes);
			//CrossoverTwoGeneLists(OutputNeuronGenes, c2.OutputNeuronGenes, CrossoverRate.Value, child.OutputNeuronGenes);

			return (INeuralNetChromosome)child;
		}
		public override void Mutate()
		{
			MutateGeneList(EyeRNeuronGenes, MutationChance.Value);
			MutateGeneList(EyeGNeuronGenes, MutationChance.Value);
			MutateGeneList(EyeBNeuronGenes, MutationChance.Value);
			MutateGeneList(DistanceNeuronGenes, MutationChance.Value);

			MutateGeneList(HiddenNeuronGenes, MutationChance.Value);
			MutateGeneList(InputNeuronGenes, MutationChance.Value);
			MutateGeneList(OutputNeuronGenes, MutationChance.Value);

			Mutate_RemoveNeurons();
			Mutate_AddNeurons();

			//Mutate_RemoveEyeNeurons();
			//Mutate_AddEyeNeurons();
			//Mutate_RemoveConnections();
			//Mutate_AddConnections();
		}
		public void Mutate_RemoveNeurons()
		{
			var neuronMap = new Dictionary<int, SpNeuronGene>();
			//Remove Neurons
			if (NeuronRemoveChance.Value > Pseudo.Random.NextDouble())
			{
				int count = 0;
				foreach (var neuron in HiddenNeuronGenes)
				{
					neuronMap.Add(count, neuron);
					count++;
				}

				int removeCount = Pseudo.Random.Next((int)Math.Round(HiddenNeuronGenes.Count * MaxNeuronRemoving.Value, 0));

				for (int n = 0; n < removeCount; n++)
				{
					int neuronIndex;

					do
					{
						neuronIndex = Pseudo.Random.Next(HiddenNeuronGenes.Count);
					} while (HiddenNeuronGenes[neuronIndex] == null);

					var gene = HiddenNeuronGenes[neuronIndex];

					HiddenNeuronGenes[neuronIndex] = null;
					neuronMap.Remove(neuronIndex);
				}

				HiddenNeuronGenes.Clear();
				HiddenNeuronGenes.AddRange(from key in neuronMap.Keys orderby key select neuronMap[key]);

				foreach (var neuronGene in neuronMap.Values.Concat(EyeRNeuronGenes).Concat(EyeGNeuronGenes).Concat(EyeBNeuronGenes).Concat(InputNeuronGenes))
				{
					foreach (var connection in new List<SpConnectionGene>(neuronGene.Connections))
					{
						if (!connection.IsOutputConnection && neuronMap.ContainsKey(connection.Target))
						{
							var target = neuronMap[connection.Target];
							connection.Target = HiddenNeuronGenes.IndexOf(target);
						}
						else
							neuronGene.Connections.Remove(connection);
					}
				}
			}
		}
		public void Mutate_AddNeurons()
		{
			//Add Neurons
			if (NeuronAddChance.Value > Pseudo.Random.NextDouble())
			{
				int neuronCount = Pseudo.Random.Next((int)Math.Round(HiddenNeuronGenes.Count * MaxNeuronAdding.Value));

				for (int n = 0; n < neuronCount; n++)
					HiddenNeuronGenes.Add(GetRandomNeuronGene("Hidden RMP_Neuron"));
			}
		}

		public SpNeuronGene GetRandomNeuronGene(string name)
		{
			var gene = new SpNeuronGene(name);
			gene.Threshold.Value =
				Pseudo.Random.NextDouble(gene.Threshold.Min, gene.Threshold.Max);
			gene.ExcitationDecayRate.Value =
				Pseudo.Random.NextDouble(gene.ExcitationDecayRate.Min, gene.ExcitationDecayRate.Max);
			gene.AbsoluteRefactoryPeriod.Value =
				Pseudo.Random.NextDouble(gene.AbsoluteRefactoryPeriod.Min, gene.AbsoluteRefactoryPeriod.Max);
			gene.RelativeRefactoryPeriod.Value =
				Pseudo.Random.NextDouble(gene.RelativeRefactoryPeriod.Min, gene.RelativeRefactoryPeriod.Max);
			gene.RefactoryPenalty.Value =
				Pseudo.Random.NextDouble(gene.RefactoryPenalty.Min, gene.RefactoryPenalty.Max);

			return gene;
		}
		public void ConnectRandomly(SpNeuronGene gene, bool isInputNeuron)
		{
			int connectionCount;

			if (isInputNeuron)
				connectionCount = HiddenNeuronGenes.Count;
			else
				connectionCount = HiddenNeuronGenes.Count + OutputNeuronGenes.Count;

			int connectionAmount = Pseudo.Random.Next(connectionCount);

			for (int i = 0; i < connectionAmount; i++)
			{
				int targetIndex;
				bool unique;

				do
				{
					targetIndex = Pseudo.Random.Next(connectionCount);

					unique = true;
					foreach (SpConnectionGene connectionGene in gene.Connections)
						if (connectionGene.Target == targetIndex)
							unique = false;
				} while (!unique);

				if (targetIndex < HiddenNeuronGenes.Count)
					gene.Connections.Add(GetRandomConnection(targetIndex, false));
				else
					gene.Connections.Add(GetRandomConnection(targetIndex - HiddenNeuronGenes.Count, true));
			}
		}
		public SpConnectionGene GetRandomConnection(int target, bool isOutputConnection)
		{
			var gene = new SpConnectionGene();

			double strength = Pseudo.Random.NextDouble(0, 1);
			if (InhibitoryConnectionChance > Pseudo.Random.NextDouble())
				strength = -strength;

			gene.Strength.Value = strength;
			gene.Delay.Value =
				Pseudo.Random.NextDouble(gene.Delay.Min, gene.Delay.Max);

			gene.Target = target;
			gene.IsOutputConnection = isOutputConnection;

			return gene;
		}

		public override GeneInfo GetGeneInfo()
		{
			throw new NotImplementedException();
		}

		public override INeuralNetChromosome Clone()
		{
			throw new NotImplementedException();
		}

		public override void Randomize()
		{
			throw new NotImplementedException();
		}
	}

	[DebuggerDisplay("ID={ID}, Bias={Bias.Value}, ExcitationDecayRate={ExcitationDecayRate.Value}")]
	public class SpNeuronGene : Gene
	{
		public DoubleMinMax Threshold;
		public DoubleMinMax ExcitationDecayRate;
		public DoubleMinMax AbsoluteRefactoryPeriod;
		public DoubleMinMax RelativeRefactoryPeriod;
		public DoubleMinMax RefactoryPenalty;

		public List<SpConnectionGene> Connections = new List<SpConnectionGene>();

		public SpNeuronGene(string name)
			: base(name)
		{
			initialize();
		}
		public SpNeuronGene(string name, string id)
			: base(name, id)
		{
			initialize();
		}

		void initialize()
		{
			Threshold = new DoubleMinMax(0, 1);
			ExcitationDecayRate = new DoubleMinMax(0, 1);
			AbsoluteRefactoryPeriod = new DoubleMinMax(0, 1);
			RelativeRefactoryPeriod = new DoubleMinMax(0, 1);
			RefactoryPenalty = new DoubleMinMax(0, 1);
		}

		public override void Mutate(double mutationChance)
		{
			MutateValue(Threshold, mutationChance);
			MutateValue(ExcitationDecayRate, mutationChance);
			MutateValue(AbsoluteRefactoryPeriod, mutationChance);
			MutateValue(RelativeRefactoryPeriod, mutationChance);
			MutateValue(RefactoryPenalty, mutationChance);

			foreach (var connectionGene in Connections)
			{
				MutateValue(connectionGene.Strength, mutationChance);
				MutateValue(connectionGene.Delay, mutationChance);
			}
		}

		static void MutateValue(DoubleMinMax value, double mutationChance)
		{
			if (mutationChance < Pseudo.Random.NextDouble())
				return;

			double step = value.Max - value.Min;

			value.Value += Pseudo.GaussianRandom.NextDouble(-step, step);
		}

		public override Gene Clone()
		{
			var gene = new SpNeuronGene(Name, GeneHeritage);

			gene.Threshold = Threshold;
			gene.ExcitationDecayRate = ExcitationDecayRate;
			gene.AbsoluteRefactoryPeriod = AbsoluteRefactoryPeriod;
			gene.RelativeRefactoryPeriod = RelativeRefactoryPeriod;
			gene.RefactoryPenalty = RefactoryPenalty;

			foreach (var connection in Connections)
				gene.Connections.Add(connection.Clone());

			return gene;
		}

		public override GeneInfo GetGeneInfo()
		{
			throw new NotImplementedException();
		}
	}

	[DebuggerDisplay("ID={ID}, Weight={Weight.Value}, Delay={Delay.Value}")]
	public class SpConnectionGene
	{
		public int Target;
		public bool IsOutputConnection;

		public DoubleMinMax Strength;
		public DoubleMinMax Delay;

		public SpConnectionGene()
		{
			initialize();
		}

		void initialize()
		{
			Strength = new DoubleMinMax(-1, 1);
			Delay = new DoubleMinMax(0, 1);
		}

		public SpConnectionGene Clone()
		{
			var gene = new SpConnectionGene();

			gene.Target = Target;

			gene.Strength = Strength;
			gene.Delay = Delay;

			return gene;
		}
	}
}
