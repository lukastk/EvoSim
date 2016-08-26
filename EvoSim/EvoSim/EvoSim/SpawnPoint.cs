using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoSim;
using Microsoft.Xna.Framework;

namespace EvoSim
{
	public class SpawnPoint
	{
		IWorld world;

		public string Name;

		public Rectangle SpawnArea;
		public List<IEntity> SpawnClones = new List<IEntity>();

		public double TimeInterval
		{
			get;
			private set;
		}
		TimeSpan timeLeft;

		public SpawnPoint(IWorld _world)
		{
			world = _world;
		}

		public void Update(GameTime gameTime)
		{
			if (SpawnArea.X < 0)
				SpawnArea.X = 0;
			else if (SpawnArea.X >= world.Width)
				SpawnArea.X = world.Width - 1;

			if (SpawnArea.Y < 0)
				SpawnArea.Y = 0;
			else if (SpawnArea.Y >= world.Height)
				SpawnArea.Y = world.Height - 1;

			if (SpawnArea.Right > world.Width)
				SpawnArea.Width = world.Width - SpawnArea.X;
			if (SpawnArea.Bottom > world.Height)
				SpawnArea.Height = world.Height - SpawnArea.Y;

			timeLeft.Subtract(gameTime.ElapsedGameTime);

			if (timeLeft.CompareTo(TimeSpan.Zero) <= 0)
			{
				foreach (var e in SpawnClones)
				{
					var clone = e.Clone();

					var pos = new EntityPosition();
					pos.X = Pseudo.Random.Next(SpawnArea.Left, SpawnArea.Right);
					pos.Y = Pseudo.Random.Next(SpawnArea.Top, SpawnArea.Bottom);

					clone.Position = pos;
					world.AddEntity(clone);
				}

				timeLeft = TimeSpan.FromSeconds(TimeInterval);
			}
		}

		public void SetTimeInterval(double seconds)
		{
			TimeInterval = seconds;
			timeLeft = TimeSpan.FromSeconds(TimeInterval);
		}
	}
}
