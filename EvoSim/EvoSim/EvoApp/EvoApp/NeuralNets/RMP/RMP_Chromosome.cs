using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using EvoSim.Genes;
using EvoSim;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;
using TakaGUI.DrawBoxes;
using System.Reflection;
using EvoApp.NeuralNets.RMP.Forms;
using EvoApp.Forms;

namespace EvoApp.NeuralNets.RMP
{
	public class RMP_Chromosome : NeuralNetChromosome
	{
		public double InhibitoryConnectionChance;
		public double ConnectionChance;

		public bool NewConnectionsCanForm;
		public bool ConnectionsCanDie;
		public bool NewNeuronsCanForm;
		public bool NeuronsCanDie;

		public override int EyeNeuronsCount
		{
			get { return DistanceNeuronGenes.Count; }
		}

		public static Func<bool> GUI_Edit(SingleSlotBox container, RMP_Chromosome chromosome)
		{
			var builder = new FieldBuilder();

			builder.BuildSessionStart(container);

			Dictionary<string, object> valueHolder = new Dictionary<string, object>();
			valueHolder.Add("RMP_Chromosome", chromosome);

			var inhibitoryConnectionChance = builder.AddDoubleField("Inhibitory Connection-Chance: ");
			inhibitoryConnectionChance.Value = chromosome.InhibitoryConnectionChance;
			var connectionChance = builder.AddDoubleField("Connection-Chance: ");
			connectionChance.Value = chromosome.ConnectionChance;

			var newConnectionsCanForm = builder.AddCheckBoxField("NewConnectionsCanForm: ");
			newConnectionsCanForm.Checked = chromosome.NewConnectionsCanForm;
			var connectionsCanDie = builder.AddCheckBoxField("ConnectionsCanDie: ");
			connectionsCanDie.Checked = chromosome.ConnectionsCanDie;
			var newNeuronsCanForm = builder.AddCheckBoxField("NewNeuronsCanForm: ");
			newNeuronsCanForm.Checked = chromosome.NewNeuronsCanForm;
			var neuronsCanDie = builder.AddCheckBoxField("NeuronsCanDie: ");
			neuronsCanDie.Checked = chromosome.NeuronsCanDie;

			Action reloadChromosome = delegate()
			{
				chromosome.InhibitoryConnectionChance = inhibitoryConnectionChance.Value;
				chromosome.ConnectionChance = connectionChance.Value;

				chromosome.NewConnectionsCanForm = newConnectionsCanForm.Checked;
				chromosome.ConnectionsCanDie = connectionsCanDie.Checked;
				chromosome.NewNeuronsCanForm = newNeuronsCanForm.Checked;
				chromosome.NeuronsCanDie = neuronsCanDie.Checked;
			};

			if (chromosome.MutationGenes.Count == 0)
			{
				//Mutation Genes
				chromosome.MutationGenes.Add(new DoubleGene("NeuronAddChance", 0, 1, 0.1));
				chromosome.MutationGenes.Add(new DoubleGene("NeuronRemoveChance", 0, 1, 0.1));
				chromosome.MutationGenes.Add(new DoubleGene("MaxNeuronRemoving", 0, 1, 0.1));
				chromosome.MutationGenes.Add(new DoubleGene("MaxNeuronAdding", 0, 1, 0.1));
				chromosome.MutationGenes.Add(new DoubleGene("ConnectionAddChance", 0, 1, 0.05));
				chromosome.MutationGenes.Add(new DoubleGene("ConnectionRemoveChance", 0, 1, 0.05));
			}
			builder.AddResizableButtonField("Edit Mutation Genes", delegate(object sender)
			{
				EditDoubleGeneListForm.ShowDialogue(container.Parent, chromosome.MutationGenes);
			});

			var sigmoidTypes = Globals.GetAllTypesDeriving(typeof(SigmoidFunction), Assembly.GetExecutingAssembly());
			var sigmoidNames = new List<string>(sigmoidTypes.Select(s => s.Name));

			var globalSigmoidComboBox = builder.AddComboBoxField("GlobalSigmoid: ", sigmoidNames);
			if (chromosome.GlobalSigmoidFunction != null)
			{
				var globalSigmoidType = chromosome.GlobalSigmoidFunction.GetType();
				foreach (var type in sigmoidTypes)
					if (type.IsEquivalentTo(globalSigmoidType))
					{
						globalSigmoidComboBox.Index = sigmoidTypes.IndexOf(type);
					}
			}

			globalSigmoidComboBox.SelectedItemChanged += delegate(object sender, int newItemIndex, int oldItemIndex)
			{
				chromosome.GlobalSigmoidFunction = (SigmoidFunction)Activator.CreateInstance(sigmoidTypes[newItemIndex]);
			};
			var globalSigmoidEditButton = builder.AddResizableButtonField("Edit GlobalSigmoid", delegate(object sender)
			{
				if (chromosome.GlobalSigmoidFunction == null)
					return;

				var sigmoid = chromosome.GlobalSigmoidFunction;
				sigmoidTypes[globalSigmoidComboBox.Index].InvokeMember("GUI_Edit", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { container.Parent, sigmoid });
			});

			var globalOutputSigmoidComboBox = builder.AddComboBoxField("GlobalOutputSigmoid: ", sigmoidNames);
			if (chromosome.GlobalOutputSigmoidFunction != null)
			{
				var globalOutputSigmoidType = chromosome.GlobalOutputSigmoidFunction.GetType();
				foreach (var type in sigmoidTypes)
					if (type.IsEquivalentTo(globalOutputSigmoidType))
					{
						globalOutputSigmoidComboBox.Index = sigmoidTypes.IndexOf(type);
					}
			}

			globalOutputSigmoidComboBox.SelectedItemChanged += delegate(object sender, int newItemIndex, int oldItemIndex)
			{
				chromosome.GlobalOutputSigmoidFunction = (SigmoidFunction)Activator.CreateInstance(sigmoidTypes[newItemIndex]);
			};
			var globalOutputSigmoidEditButton = builder.AddResizableButtonField("Edit GlobalOutputSigmoid", delegate(object sender)
			{
				if (chromosome.GlobalOutputSigmoidFunction == null)
					return;

				var sigmoid = chromosome.GlobalOutputSigmoidFunction;
				sigmoidTypes[globalOutputSigmoidComboBox.Index].InvokeMember("GUI_Edit", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { container.Parent, sigmoid });
			});

			builder.AddResizableButtonField("Randomize", delegate(object sender)
			{
				reloadChromosome();

				if (chromosome.GlobalSigmoidFunction == null ||
					chromosome.GlobalOutputSigmoidFunction == null)
					AlertForm.ShowDialogue(container.Parent, null, "Can't randomize before choosing a sigmoid function.");
				else
					RandomizeForm.ShowDialogue(container.Parent, chromosome);
			});

			builder.AddResizableButtonField("Edit Neuron-genes", delegate(object sender)
			{
				reloadChromosome();

				EditChromosomeNeuronsForm.ShowDialogue(container.Parent, valueHolder);
			});

			container.IsClosing += delegate(object sender)
			{
				reloadChromosome();
			};

			builder.BuildSessionEnd();

			return delegate()
			{
				return chromosome.GlobalSigmoidFunction != null && chromosome.GlobalOutputSigmoidFunction != null;
			};
		}

