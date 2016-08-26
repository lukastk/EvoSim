using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoSim.Genes;
using EvoSim;
using System.IO;
using TakaGUI.DrawBoxes.Forms;
using EvoApp.EntityPainters;

namespace EvoApp.Menu
{
	public static class EditorData
	{
		public static List<WorldTemplate> WorldTemplates = new List<WorldTemplate>();
		public static List<EntityTemplate> EntityTemplates = new List<EntityTemplate>();
		public static List<CreatureTemplate> CreatureTemplates = new List<CreatureTemplate>();
		public static List<GenomeTemplate> GenomeTemplates = new List<GenomeTemplate>();
		public static List<ANNTemplate> ANNTemplates = new List<ANNTemplate>();

		public static void Save(string fileName)
		{
			using (FileStream fs = new FileStream(fileName, FileMode.Create))
			{
				using (BinaryWriter w = new BinaryWriter(fs))
				{
					SaveData(w);
				}
			}

			using (FileStream fs = new FileStream(fileName + ".str", FileMode.Create))
			{
				using (BinaryWriter w = new BinaryWriter(fs))
				{
					BinarySerializable.WriteSaveInfo(w);
				}
			}

			BinarySerializable.ClearSaveBuffer();
		}
		public static void Load(string fileName)
		{
			using (FileStream fs = new FileStream(fileName + ".str", FileMode.OpenOrCreate))
			{
				BinaryReader r = new BinaryReader(fs);
				BinarySerializable.LoadIntoBuffer(r);
			}

			using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
			{
				BinaryReader r = new BinaryReader(fs);
				LoadData(r);
			}

			BinarySerializable.ClearLoadBuffer();
		}

		static void SaveData(BinaryWriter w)
		{
			BinarySerializable.WriteListInfo(WorldTemplates, w);
			BinarySerializable.WriteListInfo(CreatureTemplates, w);
			BinarySerializable.WriteListInfo(EntityTemplates, w);
			BinarySerializable.WriteListInfo(GenomeTemplates, w);
			BinarySerializable.WriteListInfo(ANNTemplates, w);
		}

		static void LoadData(BinaryReader r)
		{
			if (r.BaseStream.Position == r.BaseStream.Length)
				return;

			BinarySerializable.LoadListInfo(WorldTemplates, r);
			BinarySerializable.LoadListInfo(CreatureTemplates, r);
			BinarySerializable.LoadListInfo(EntityTemplates, r);
			BinarySerializable.LoadListInfo(GenomeTemplates, r);
			BinarySerializable.LoadListInfo(ANNTemplates, r);
		}
	}

	public class WorldTemplate : BinarySerializable
	{
		public string TemplateName;
		public IWorld World;

		public WorldTemplate()
			: base("WorldTemplate")
		{
		}

		protected override void WriteInfo(System.IO.BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(TemplateName);
			World.Save(w);
		}

		public override void Load(System.IO.BinaryReader r, uint id)
		{
			base.Load(r, id);

			TemplateName = r.ReadString();
			World = BinarySerializable.GetObject<IWorld>(r);
		}
	}

	public class EntityTemplate : BinarySerializable
	{
		public string TemplateName;
		public IEntity Entity;
		public IEntityPainter Painter;

		public EntityTemplate()
			: base("EntityTemplate")
		{
		}

		protected override void WriteInfo(System.IO.BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(TemplateName);
			Entity.Save(w);
		}

		public override void Load(System.IO.BinaryReader r, uint id)
		{
			base.Load(r, id);

			TemplateName = r.ReadString();
			Entity = BinarySerializable.GetObject<IEntity>(r);
		}
	}

	public class CreatureTemplate : BinarySerializable
	{
		public string TemplateName;
		public ICreature Creature;
		public IEntityPainter Painter;

		public CreatureTemplate()
			: base("CreatureTemplate")
		{
		}

		protected override void WriteInfo(System.IO.BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(TemplateName);
			Creature.Save(w);
		}

		public override void Load(System.IO.BinaryReader r, uint id)
		{
			base.Load(r, id);

			TemplateName = r.ReadString();
			Creature = BinarySerializable.GetObject<ICreature>(r);
		}
	}

	public class GenomeTemplate : BinarySerializable
	{
		public string TemplateName;
		public Genome Genome;

		public GenomeTemplate()
			: base("GenomeTemplate")
		{
		}

		protected override void WriteInfo(System.IO.BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(TemplateName);
			Genome.Save(w);
		}

		public override void Load(System.IO.BinaryReader r, uint id)
		{
			base.Load(r, id);

			TemplateName = r.ReadString();
			Genome = BinarySerializable.GetObject<Genome>(r);
		}
	}

	public class ANNTemplate : BinarySerializable
	{
		public string TemplateName;
		public INeuralNetChromosome ANN;

		public ANNTemplate()
			: base("ANNTemplate")
		{
		}

		protected override void WriteInfo(System.IO.BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(TemplateName);
			ANN.Save(w);
		}

		public override void Load(System.IO.BinaryReader r, uint id)
		{
			base.Load(r, id);

			TemplateName = r.ReadString();
			ANN = BinarySerializable.GetObject<INeuralNetChromosome>(r);
		}
	}
}
