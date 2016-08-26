using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using TakaGUI.IO;
using EvoSim;
using EvoApp.EntityPainters;
using TakaGUI;

namespace EvoApp
{
	public class Simulation : BinarySerializable
	{
		/// <summary>
		/// The elementName of the folder that the simulation is stored in.
		/// </summary>
		public string FileName;

		public IWorld World;

		public List<IEntityPainter> EntityPainters = new List<IEntityPainter>();

		public Simulation()
			: base("Simulation")
		{
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(FileName);
			World.Save(w);

			WriteListInfo(EntityPainters, w);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			FileName = r.ReadString();

			World = BinarySerializable.GetObject<IWorld>(r);

			LoadListInfo(EntityPainters, r);

			foreach (var p in EntityPainters)
				p.Initialize(DrawBox.DefaultSkinFile);
		}

		public void Save(string fileName)
		{
			string oldName = FileName;
			FileName = fileName;

			using (FileStream fs = new FileStream(fileName + ".evo", FileMode.Create))
			{
				using (BinaryWriter w = new BinaryWriter(fs))
				{
					this.Save(w);
				}
			}

			using (FileStream fs = new FileStream(fileName + ".evo.str", FileMode.Create))
			{
				using (BinaryWriter w = new BinaryWriter(fs))
				{
					BinarySerializable.WriteSaveInfo(w);
				}
			}

			FileName = oldName;

			BinarySerializable.ClearSaveBuffer();
		}
		public void Save()
		{
			Save(FileName);
		}

		public static Simulation Load(string fileName)
		{
			Simulation simulation = null;

			if (Path.GetExtension(fileName) == ".evo")
				fileName = Path.GetFileNameWithoutExtension(fileName);

			using (FileStream fs = new FileStream(fileName + ".evo.str", FileMode.Open))
			{
				BinaryReader r = new BinaryReader(fs);
				BinarySerializable.LoadIntoBuffer(r);
			}

			using (FileStream fs = new FileStream(fileName + ".evo", FileMode.Open))
			{
				BinaryReader r = new BinaryReader(fs);
				simulation = BinarySerializable.GetObject<Simulation>(r);
			}

			BinarySerializable.ClearLoadBuffer();

			simulation.World.ReloadRegions();

			return simulation;
		}
	}
}
