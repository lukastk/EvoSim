using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using EvoSim.RandomNumberGenerators;
using System.Diagnostics;
using EvoSim.Genes;
using System.IO;

namespace EvoSim
{
	public interface ICreature : IEntity
	{
		INeuralNet Brain { get; set; }
		Genome CreatureGenome { get; set; }

		int EyeNeuronsAmount { get; set; }
		double EyeSpan { get; set; }
		int ViewDistance { get; set; }
		DoubleMinMax Energy { get; set; }

		IEntity GetNewInstance();
		void ImprintGenome(Genome genome);
	}
}
