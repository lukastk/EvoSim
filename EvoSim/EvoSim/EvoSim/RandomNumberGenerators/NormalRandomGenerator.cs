using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace EvoSim.RandomNumberGenerators
{
    public class NormalRandomGenerator : BinarySerializable, IRandomGenerator
    {
        public int Min { get; set; }
		public int Max { get; set; }
        public double StandardDeviation { get; set; }
        public double Mean { get; set; }
        private double _maxToGenerateForProbability;
		private UniformRandomGenerator _rGen;
        
        // Key is x, value is p(x)
		private Dictionary<int, double> probabilities = new Dictionary<int, double>();

		public NormalRandomGenerator()
			: base("NormalRandomGenerator")
		{
		}
		public NormalRandomGenerator(int min, int max)
			: base("NormalRandomGenerator")
		{
			_rGen = new UniformRandomGenerator();
			Load(min, max);
		}
		public NormalRandomGenerator(int min, int max, UniformRandomGenerator rand)
			: base("NormalRandomGenerator")
		{
			_rGen = rand;
			Load(min, max);
		}

		void Load(int min, int max)
		{
			this.Min = min;
			this.Max = max;

			// Assume random normal distribution from [min..max]
			// Calculate mean. For [4 .. 6] the mean is 5.
			this.Mean = ((max - min) / 2) + min;

			// Calculate standard deviation
			int xMinusMyuSquaredSum = 0;
			for (int i = min; i < max; i++)
			{
				xMinusMyuSquaredSum += (int)Math.Pow(i - this.Mean, 2);
			}

			this.StandardDeviation = Math.Sqrt(xMinusMyuSquaredSum / (max - min + 1));
			// Flat, uniform distros tend to have a stdev that'i too high; for example,
			// for 1-10, stdev is 3, meaning the ranges are 68% in 2-8, and 95% in -1 to 11...
			// So we cut this down to create better statistical variation. We now
			// get numbers like: 1dev=68%, 2dev=95%, 3dev=99% (+= 1%). w00t!
			this.StandardDeviation *= (0.5);

			for (int i = min; i < max; i++)
			{
				probabilities[i] = calculatePdf(i);
				if (i > 0)
				{
					// Eg. if we have: 1 (20%), 2 (60%), 3 (20%), we want to see
					// 1 (20), 2 (80), 3 (100)
					probabilities[i] += probabilities[i - 1];
				}
			}

			this._maxToGenerateForProbability = this.probabilities.Values.Max();
		}

        public double calculatePdf(int x)
        {
            // Formula from Wikipedia: http://en.wikipedia.org/wiki/Normal_distribution
            // f(x) = e ^ [-(x-myu)^2 / 2*sigma^2]
            //        -------------------------
            //         root(2 * pi * sigma^2)

            double negativeXMinusMyuSquared = -(x - this.Mean) * (x - this.Mean);
            double variance = StandardDeviation * StandardDeviation;
            double twoSigmaSquared = 2 * variance;
            double twoPiSigmaSquared = Math.PI * twoSigmaSquared;

            double eExponent = negativeXMinusMyuSquared / twoSigmaSquared;
            double top = Math.Pow(Math.E, eExponent);
            double bottom = Math.Sqrt(twoPiSigmaSquared);

            return top / bottom;
        }

        public int Next()
        {
            // map [0..1] to [0 .. maxToGenerateForProbability]
            double pickedProb = this._rGen.NextDouble() * this._maxToGenerateForProbability;
            for (int i = this.Min; i < this.Max; i++)
            {
                if (pickedProb <= this.probabilities[i])
                {
                    return i;
                }
            }

            throw new InvalidOperationException("WTH?");
        }
		public int Next(int min, int max)
		{
			return min + (int)Math.Round((max - min) * NextDouble(), 0);
		}

		public double NextDouble()
		{
			int randomInt = Next();

			if (randomInt != 0)
				return (double)(Next() - Min) / (Max - Min);
			else
				return 1;
		}
		public double NextDouble(double min, double max)
		{
			return min + (max - min) * NextDouble();
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(Min);
			w.Write(Max);
			w.Write(StandardDeviation);
			w.Write(Mean);
			w.Write(_maxToGenerateForProbability);
			_rGen.Save(w);

			w.Write(probabilities.Count);
			foreach (var pair in probabilities)
			{
				w.Write(pair.Key);
				w.Write(pair.Value);
			}
		}
		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			Min = r.ReadInt32();
			Max = r.ReadInt32();
			StandardDeviation = r.ReadDouble();
			Mean = r.ReadDouble();
			_maxToGenerateForProbability = r.ReadDouble();
			_rGen = BinarySerializable.GetObject<UniformRandomGenerator>(r);

			int length = r.ReadInt32();
			for (int i = 0; i < length; i++)
			{
				int key = r.ReadInt32();
				double value = r.ReadDouble();
				probabilities.Add(key, value);
			}
		}
    }
}
