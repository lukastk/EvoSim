using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoSim.Genes
{
	public interface IGeneInfoHolder
	{
		GeneInfo GetGeneInfo();
	}

	public class GeneInfo : Dictionary<string, List<GeneInfo>>
	{
		public List<double> Doubles = new List<double>();
		public readonly string Name;

		public GeneInfo()
		{
			this.Add("Misc", new List<GeneInfo>());
		}

		public void AddGeneList<T>(string key, List<T> geneList)
			where T : IGeneInfoHolder
		{
			var geneDictList = new List<GeneInfo>();

			foreach (var gene in geneList)
				geneDictList.Add(gene.GetGeneInfo());

			this.Add(key, geneDictList);
		}

		public void AddMiscGeneInfo(GeneInfo geneInfo)
		{
			this["Misc"].Add(geneInfo);
		}
	}
}
