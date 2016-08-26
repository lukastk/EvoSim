using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace EvoSim.RandomNumberGenerators
{
    public class UniformRandomGenerator : BinarySerializable, IRandomGenerator
    {
        // Yes, it'i ironic we have to seed our random generator with a random number
        private MersenneTwister _rGen = new MersenneTwister((uint)new Random().Next());

		public UniformRandomGenerator()
			: base("UniformRandomGenerator")
		{
			_rGen = new MersenneTwister((uint)new Random().Next());
		}
		public UniformRandomGenerator(uint seed)
			: base("UniformRandomGenerator")
		{
			_rGen = new MersenneTwister(seed);
		}

        public int Next()
        {
            return _rGen.Next();
        }

        public double NextDouble()
        {
            return _rGen.NextDouble();
        }

		public double NextDouble(double min, double max)
		{
			return min + (max - min) * _rGen.NextDouble();
		}

        public int Next(int max)
        {
            return _rGen.Next(max);
        }

        public int Next(int min, int max)
        {
            return _rGen.Next(min, max);
        }

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			_rGen.Save(w);
		}
		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			_rGen = BinarySerializable.GetObject<MersenneTwister>(r);
		}
    }
}
