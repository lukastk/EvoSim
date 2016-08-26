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
	[DebuggerDisplay("Name={Name}, ID={ID}, Value={Value}")]
	public class DoubleGene : Gene
	{
		double _Value;
		public double Value
		{
			get { return _Value; }
			set
			{
				_Value = value;

				if (Min != -1 && _Value < Min)
					_Value = Min;

				if (Max != -1 && _Value > Max)
					_Value = Max;
			}
		}

		double _Min;
		public double Min
		{
			get { return _Min; }
			set
			{
				_Min = value;

				if (_Min == -1)
					return;

				if (_Min > _Max)
					_Min = _Max;

				Value = Value;
			}
		}

		double _Max;
		public double Max
		{
			get { return _Max; }
			set
			{
				_Max = value;

				if (_Max == -1)
					return;

				if (_Max < _Min)
					_Max = _Min;

				Value = Value;
			}
		}

		public DoubleGene()
			: base("")
		{
		}
		public DoubleGene(string name, double min, double max)
			: base(name)
		{
			Min = min;
			Max = max;
		}
		public DoubleGene(string name, string geneHeritage, double min, double max)
			: base(name, geneHeritage)
		{
			Min = min;
			Max = max;
		}
		public DoubleGene(string name, double min, double max, double value)
			: base(name)
		{
			Min = min;
			Max = max;
			Value = value;
		}

		public void SetMinMaxValue(double min, double max, double value)
		{
			_Min = min;
			_Max = max;
			_Value = value;

			Min = Min;
			Max = Max;
			Value = Value;
		}

		public override void Mutate(double mutationChance)
		{
			if (!IsMutable)
				return;

			if (mutationChance < Pseudo.Random.NextDouble())
				return;

			double step = Max - Min;

			Value += Pseudo.GaussianRandom.NextDouble(-step, step);
		}

		public override Gene Clone()
		{
			var gene = new DoubleGene(Name, GeneHeritage, Min, Max);
			gene.Value = Value;

			gene.IsMutable = IsMutable;

			return gene;
		}

		public void Randomize()
		{
			if (!IsMutable)
				return;

			Value = Pseudo.Random.NextDouble(Min, Max);
		}

		public override GeneInfo GetGeneInfo()
		{
			var geneInfo = new GeneInfo();

			geneInfo.Doubles.Add(Value);

			return geneInfo;
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(Value);
			w.Write(Min);
			w.Write(Max);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			_Value = r.ReadDouble();
			_Min = r.ReadDouble();
			_Max = r.ReadDouble();

			Value = Value;
			Min = Min;
			Max = Max;
		}
	}
}
