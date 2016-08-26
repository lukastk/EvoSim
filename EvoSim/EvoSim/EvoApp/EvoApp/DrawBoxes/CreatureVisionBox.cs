using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI;
using Microsoft.Xna.Framework;
using EvoSim;

namespace EvoApp.DrawBoxes
{
	public class CreatureVisionBox : DrawBox
	{
		public Creature Creature;

		public WorldBox SelectingPool;

		public void Initialize(Creature creature)
		{
			Creature = creature;

			base.BaseInitialize();
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			if (SelectingPool != null && SelectingPool.IsUnderMouse && MouseInput.IsClicked(TakaGUI.Services.MouseButtons.Left))
			{
				if (SelectingPool != null)
				{
					foreach (var entity in SelectingPool.World.EntityList)
					{
						if (entity.EntityName != "Creature")
							continue;

						var entityRect = new Rectangle((int)entity.Position.X + SelectingPool.RealX - SelectingPool.ViewCamera.LookX - entity.Radius / 2,
														(int)entity.Position.Y + SelectingPool.RealY - SelectingPool.ViewCamera.LookY - entity.Radius / 2,
														entity.Radius,
														entity.Radius);

						if (MouseInput.X >= entityRect.Left && MouseInput.Y >= entityRect.Top &&
							MouseInput.X <= entityRect.Right && MouseInput.Y <= entityRect.Bottom)
						{
							Creature = (Creature)entity;
						}
					}
				}
			}
		}

		public override void Project(Microsoft.Xna.Framework.GameTime gameTime, int x, int y, TakaGUI.Services.IRender render)
		{
			base.Project(gameTime, x, y, render);

			if (Creature == null)
				return;

			int posX = x;
			int w = (int)Math.Round((float)Width / Creature.EyeNeuronsAmount, 0);
			int h = (int)Math.Round((float)Height / 2, 0);

			render.Begin();

			int i;
			for (i = 0; i < Creature.EyeNeuronsAmount; i++)
			{
				render.DrawRect(new Rectangle(posX, y, w, h), new Color(
					(float)Creature.Brain.Inputs[i].FireFrequency,
					(float)Creature.Brain.Inputs[i + Creature.EyeNeuronsAmount].FireFrequency,
					(float)Creature.Brain.Inputs[i + Creature.EyeNeuronsAmount * 2].FireFrequency));
				render.DrawRect(new Rectangle(posX, y + h, w, h), new Color(
					(float)Creature.Brain.Inputs[i + Creature.EyeNeuronsAmount * 3].FireFrequency,
					(float)Creature.Brain.Inputs[i + Creature.EyeNeuronsAmount * 3].FireFrequency,
					(float)Creature.Brain.Inputs[i + Creature.EyeNeuronsAmount * 3].FireFrequency));

				posX += w;
			}

			render.End();
		}
	}
}
