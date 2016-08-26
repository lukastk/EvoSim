using System.Collections.Generic;
using System.Collections.ObjectModel;
using EvoSim.Genes;
using TakaGUI.IO;
using System.IO;
using System;
using TakaGUI;

namespace EvoSim
{
	public interface INeuralNet : IBinarySerializable
	{
		ReadOnlyCollection<IInputNeuron> Inputs { get; }
		ReadOnlyCollection<IOutputNeuron> Outputs { get; }

		void ImprintGenome(INeuralNetChromosome genome);

		void Update();
	}

	public abstract class NeuralNet<TNeuron, TInputNeuron, TOutputNeuron> : BinarySerializable, INeuralNet
		where TNeuron : BinarySerializable
		where TInputNeuron : IInputNeuron
		where TOutputNeuron : IOutputNeuron
	{
		public ReadOnlyCollection<TNeuron> HiddenNeurons { get; private set; }
		public ReadOnlyCollection<TInputNeuron> InputNeurons { get; private set; }
		public ReadOnlyCollection<TOutputNeuron> OutputNeurons { get; private set; }

		public ReadOnlyCollection<IInputNeuron> Inputs { get; private set; }
		public ReadOnlyCollection<IOutputNeuron> Outputs { get; private set; }

		List<TNeuron> hiddenNeurons = new List<TNeuron>();
		List<TInputNeuron> inputNeurons = new List<TInputNeuron>();
		List<TOutputNeuron> outputNeurons = new List<TOutputNeuron>();

		List<IInputNeuron> inputs = new List<IInputNeuron>();
		List<IOutputNeuron> outputs = new List<IOutputNeuron>();

		public NeuralNet()
			: base("NeuralNet")
		{
			HiddenNeurons = hiddenNeurons.AsReadOnly();
			InputNeurons = inputNeurons.AsReadOnly();
			OutputNeurons = outputNeurons.AsReadOnly();

			Inputs = inputs.AsReadOnly();
			Outputs = outputs.AsReadOnly();
		}

		public virtual void AddHiddenNeuron(TNeuron neuron)
		{
			hiddenNeurons.Add(neuron);
		}
		public virtual void AddInputNeuron(TInputNeuron neuron)
		{
			inputNeurons.Add(neuron);
			inputs.Add(neuron);
		}
		public virtual void AddOutputNeuron(TOutputNeuron neuron)
		{
			outputNeurons.Add(neuron);
			outputs.Add(neuron);
		}

		public virtual void RemoveHiddenNeuron(TNeuron neuron)
		{
			hiddenNeurons.Remove(neuron);
		}
		public virtual void RemoveInputNeuron(TInputNeuron neuron)
		{
			inputNeurons.Remove(neuron);
			inputs.Remove(neuron);
		}
		public virtual void RemoveOutputNeuron(TOutputNeuron neuron)
		{
			outputNeurons.Remove(neuron);
			outputs.Remove(neuron);
		}

		public virtual void RemoveAllHiddenNeurons()
		{
			hiddenNeurons.Clear();
		}
		public virtual void RemoveAllInputNeurons()
		{
			inputNeurons.Clear();
			inputs.Clear();
		}
		public virtual void RemoveAllOutputNeurons()
		{
			outputNeurons.Clear();
			outputs.Clear();
		}

		public abstract void ImprintGenome(INeuralNetChromosome genome);

		public abstract void Update();

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			WriteListInfo(hiddenNeurons, w);
			WriteListInfo(inputNeurons, w);
			WriteListInfo(outputNeurons, w);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			LoadListInfo(hiddenNeurons, r);
			LoadListInfo(inputNeurons, r);
			LoadListInfo(outputNeurons, r);
		}

		public virtual Func<bool> GUI_Edit(SingleSlotBox container)
		{
			return null;
		}
	}
	
	public interface IInputNeuron : IBinarySerializable
	{
		double FireFrequency { get; set; }

		CastingList<IConnection> Connections { get; }
	}

	public interface IOutputNeuron : IBinarySerializable
	{
		double Output { get; set; }
	}

	public interface IConnection
	{
		double Weight { get; set; }
	}
}
