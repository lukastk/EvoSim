using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoSim.Genes;
using System.IO;

namespace EvoSim
{
	public abstract class SigmoidFunction : GeneModule
	{
		public abstract double Sigmoid(double a);
	}
}
