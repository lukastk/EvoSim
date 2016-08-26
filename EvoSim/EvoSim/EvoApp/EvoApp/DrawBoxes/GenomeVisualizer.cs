using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI;
using Microsoft.Xna.Framework;
using TakaGUI.Data;
using System.Collections;
using TakaGUI.Services;
using EvoSim.Genes;
using EvoSim;
using EvoApp.NeuralNets.RMP;

namespace EvoApp.DrawBoxes
{
	public class GenomeVisualizer : DrawBox
	{
		public static string DefaultCategory = "Simulation/GenomeVisualizer";

		public WorldBox SelectingPool;
		public Genome Genome;
		public Genome Genome2;
		public Genome ChildGenome;
		public MonoFont Font;

		public virtual void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			Font = GetMonoFont(file, category, "Font");

			base.BaseInitialize();
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			if (SelectingPool != null && SelectingPool.IsUnderMouse)
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
							if (MouseInput.IsClicked(TakaGUI.Services.MouseButtons.Left))
								Genome = ((ICreature)entity).CreatureGenome;
							else if (MouseInput.IsClicked(TakaGUI.Services.MouseButtons.Right))
								Genome2 = ((ICreature)entity).CreatureGenome;

							if ( ( MouseInput.IsClicked(TakaGUI.Services.MouseButtons.Left) ||
								MouseInput.IsClicked(TakaGUI.Services.MouseButtons.Right)) &&
								(Genome != null && Genome2 != null))
								ChildGenome = Genome.GetChildGenome(Genome2);
						}
					}
				}
			}
		}

		public override void Project(Microsoft.Xna.Framework.GameTime gameTime, int x, int y, IRender render)
		{
			base.Project(gameTime, x, y, render);

			const int geneHeight = 3;

			render.Begin();
			render.DrawRect(new Rectangle(x, y, Width, Height), Color.White);



			//Genome1
			if (Genome == null)
			{
				render.End();
				return;
			}
			var geneDict = Genome.NetChromosome.GetGeneInfo();

			int headerX = x + 5;
			int posY = y + 5;

			int genome2PosY = posY;

			foreach (var geneListKey in geneDict.Keys)
			{
				var geneList = geneDict[geneListKey];

				int stringWidth = Font.MeasureString(geneListKey).X;
				Font.DrawString(geneListKey, new Point(headerX, posY), Color.Black, render);
				genome2PosY = Math.Max(DrawGenes(headerX, posY + Font.CharHeight + 2, geneList, render, stringWidth, geneHeight), genome2PosY);
				headerX += stringWidth + 4;
			}

			//Genome2
			if (Genome2 == null)
			{
				render.End();
				return;
			}
			geneDict = Genome2.NetChromosome.GetGeneInfo();
			headerX = x + 5;
			posY = genome2PosY += 5;

			foreach (var geneListKey in geneDict.Keys)
			{
				var geneList = geneDict[geneListKey];

				int stringWidth = Font.MeasureString(geneListKey).X;
				Font.DrawString(geneListKey, new Point(headerX, posY), Color.Black, render);
				genome2PosY = Math.Max(DrawGenes(headerX, posY + Font.CharHeight + 2, geneList, render, stringWidth, geneHeight), genome2PosY);
				headerX += stringWidth + 4;
			}

			//ChildGenome
			if (ChildGenome == null)
			{
				render.End();
				return;
			}
			geneDict = ChildGenome.NetChromosome.GetGeneInfo();
			headerX = x + 5;
			posY = genome2PosY += 5;

			foreach (var geneListKey in geneDict.Keys)
			{
				var geneList = geneDict[geneListKey];

				int stringWidth = Font.MeasureString(geneListKey).X;
				Font.DrawString(geneListKey, new Point(headerX, posY), Color.Black, render);
				genome2PosY = Math.Max(DrawGenes(headerX, posY + Font.CharHeight + 2, geneList, render, stringWidth, geneHeight), genome2PosY);
				headerX += stringWidth + 4;
			}

			render.End();
		}

		int DrawGenes(int posX, int posY, IList geneList, IRender render, int geneWidth, int geneHeight)
		{
			foreach (var gene in geneList)
			{
				string geneType = gene.GetType().Name;

				switch (geneType)
				{
					case "DoubleGene":
						var dGene = (DoubleGene)gene;
						float val = (float)((dGene.Value - dGene.Min) / (dGene.Max - dGene.Min));
						var color = new Color(val, val, val);

						render.DrawRect(new Rectangle(posX, posY, geneWidth, geneHeight), color);
						break;
					case "RMP_NeuronGene":
						var nGene = (RMP_NeuronGene)gene;

						var colorList = new List<Color>();
						colorList.Add(GetColorFromDouble(nGene.Bias.Value));

						foreach (var c in nGene.Connections)
							colorList.Add(GetColorFromDouble(c.Weight.Value));

						DrawHorizontalColors(posX, posY, geneWidth, geneHeight, colorList, render);

						break;
				}

				posY += geneHeight;
			}

			return posY;
		}

		Color GetColorFromDouble(double val)
		{
			return new Color((float)val, (float)val, (float)val);
		}

		void DrawHorizontalColors(int posX, int posY, int width, int height, List<Color> colors, IRender render)
		{
			int colorWidth = width / colors.Count;

			foreach (var color in colors)
			{
				render.DrawRect(new Rectangle(posX, posY, colorWidth, height), color);
				posX += colorWidth;
			}
		}
	}
}
