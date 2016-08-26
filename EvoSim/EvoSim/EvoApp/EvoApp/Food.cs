using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using EvoApp;
using TakaGUI.DrawBoxes;
using TakaGUI;
using EvoApp.Forms;

namespace EvoSim
{
	public class Food : Entity
	{
		public double EnergyStorage = 60 * 60;

		public Food()
			: base("Food")
		{
			Radius = 8;
			EntityColor = Color.Green;
		}

		public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
		{
			base.Update(gameTime);

			if (EnergyStorage <= 0)
				Kill();
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(EnergyStorage);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			EnergyStorage = r.ReadDouble();
		}

		public override IEntity GetNewInstance()
		{
			return new Food();
		}
		protected override void CloneValues(IEntity entity)
		{
			base.CloneValues(entity);

			var clone = (Food)entity;

			clone.EnergyStorage = EnergyStorage;
		}

		protected override void GUI_Edit_SetColumnListBox(ColumnListBox listBox)
		{
			base.GUI_Edit_SetColumnListBox(listBox);

			listBox.AddRow("EnergyStorage", EnergyStorage);
		}
		protected override void GUI_Edit_SetValues(ColumnListBox listBox)
		{
			base.GUI_Edit_SetValues(listBox);

			Dictionary<string, object> values = new Dictionary<string, object>();
			foreach (var row in listBox.Values)
				values.Add((string)row.Values[0], row.Values[1]);

			EnergyStorage = (double)values["EnergyStorage"];
		}
	}
}