		/// <summary>
		/// Used to create sigmoids for new neurons.
		/// </summary>
		public SigmoidFunction GlobalSigmoidFunction { get; set; }
		public SigmoidFunction GlobalOutputSigmoidFunction { get; set; }

		public List<RMP_NeuronGene> EyeRNeuronGenes { get; private set; }
		public List<RMP_NeuronGene> EyeGNeuronGenes { get; private set; }
		public List<RMP_NeuronGene> EyeBNeuronGenes { get; private set; }
		public List<RMP_NeuronGene> DistanceNeuronGenes { get; private set; }

		public List<RMP_NeuronGene> HiddenNeuronGenes { get; private set; }
		public List<RMP_NeuronGene> InputNeuronGenes { get; private set; }
		public List<RMP_NeuronGene> OutputNeuronGenes { get; private set; }

		public List<DoubleGene> MutationGenes { get; private set; }
		public DoubleGene NeuronAddChance { get { return MutationGenes[0]; } }
		public DoubleGene NeuronRemoveChance { get { return MutationGenes[1]; } }
		public DoubleGene ConnectionAddChance { get { return MutationGenes[4]; } }
		public DoubleGene ConnectionRemoveChance { get { return MutationGenes[5]; } }

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
				return new List<BinarySerializable>(HiddenNeuronGenes);
			}
		}
		public override IList<BinarySerializable> Outputs
		{
			get
			{
				return new List<BinarySerializable>(OutputNeuronGenes);
			}
		}

		public RMP_Chromosome()
		{
			EyeRNeuronGenes = new List<RMP_NeuronGene>();
			EyeGNeuronGenes = new List<RMP_NeuronGene>();
			EyeBNeuronGenes = new List<RMP_NeuronGene>();
			DistanceNeuronGenes = new List<RMP_NeuronGene>();

			HiddenNeuronGenes = new List<RMP_NeuronGene>();
			InputNeuronGenes = new List<RMP_NeuronGene>();
			OutputNeuronGenes = new List<RMP_NeuronGene>();

			MutationGenes = new List<DoubleGene>();
		}

		public override INeuralNet GetNeuralNet()
		{
			var neuralNet = new RMP_Net();

			neuralNet.ImprintGenome(this);

			return neuralNet;
		}

		public override void Randomize()
		{
			foreach (var g in MutationGenes)
				g.Randomize();

			foreach (var n in EyeRNeuronGenes)
				n.Randomize();
			foreach (var n in EyeGNeuronGenes)
				n.Randomize();
			foreach (var n in EyeBNeuronGenes)
				n.Randomize();
			foreach (var n in DistanceNeuronGenes)
				n.Randomize();

			foreach (var n in InputNeuronGenes)
				n.Randomize();
			foreach (var n in OutputNeuronGenes)
				n.Randomize();
			foreach (var n in HiddenNeuronGenes)
				n.Randomize();

			AllNeuronsConnectRandomly();
		}
		public override void CreateRandom(int eyeNeurons, int inputs, int outputs, int hidden)
		{
			MutationGenes.Clear();

			MutationGenes.Add(new DoubleGene("NeuronAddChance", 0, 1, 0.1));
			MutationGenes.Add(new DoubleGene("NeuronRemoveChance", 0, 1, 0.1));
			MutationGenes.Add(new DoubleGene("MaxNeuronRemoving", 0, 1, 0.1));
			MutationGenes.Add(new DoubleGene("MaxNeuronAdding", 0, 1, 0.1));
			MutationGenes.Add(new DoubleGene("ConnectionAddChance", 0, 1, 0.05));
			MutationGenes.Add(new DoubleGene("ConnectionRemoveChance", 0, 1, 0.05));

			EyeRNeuronGenes.Clear();
			EyeGNeuronGenes.Clear();
			EyeBNeuronGenes.Clear();
			DistanceNeuronGenes.Clear();

			for (int i = 0; i < eyeNeurons; i++)
				EyeRNeuronGenes.Add(GetRandomNeuronGene());
			for (int i = 0; i < eyeNeurons; i++)
				EyeGNeuronGenes.Add(GetRandomNeuronGene());
			for (int i = 0; i < eyeNeurons; i++)
				EyeBNeuronGenes.Add(GetRandomNeuronGene());
			for (int i = 0; i < eyeNeurons; i++)
				DistanceNeuronGenes.Add(GetRandomNeuronGene());

			HiddenNeuronGenes.Clear();
			InputNeuronGenes.Clear();
			OutputNeuronGenes.Clear();

			for (int n = 0; n < hidden; n++)
				HiddenNeuronGenes.Add(GetRandomNeuronGene());
			for (int n = 0; n < inputs; n++)
				InputNeuronGenes.Add(GetRandomNeuronGene());
			for (int n = 0; n < outputs; n++)
				OutputNeuronGenes.Add(GetRandomOutputNeuronGene());

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

		public void AllNeuronsConnectRandomly()
		{
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
			var c2 = (RMP_Chromosome)netC2;
			var child = new RMP_Chromosome();

			crossoverFunction.Crossover(MutationGenes, c2.MutationGenes, child.MutationGenes);

			crossoverFunction.Crossover(EyeRNeuronGenes, c2.EyeRNeuronGenes, child.EyeRNeuronGenes);
			crossoverFunction.Crossover(EyeGNeuronGenes, c2.EyeGNeuronGenes, child.EyeGNeuronGenes);
			crossoverFunction.Crossover(EyeBNeuronGenes, c2.EyeBNeuronGenes, child.EyeBNeuronGenes);
			crossoverFunction.Crossover(DistanceNeuronGenes, c2.DistanceNeuronGenes, child.DistanceNeuronGenes);

			crossoverFunction.Crossover(HiddenNeuronGenes, c2.HiddenNeuronGenes, child.HiddenNeuronGenes);
			crossoverFunction.Crossover(InputNeuronGenes, c2.InputNeuronGenes, child.InputNeuronGenes);
			crossoverFunction.Crossover(OutputNeuronGenes, c2.OutputNeuronGenes, child.OutputNeuronGenes);

			child.GlobalSigmoidFunction = (SigmoidFunction)GlobalSigmoidFunction.CrossoverModule(c2.GlobalSigmoidFunction, crossoverFunction);
			child.GlobalOutputSigmoidFunction = (SigmoidFunction)GlobalOutputSigmoidFunction.CrossoverModule(c2.GlobalOutputSigmoidFunction, crossoverFunction);

			return child;
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

			if (NeuronsCanDie)
				Mutate_RemoveNeurons();
			if (NewNeuronsCanForm)
				Mutate_AddNeurons();

			//Mutate_RemoveEyeNeurons();
			//Mutate_AddEyeNeurons();

			if (ConnectionsCanDie)
				Mutate_RemoveConnections();
			if (NewConnectionsCanForm)
				Mutate_AddConnections();
		}

		public void Mutate_RemoveNeurons()
		{
			var neuronMap = new Dictionary<int, RMP_NeuronGene>();
			//Remove Neurons
			if (NeuronRemoveChance.Value > Pseudo.Random.NextDouble())
			{
				int count = 0;
				foreach (var neuron in HiddenNeuronGenes)
				{
					neuron.Name += "-"+count.ToString();
					neuronMap.Add(count, neuron);
					count++;
				}

				int removeCount = 1;
				
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
				HiddenNeuronGenes.AddRange(from key in neuronMap.Keys orderby Convert.ToInt32(key) select neuronMap[key]);

				foreach (RMP_NeuronGene neuronGene in neuronMap.Values.Concat(this.Inputs))
				{
					foreach (var connection in new List<RMP_ConnectionGene>(neuronGene.Connections))
					{
						if (!connection.IsOutputConnection)
						{
							if (neuronMap.ContainsKey(connection.Target))
							{
								var target = neuronMap[connection.Target];
								connection.Target = HiddenNeuronGenes.IndexOf(target);
							}
							else
							{
								neuronGene.Connections.Remove(connection);
							}
						}
					}
				}
			}
		}
		public void Mutate_AddNeurons()
		{
			//Add Neurons
			if (NeuronAddChance.Value > Pseudo.Random.NextDouble())
			{
				int neuronCount = 1;

				for (int n = 0; n < neuronCount; n++)
				{
					var neuronGene = GetRandomNeuronGene();
					ConnectRandomly(neuronGene, false);
					HiddenNeuronGenes.Add(neuronGene);
				}
			}
		}

		public void Mutate_RemoveConnections()
		{
			foreach (var neuronGene in InputNeuronGenes.Concat(HiddenNeuronGenes))
			{
				if (neuronGene.Connections.Count != 0 && ConnectionRemoveChance.Value > Pseudo.Random.NextDouble())
				{
					neuronGene.Connections.RemoveAt(Pseudo.Random.Next(neuronGene.Connections.Count));
				}
			}
		}
		public void Mutate_AddConnections()
		{
			int count = 0;
			bool isInputNeuron = true;
			foreach (var neuronGene in InputNeuronGenes.Concat(HiddenNeuronGenes))
			{
				if (count == Inputs.Count)
					isInputNeuron = false;

				if (ConnectionAddChance.Value > Pseudo.Random.NextDouble())
					neuronGene.Connections.Add(GetRandomConnection(neuronGene, isInputNeuron));
			}
		}

		public RMP_NeuronGene GetRandomNeuronGene()
		{
			var gene = new RMP_NeuronGene(GlobalSigmoidFunction);
			gene.Bias.Value =
				Math.Pow(Pseudo.Random.NextDouble(gene.Bias.Min, gene.Bias.Max), 2);
			gene.SigmoidFunction.Randomize();

			return gene;
		}
		public RMP_NeuronGene GetRandomOutputNeuronGene()
		{
			var gene = new RMP_NeuronGene(GlobalOutputSigmoidFunction);
			gene.Bias.Value = Math.Pow(Pseudo.Random.NextDouble(gene.Bias.Min, gene.Bias.Max), 2);
			gene.SigmoidFunction.Randomize();

			return gene;
		}
		public void ConnectRandomly(RMP_NeuronGene gene, bool isInputNeuron)
		{
			int connectionCount;

			if (isInputNeuron)
				connectionCount = HiddenNeuronGenes.Count;
			else
				connectionCount = HiddenNeuronGenes.Count + OutputNeuronGenes.Count;

			gene.Connections.Clear();

			for (int i = 0; i < connectionCount; i++)
			{
				if (ConnectionChance < Pseudo.Random.NextDouble())
					continue;

				gene.Connections.Add(GetRandomConnection(gene, isInputNeuron));
			}
		}
		public RMP_ConnectionGene GetRandomConnection(int target, bool isOutputConnection)
		{
			var gene = new RMP_ConnectionGene();

			double weight;
			if (InhibitoryConnectionChance > Pseudo.Random.NextDouble())
				weight = Pseudo.Random.NextDouble(gene.Weight.Min, 0);
			else
				weight = Pseudo.Random.NextDouble(0, gene.Weight.Max);

			gene.Weight.Value = weight;

			gene.Target = target;
			gene.IsOutputConnection = isOutputConnection;

			return gene;
		}
		public RMP_ConnectionGene GetRandomConnection(RMP_NeuronGene host, bool hostIsInputNeuron)
		{
			int connectionCount;

			if (hostIsInputNeuron)
				connectionCount = HiddenNeuronGenes.Count;
			else
				connectionCount = HiddenNeuronGenes.Count + OutputNeuronGenes.Count;

			int targetIndex;
			bool unique;

			do
			{
				targetIndex = Pseudo.Random.Next(connectionCount);

				unique = true;
				foreach (RMP_ConnectionGene connectionGene in host.Connections)
					if (connectionGene.Target == targetIndex)
						unique = false;
			} while (!unique);

			if (targetIndex < HiddenNeuronGenes.Count)
				return GetRandomConnection(targetIndex, false);
			else
				return GetRandomConnection(targetIndex - HiddenNeuronGenes.Count, true);
		}

		public override INeuralNetChromosome Clone()
		{
			var clone = new RMP_Chromosome();

			clone.InhibitoryConnectionChance = InhibitoryConnectionChance;
			clone.ConnectionChance = ConnectionChance;

			clone.NewConnectionsCanForm = NewConnectionsCanForm;
			clone.ConnectionsCanDie = ConnectionsCanDie;
			clone.NewNeuronsCanForm = NewNeuronsCanForm;
			clone.NeuronsCanDie = NeuronsCanDie;

			clone.EyeRNeuronGenes.AddRange(EyeRNeuronGenes.Select(n => (RMP_NeuronGene)n.Clone()));
			clone.EyeGNeuronGenes.AddRange(EyeGNeuronGenes.Select(n => (RMP_NeuronGene)n.Clone()));
			clone.EyeBNeuronGenes.AddRange(EyeBNeuronGenes.Select(n => (RMP_NeuronGene)n.Clone()));
			clone.DistanceNeuronGenes.AddRange(DistanceNeuronGenes.Select(n => (RMP_NeuronGene)n.Clone()));

			clone.HiddenNeuronGenes.AddRange(HiddenNeuronGenes.Select(n => (RMP_NeuronGene)n.Clone()));
			clone.InputNeuronGenes.AddRange(InputNeuronGenes.Select(n => (RMP_NeuronGene)n.Clone()));
			clone.OutputNeuronGenes.AddRange(OutputNeuronGenes.Select(n => (RMP_NeuronGene)n.Clone()));

			clone.MutationGenes.AddRange(MutationGenes.Select(n => (DoubleGene)n.Clone()));

			clone.GlobalSigmoidFunction = (SigmoidFunction)GlobalSigmoidFunction.Clone();
			clone.GlobalOutputSigmoidFunction = (SigmoidFunction)GlobalOutputSigmoidFunction.Clone();

			return clone;
		}

		public override GeneInfo GetGeneInfo()
		{
			var geneInfo = new GeneInfo();

			geneInfo.AddGeneList("EyeRNeuron", EyeRNeuronGenes);
			geneInfo.AddGeneList("EyeGNeuron", EyeGNeuronGenes);
			geneInfo.AddGeneList("EyeBNeuron", EyeBNeuronGenes);
			geneInfo.AddGeneList("DistanceNeuron", DistanceNeuronGenes);

			geneInfo.AddGeneList("HiddenNeuron", HiddenNeuronGenes);
			geneInfo.AddGeneList("InputNeuron", InputNeuronGenes);
			geneInfo.AddGeneList("OutputNeuron", OutputNeuronGenes);

			geneInfo.AddGeneList("Mutation", MutationGenes);

			return geneInfo;
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(InhibitoryConnectionChance);
			w.Write(ConnectionChance);

			w.Write(NewConnectionsCanForm);
			w.Write(ConnectionsCanDie);
			w.Write(NewNeuronsCanForm);
			w.Write(NeuronsCanDie);

			WriteListInfo(EyeRNeuronGenes, w);
			WriteListInfo(EyeGNeuronGenes, w);
			WriteListInfo(EyeBNeuronGenes, w);
			WriteListInfo(DistanceNeuronGenes, w);

			WriteListInfo(HiddenNeuronGenes, w);
			WriteListInfo(InputNeuronGenes, w);
			WriteListInfo(OutputNeuronGenes, w);

			WriteListInfo(MutationGenes, w);

			GlobalSigmoidFunction.Save(w);
			GlobalOutputSigmoidFunction.Save(w);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			InhibitoryConnectionChance = r.ReadDouble();
			ConnectionChance = r.ReadDouble();

			NewConnectionsCanForm = r.ReadBoolean();
			ConnectionsCanDie = r.ReadBoolean();
			NewNeuronsCanForm = r.ReadBoolean();
			NeuronsCanDie = r.ReadBoolean();

			LoadListInfo(EyeRNeuronGenes, r);
			LoadListInfo(EyeGNeuronGenes, r);
			LoadListInfo(EyeBNeuronGenes, r);
			LoadListInfo(DistanceNeuronGenes, r);

			LoadListInfo(HiddenNeuronGenes, r);
			LoadListInfo(InputNeuronGenes, r);
			LoadListInfo(OutputNeuronGenes, r);

			LoadListInfo(MutationGenes, r);

			GlobalSigmoidFunction = BinarySerializable.GetObject<SigmoidFunction>(r);
			GlobalOutputSigmoidFunction = BinarySerializable.GetObject<SigmoidFunction>(r);
		}
	}

	[DebuggerDisplay("ID={ID}, Bias={Bias.Value}, ExcitationDecayRate={ExcitationDecayRate.Value}")]
	public class RMP_NeuronGene : Gene
	{
		public DoubleMinMax Bias;
		public List<RMP_ConnectionGene> Connections = new List<RMP_ConnectionGene>();

		public SigmoidFunction SigmoidFunction;

		public RMP_NeuronGene()
			: base("RMP_NeuronGene")
		{
			initialize();
		}
		public RMP_NeuronGene(SigmoidFunction sigmoidFunction)
			: base("RMP_NeuronGene")
		{
			initialize();
			SigmoidFunction = (SigmoidFunction)sigmoidFunction.Clone();
		}
		public RMP_NeuronGene(string geneHeritage, SigmoidFunction sigmoidFunction)
			: base("RMP_NeuronGene", geneHeritage)
		{
			initialize();
			SigmoidFunction = (SigmoidFunction)sigmoidFunction.Clone();
		}

		void initialize()
		{
			Bias = new DoubleMinMax(0, 1);
		}

		public override void Mutate(double mutationChance)
		{
			MutateValue(Bias, mutationChance);

			foreach (var connectionGene in Connections)
			{
				connectionGene.Mutate(mutationChance);
			}

			SigmoidFunction.TryToMutate(mutationChance);
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
			var gene = new RMP_NeuronGene(GeneHeritage, (SigmoidFunction)SigmoidFunction.Clone());

			gene.IsMutable = IsMutable;
			gene.Bias = Bias;

			foreach (var connection in Connections)
				gene.Connections.Add((RMP_ConnectionGene)connection.Clone());

			return gene;
		}

		public override GeneInfo GetGeneInfo()
		{
			var geneInfo = new GeneInfo();

			geneInfo.Doubles.Add(Bias.Value);
			geneInfo.AddGeneList("Connections", Connections);
			geneInfo.AddMiscGeneInfo(SigmoidFunction.GetGeneInfo());

			return geneInfo;
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			Bias.WriteTo(w);
			WriteListInfo(Connections, w);

			SigmoidFunction.Save(w);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			Bias = DoubleMinMax.Read(r);
			LoadListInfo(Connections, r);

			SigmoidFunction = BinarySerializable.GetObject<SigmoidFunction>(r);
		}

		public void Randomize()
		{
			Bias.Value = Pseudo.Random.NextDouble(Bias.Min, Bias.Max);
			SigmoidFunction.Randomize();

			foreach (var c in Connections)
				c.Randomize();
		}
	}

	[DebuggerDisplay("ID={ID}, Weight={Weight.Value}, Delay={Delay.Value}")]
	public class RMP_ConnectionGene : Gene
	{
		public int Target;
		public bool IsOutputConnection;

		public DoubleMinMax Weight;

		public RMP_ConnectionGene()
			: base("RMP_ConnectionGene")
		{
			initialize();
		}
		public RMP_ConnectionGene(string geneHeritage)
			: base("RMP_ConnectionGene", geneHeritage)
		{
			initialize();
		}

		void initialize()
		{
			Weight = new DoubleMinMax(-10, 10);
		}

		public override Gene Clone()
		{
			var gene = new RMP_ConnectionGene(GeneHeritage);

			gene.Target = Target;
			gene.IsOutputConnection = IsOutputConnection;
			gene.Weight = Weight;

			return gene;
		}

		public override void Mutate(double mutationChance)
		{
			if (mutationChance < Pseudo.Random.NextDouble())
				return;

			double step = Weight.Max - Weight.Min;

			Weight.Value += Pseudo.GaussianRandom.NextDouble(-step, step);
		}

		public override GeneInfo GetGeneInfo()
		{
			var geneInfo = new GeneInfo();

			geneInfo.Doubles.Add(Weight.Value);

			return geneInfo;
		}

		public void Randomize()
		{
			Weight.Value = Pseudo.Random.NextDouble(Weight.Min, Weight.Max);
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(Target);
			w.Write(IsOutputConnection);

			Weight.WriteTo(w);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			Target = r.ReadInt32();
			IsOutputConnection = r.ReadBoolean();

			Weight = DoubleMinMax.Read(r);
		}
	}
}
