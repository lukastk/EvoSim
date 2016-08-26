using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using System.Xml;
using EvoSim;
using TakaGUI;
using TakaGUI.DrawBoxes;
using EvoApp.Forms;

namespace EvoApp
{
	public class Entity : BinarySerializable, IEntity
	{
		public string EntityName { get; private set; }

		public IWorld World { get; set; }

		Point _region;
		public Point Region
		{
			get { return _region; }
			set
			{
				World.Regions[Region.X, Region.Y].Remove(this);
				World.Regions[value.X, value.Y].Add(this);

				_region = value;
			}
		}

		double rotation;
		public double Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;

				if (rotation < 0)
					rotation = Math.PI * 2 + rotation;
				else if (rotation > Math.PI * 2)
					rotation = rotation - Math.PI * 2;
			}
		}

		EntityPosition _position;
		public EntityPosition Position
		{
			get { return _position; }
			set
			{
				_position = value;

				if (World != null)
				{
					if (World.WorldHasEdges)
					{
						if (_position.X < 0)
							_position.X = 0;
						else if (_position.X >= World.Width)
							_position.X = World.Width - 1;

						if (_position.Y < 0)
							_position.Y = 0;
						else if (_position.Y >= World.Height)
							_position.Y = World.Height - 1;
					}
					else
					{
						if (_position.X < 0)
							_position.X = World.Width + _position.X;
						else if (_position.X > World.Width)
							_position.X = _position.X - World.Width;

						if (_position.Y < 0)
							_position.Y = (World.Height - 1) + _position.Y;
						else if (_position.Y >= World.Height)
							_position.Y = _position.Y - World.Height;
					}

					var region = GetRegionPoint(Position.X, Position.Y);

					if (region.X != Region.X || region.Y != Region.Y)
						Region = region;
				}
			}
		}
		public void Move(double dx, double dy)
		{
			var newPos = Position;
			newPos.X += dx;
			newPos.Y += dy;

			Position = newPos;
		}

		public void CheckRegion()
		{
			DeleteFromRegion();

			_region.X = (int)(_position.X / World.RegionSize);
			_region.Y = (int)(_position.Y / World.RegionSize);

			World.Regions[Region.X, Region.Y].Add(this);
		}
		public void DeleteFromRegion()
		{
			for (int x = 0; x < World.RegionLengthX; x++)
			{
				for (int y = 0; y < World.RegionLengthY; y++)
				{
					if (World.Regions[x, y].Contains(this))
						World.Regions[x, y].Remove(this);
				}
			}
		}

		public Point GetRegionPoint(double x, double y)
		{
			var regionPoint = new Point();
			regionPoint.X = (int)(_position.X / World.RegionSize);
			regionPoint.Y = (int)(_position.Y / World.RegionSize);

			return regionPoint;
		}

		public Color EntityColor { get; set; }
		public virtual int Radius { get; set; }
		public int Age { get; set; }

		public bool IsDead { get; private set; }

		public Entity()
			: base("Entity")
		{
		}
		public Entity(string entityName)
			: base("Entity")
		{
			EntityName = entityName;
		}

		public virtual void Update(GameTime gameTime)
		{
			Age += 1;
		}

		public void Kill()
		{
			IsDead = true;
		}

		public virtual IEntity GetNewInstance()
		{
			return new Entity();
		}
		public IEntity Clone()
		{
			var clone = GetNewInstance();

			CloneValues(clone);

			return clone;
		}
		protected virtual void CloneValues(IEntity entity)
		{
			var clone = (Entity)entity;

			clone.EntityName = EntityName;

			clone.World = World;
			clone._region = Region;

			clone.Rotation = Rotation;
			clone._position = Position;

			clone.EntityColor = EntityColor;
			clone.Radius = Radius;
			clone.Age = Age;

			clone.IsDead = IsDead;
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(GetIntFromString(EntityName));

			WriteNullableObject(World, w);

			w.Write(Rotation);
			w.Write(Position.X);
			w.Write(Position.Y);
			w.Write(EntityColor.R);
			w.Write(EntityColor.G);
			w.Write(EntityColor.B);
			w.Write(EntityColor.A);
			w.Write(Radius);
			w.Write(Age);

			w.Write(IsDead);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			EntityName = GetStringFromInt(r.ReadUInt32());

			World = ReadNullableObject<IWorld>(r);

			Rotation = r.ReadDouble();
			var pos = new EntityPosition();
			pos.X = r.ReadDouble();
			pos.Y = r.ReadDouble();
			_position = pos;
			Color color = new Color();
			color.R = r.ReadByte();
			color.G = r.ReadByte();
			color.B = r.ReadByte();
			color.A = r.ReadByte();
			EntityColor = color;
			Radius = r.ReadInt32();
			Age = r.ReadInt32();

			IsDead = r.ReadBoolean();
		}

		public virtual Func<bool> GUI_Edit(SingleSlotBox container)
		{
			GUI_Edit_AddColumnListBox(container);

			return GUI_Edit_GetValidFunction();
		}
		protected virtual Func<bool> GUI_Edit_GetValidFunction()
		{
			return delegate()
			{
				return true;
			};
		}
		protected virtual ColumnListBox GUI_Edit_AddColumnListBox(SingleSlotBox container)
		{
			var columnListBox = new ColumnListBox();
			columnListBox.Initialize(2);
			container.AddDrawBox(columnListBox);
			columnListBox.SetIntOrStringSort(false, false);
			columnListBox.SetColumnName(0, "Name");
			columnListBox.SetColumnName(1, "Value");
			columnListBox.Width = 200;
			columnListBox.Height = 200;

			GUI_Edit_SetColumnListBox(columnListBox);

			columnListBox.ItemDoubleClicked += delegate(object sender, ColumnListBox.ListBoxRow item, int index)
			{
				var valType = columnListBox.SelectedRow.Values[1].GetType();

				if (valType.IsEquivalentTo(typeof(int)))
				{
					EditIntForm.ShowDialogue(container.Parent, (int)columnListBox.SelectedRow.Values[1], delegate(object _sender)
					{
						columnListBox.SelectedRow.Values[1] = ((EditIntForm)_sender).Result;
					});
				}
				else if (valType.IsEquivalentTo(typeof(byte)))
				{
					EditByteForm.ShowDialogue(container.Parent, (byte)columnListBox.SelectedRow.Values[1], delegate(object _sender)
					{
						columnListBox.SelectedRow.Values[1] = ((EditByteForm)_sender).Result;
					});
				}
				else if (valType.IsEquivalentTo(typeof(double)))
				{
					EditDoubleForm.ShowDialogue(container.Parent, (double)columnListBox.SelectedRow.Values[1], delegate(object _sender)
					{
						columnListBox.SelectedRow.Values[1] = ((EditDoubleForm)_sender).Result;
					});
				}
				else if (valType.IsEquivalentTo(typeof(bool)))
				{
					EditBoolForm.ShowDialogue(container.Parent, (bool)columnListBox.SelectedRow.Values[1], delegate(object _sender)
					{
						columnListBox.SelectedRow.Values[1] = ((EditBoolForm)_sender).Result;
					});
				}
			};

			var editFieldButton = new ResizableButton();
			editFieldButton.Initialize();
			container.AddDrawBox(editFieldButton);
			editFieldButton.Title = "Edit Field";
			editFieldButton.FitToText();
			Push.ToTheBottomSideOf(editFieldButton, columnListBox, 3, Push.VerticalAlign.Left);
			editFieldButton.Width = 200;
			editFieldButton.Click += delegate(object sender)
			{
				var valType = columnListBox.SelectedRow.Values[1].GetType();

				if (valType.IsEquivalentTo(typeof(int)))
				{
					EditIntForm.ShowDialogue(container.Parent, (int)columnListBox.SelectedRow.Values[1], delegate(object _sender)
					{
						columnListBox.SelectedRow.Values[1] = ((EditIntForm)_sender).Result;
					});
				}
				else if (valType.IsEquivalentTo(typeof(byte)))
				{
					EditByteForm.ShowDialogue(container.Parent, (byte)columnListBox.SelectedRow.Values[1], delegate(object _sender)
					{
						columnListBox.SelectedRow.Values[1] = ((EditByteForm)_sender).Result;
					});
				}
				else if (valType.IsEquivalentTo(typeof(double)))
				{
					EditDoubleForm.ShowDialogue(container.Parent, (double)columnListBox.SelectedRow.Values[1], delegate(object _sender)
					{
						columnListBox.SelectedRow.Values[1] = ((EditDoubleForm)_sender).Result;
					});
				}
				else if (valType.IsEquivalentTo(typeof(bool)))
				{
					EditBoolForm.ShowDialogue(container.Parent, (bool)columnListBox.SelectedRow.Values[1], delegate(object _sender)
					{
						columnListBox.SelectedRow.Values[1] = ((EditBoolForm)_sender).Result;
					});
				}
			};

			container.Wrap();

			columnListBox.Alignment = DrawBoxAlignment.GetFull();
			editFieldButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();

			container.IsClosing += delegate(object sender)
			{
				GUI_Edit_SetValues(columnListBox);
			};

			return columnListBox;
		}
		protected virtual void GUI_Edit_SetColumnListBox(ColumnListBox listBox)
		{
			listBox.AddRow("PositionX", Position.X);
			listBox.AddRow("PositionY", Position.Y);
			listBox.AddRow("Rotation", Rotation);
			listBox.AddRow("EntityColorR", EntityColor.R);
			listBox.AddRow("EntityColorG", EntityColor.G);
			listBox.AddRow("EntityColorB", EntityColor.B);
			listBox.AddRow("Radius", Radius);
			listBox.AddRow("Age", Age);
		}
		protected virtual void GUI_Edit_SetValues(ColumnListBox listBox)
		{
			Dictionary<string, object> values = new Dictionary<string, object>();
			foreach (var row in listBox.Values)
				values.Add((string)row.Values[0], row.Values[1]);

			Position = new EntityPosition((double)values["PositionX"], (double)values["PositionY"]);
			Rotation = (double)values["Rotation"];
			var entityColor = new Color();
			entityColor.R = (byte)values["EntityColorR"];
			entityColor.G = (byte)values["EntityColorG"];
			entityColor.B = (byte)values["EntityColorB"];
			EntityColor = entityColor;
			Radius = (int)values["Radius"];
			Age = (int)values["Age"];
		}
	}
}
