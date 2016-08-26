using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI;
using TakaGUI.Data;
using Microsoft.Xna.Framework;
using EvoApp.NeuralNets.RMP;
using EvoSim;

namespace EvoApp.DrawBoxes
{
	public class RMP_NetNumberVisualizer : DrawBox
	{
		public static string DefaultCategory = "Simulation/RMP_NetNumberVisualizer";
		public ISprite Circle;
		public ISprite SmallCircle;
		public MonoFont Font;

		public RMP_Net NeuralNet { get; private set; }
		Vector2[] neuronPositions;
		RMP_Neuron[] neurons;
		public RMP_Neuron SelectedNeuron;
		float graphWidth;
		float graphHeight;

		public WorldBox SelectingPool;

		public virtual void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			Circle = GetTexture(file, category, "Circle");
			SmallCircle = GetTexture(file, category, "SmallCircle");
			Font = GetMonoFont(file, category, "Font");

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
							var creature = (ICreature)entity;
							SetNewNeuralNet((RMP_Net)creature.Brain);
						}
					}
				}
			}

			if (neuronPositions != null && IsUnderMouse && MouseInput.IsPressed(TakaGUI.Services.MouseButtons.Left))
			{
				for (int i = 0; i < neuronPositions.Length; i++)
				{
					float x = (int)neuronPositions[i].X - 5 + (Width / 2 - graphWidth / 2 + RealX);
					float y = (int)neuronPositions[i].Y - 5 + (Height / 2 - graphHeight / 2 + RealY);
					var neuronRect = new Rectangle((int)x,
													(int)y,
													10, 10);

					if (MouseInput.X >= neuronRect.Left && MouseInput.Y >= neuronRect.Top &&
						MouseInput.X <= neuronRect.Right && MouseInput.Y <= neuronRect.Bottom)
					{
						neurons[i].Output = 1;
					}
				}
			}
		}

		public override void Project(Microsoft.Xna.Framework.GameTime gameTime, int x, int y, TakaGUI.Services.IRender render)
		{
			base.Project(gameTime, x, y, render);

			if (NeuralNet == null)
				return;

			render.Begin();

			render.DrawRect(new Rectangle(x, y, Width, Height), Color.White);

			float originX = Width / 2 - graphWidth / 2 + x;
			float originY = Height / 2 - graphHeight / 2 + y;

			for (int i = 0; i < neurons.Length; i++)
			{
				Vector2 neuronPos = neuronPositions[i];

				foreach (var connection in neurons[i].Connections)
				{
					Color color;
					if (connection.Weight < 0)
						color = new Color(-(float)connection.Weight, 0, 0);
					else
						color = new Color(0, (float)connection.Weight, 0);

					Vector2 neuron2Pos = neuronPositions[Array.IndexOf(neurons, connection.Target)];
					render.DrawLine(
						new Vector2(neuronPos.X + originX, neuronPos.Y + originY),
						new Vector2(neuron2Pos.X + originX, neuron2Pos.Y + originY),
						color);
				}
			}

			for (int i = 0; i < neurons.Length; i++)
			{
				string numStr = Math.Round(neurons[i].Output, 4).ToString();
				var vec = neuronPositions[i] + new Vector2(originX, originY) - new Vector2(Font.CharWidth / 2, Font.CharHeight / 2);
				var strSize = Font.MeasureString(numStr);
				render.DrawRect(new Rectangle((int)vec.X - 1, (int)vec.Y - 1, strSize.X + 1, strSize.Y + 1), Color.White);
				Font.DrawString(numStr, new Point((int)vec.X, (int)vec.Y), Color.Black, render);
			}

			render.End();
		}

		public void SetNewNeuralNet(RMP_Net net)
		{
			int charRadius = Math.Max(Font.CharWidth, Font.CharHeight) + 5;

			NeuralNet = net;

			var neuronList = new List<RMP_Neuron>();
			neuronList.AddRange(net.HiddenNeurons);
			neuronList.AddRange(net.InputNeurons);
			neuronList.AddRange(net.OutputNeurons);
			neurons = neuronList.ToArray();

			const int neuronMargin = 15;
			const int inputHiddenMargin = 10;
			const int hiddenOutputMargin = 10;

			double radius = (net.HiddenNeurons.Count * (neuronMargin + charRadius) / Math.PI / 2);
			double rotationStep = Math.PI * 2 / net.HiddenNeurons.Count;
			double rotation = 0;

			neuronPositions = new Vector2[neurons.Length];

			int i;
			for (i = 0; i < net.HiddenNeurons.Count; i++)
			{																	//Margin between input neurons and hidden neurons.
				neuronPositions[i].X = (float)(Math.Cos(rotation) * radius) + charRadius + inputHiddenMargin + (float)radius;
				neuronPositions[i].Y = (float)(Math.Sin(rotation) * radius);

				rotation += rotationStep;
			}

			const int inputNeuronMargin = 2;
			int inputY = 0;
			for (; i < net.HiddenNeurons.Count + net.InputNeurons.Count; i++)
			{
				neuronPositions[i].Y = inputY;
				inputY += inputNeuronMargin + charRadius;
			}

			const int outputNeuronMargin = 2;
			int outputY = 0;
			for (; i < net.HiddenNeurons.Count + net.InputNeurons.Count + net.OutputNeurons.Count; i++)
			{
				neuronPositions[i].X = charRadius + inputHiddenMargin + charRadius + (float)radius * 2 + hiddenOutputMargin;
				neuronPositions[i].Y = outputY;
				outputY += outputNeuronMargin + charRadius; ;
			}

			for (i = 0; i < neuronPositions.Length; i++)
			{
				if (neuronPositions[i].X > graphWidth)
					graphWidth = neuronPositions[i].X;
				if (neuronPositions[i].Y > graphHeight)
					graphHeight = neuronPositions[i].Y;
			}
		}
	}
}
