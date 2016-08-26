using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI;
using TakaGUI.Data;
using Microsoft.Xna.Framework;
using TakaGUI.Services;
using EvoSim;
using EvoApp;
using EvoApp.EntityPainters;

namespace EvoApp.DrawBoxes
{
	public class WorldBox : DrawBox
	{
		public static string DefaultCategory = "Simulation/WorldBox";
		public ISprite Creature;
		public ISprite Food;

		public MonoFont Font;

		public Camera ViewCamera;
		public IWorld World;

		float[] zoomModes = new float[] { 0.25f, 0.5f, 1, 2, 3, 4, 5 };
		int zoomMode = 2;

		public bool ShowEnergyLevels = true;
		public bool ShowRegions = true;

		bool isDragging = false;
		Point dragOrigin;

		public IEntity SelectedEntity;

		List<string> stringsToDraw = new List<string>();

		public Dictionary<Type, List<IEntityPainter>> EntityPaintersDictionary = new Dictionary<Type, List<IEntityPainter>>();

		public WorldBox()
		{
			ViewCamera.Zoom = 1f;
		}

		public virtual void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			Creature = GetTexture(file, category, "Creature");
			Food = GetTexture(file, category, "Food");

			Font = GetMonoFont(file, category, "Font");

			base.BaseInitialize();
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			if (World == null)
				return;

