using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using System.Xml;

namespace EvoSim
{
	public interface IEntity : IBinarySerializable
	{
		string EntityName { get; }
		IWorld World { get; set; }
		Point Region { get; set; }
		double Rotation { get; set; }
		EntityPosition Position { get; set; }
		void Move(double dx, double dy);

		void CheckRegion();
		void DeleteFromRegion();

		Point GetRegionPoint(double x, double y);

		Color EntityColor { get; set; }
		int Radius { get; set; }
		int Age { get; set; }

		bool IsDead { get; }

		void Update(GameTime gameTime);

		void Kill();

		IEntity Clone();
	}

	public struct EntityPosition
	{
		public double X;
		public double Y;

		public EntityPosition(double x, double y)
		{
			X = x;
			Y = y;
		}
	}
}
