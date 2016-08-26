using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoSim.Genes;

namespace EvoSim
{
	public abstract class CrossoverFunction : GeneModule
	{
		public abstract void Crossover<T>(List<T> list1, List<T> list2, List<T> putIn)
			where T : ICloneable;
	}
}