			if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.Space))
				Hidden = !Hidden;

			if (IsUnderMouse && MouseInput.IsClicked(MouseButtons.Left))
			{
				isDragging = true;
				dragOrigin = new Point(MouseInput.X, MouseInput.Y);
			}
			else if (!MouseInput.IsPressed(MouseButtons.Left))
				isDragging = false;

			if (isDragging)
			{
				ViewCamera.LookX -= MouseInput.DeltaX;
				ViewCamera.LookY -= MouseInput.DeltaY;
			}

			if (MouseInput.DeltaScroll != 0)
			{
				double add = MouseInput.DeltaScroll / 120;

				zoomMode += (int)Math.Round(add, 0);

				if (zoomMode < 0)
					zoomMode = 0;
				else if (zoomMode > zoomModes.Length - 1)
					zoomMode = zoomModes.Length - 1;

				ViewCamera.Zoom = zoomModes[zoomMode];

				if ((ViewCamera.LookX / ViewCamera.Zoom) > World.Width)
					ViewCamera.LookX = 0;
				if ((ViewCamera.LookY / ViewCamera.Zoom) > World.Height)
					ViewCamera.LookY = 0;
			}

			if (IsUnderMouse && MouseInput.IsClicked(MouseButtons.Left))
			{
				foreach (var e in World.EntityList)
				{
					int radius = (int)Math.Round(e.Radius * ViewCamera.Zoom);
					Rectangle destRect = new Rectangle((int)Math.Round(RealX - ViewCamera.LookX + e.Position.X * ViewCamera.Zoom, 0) - radius,
														(int)Math.Round(RealY - ViewCamera.LookY + e.Position.Y * ViewCamera.Zoom, 0) - radius,
														radius * 2,
														radius * 2);

					if (MouseInput.X >= destRect.Left && MouseInput.Y >= destRect.Top &&
						MouseInput.X <= destRect.Right && MouseInput.Y <= destRect.Bottom)
					{
						SelectedEntity = e;
					}
				}
			}

			World.Update(gameTime);
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();
			
			if (World == null)
			{
				Font.DrawString("No World", new Point(x + Width / 2, y + Height / 2), new Vector2(0, 0), Color.White, render);
				render.End();
				return;
			}

			float originX = ViewCamera.LookX;
			float originY = ViewCamera.LookY;

			if (ShowRegions)
			{
				var gridColor = new Color(100, 100, 100, 100);

				int width = (int)Math.Round(World.Width * ViewCamera.Zoom);
				int height = (int)Math.Round(World.Height * ViewCamera.Zoom);
				int regionSize = (int)Math.Round(World.RegionSize * ViewCamera.Zoom);

				int stop = width + x;
				for (int gridX = x; gridX < stop; gridX += regionSize)
					render.DrawLine(new Vector2(gridX - originX, y - originY), new Vector2(gridX - originX, y + height - originY), gridColor);

				stop = height + y;
				for (int gridY = y; gridY < stop; gridY += regionSize)
					render.DrawLine(new Vector2(x - originX, gridY - originY), new Vector2(x + width - originX, gridY - originY), gridColor);
			}

			foreach (Entity entity in World.EntityList)
			{
				//Color color = entity.EntityColor;

				//switch (entity.EntityName)
				//{
				//    case "Creature":
				//        sprite = Creature;

				//        double energyQ = ((Creature)entity).Energy.Value / ((Creature)entity).Energy.Max;
				//        color.R = (byte)Math.Round(color.R * energyQ, 0);
				//        color.G = (byte)Math.Round(color.G * energyQ, 0);
				//        color.B = (byte)Math.Round(color.B * energyQ, 0);
				//        break;
				//    case "Food":
				//        sprite = Food;
				//        break;
				//}

				Type eType = entity.GetType();

				int radius = (int)Math.Round(entity.Radius * ViewCamera.Zoom);
				Rectangle destRect = new Rectangle((int)Math.Round(x - originX + entity.Position.X * ViewCamera.Zoom, 0),
													(int)Math.Round(y - originY + entity.Position.Y * ViewCamera.Zoom, 0),
													radius * 2,
													radius * 2);

				if (EntityPaintersDictionary.ContainsKey(eType))
				{
					foreach (var painter in EntityPaintersDictionary[eType])
					{
						painter.Draw(gameTime, entity, destRect.X, destRect.Y, ViewCamera.Zoom, render);
					}
				}

				//render.DrawSprite(sprite, destRect, new Vector2(radius, radius), Color.Red, (float)entity.Rotation);

				if (entity == SelectedEntity)
				{
					destRect.X -= destRect.Width / 2;
					destRect.Y -= destRect.Height / 2;

					render.DrawHorizontalLine(new Point(destRect.X, destRect.Y), destRect.Width, Color.White);
					render.DrawHorizontalLine(new Point(destRect.X, destRect.Bottom), destRect.Width, Color.White);
					render.DrawVerticalLine(new Point(destRect.X, destRect.Y), destRect.Height, Color.White);
					render.DrawVerticalLine(new Point(destRect.Right, destRect.Y), destRect.Height, Color.White);
				}

				//if (entity.EntityName == "Creature" && entity == infoDrawCreature)
				//{
				//    var creature = (Creature)entity;
				//    double rotation = (creature.Rotation - creature.EyeSpan / 2);
				//    double rotationStep = creature.EyeSpan / creature.EyeNeuronsAmount;
				//    float length = infoDrawCreature.ViewDistance;

				//    for (int i = 0; i < creature.EyeNeuronsAmount + 1; i++)
				//    {
				//        float posX = (float)destRect.X;
				//        float posY = (float)destRect.Y;
				//        float lineX = (float)Math.Cos(rotation) * length + posX;
				//        float lineY = (float)Math.Sin(rotation) * length + posY;
				//        render.DrawLine(new Vector2(posX, posY), new Vector2(lineX, lineY), Color.Red);
				//        rotation += rotationStep;
				//    }
				//}
			}

			int tX = 10 + x;
			int tY = 30 + y;
			foreach (string s in stringsToDraw)
			{
				Font.DrawString(s, new Point(tX, tY), Color.White, render);
				tY += Font.CharHeight + 2;
			}

			stringsToDraw.Clear();

			render.End();
		}

		public struct Camera
		{
			int lookX;
			int lookY;
			public int LookX
			{
				get
				{
					if (LookAt == null)
						return lookX;
					else
						return (int)LookAt.Position.X;
				}
				set
				{
					lookX = value;
				}
			}
			public int LookY
			{
				get
				{
					if (LookAt == null)
						return lookY;
					else
						return (int)LookAt.Position.Y;
				}
				set
				{
					lookY = value;
				}
			}

			public float Zoom;

			public Entity LookAt;
		}
	}
}
