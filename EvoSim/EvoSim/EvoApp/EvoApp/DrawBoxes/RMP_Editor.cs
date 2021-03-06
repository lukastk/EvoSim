﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI;
using TakaGUI.Data;
using Microsoft.Xna.Framework;
using EvoSim;
using EvoApp.NeuralNets.RMP;
using EvoApp;

namespace EvoApp.DrawBoxes
{
	public class RMP_Editor : DrawBox
	{
		public static string DefaultCategory = "Simulation/RMP_NetNumberVisualizer";
		public ISprite Circle;
		public ISprite SmallCircle;
		public MonoFont Font;

		public ICreature SelectedCreature;
		public RMP_Net NeuralNet { get; private set; }
		Vector2[] neuronPositions;
		RMP_Neuron[] neurons;
		int selectedNeuron;
		float graphWidth;
		float graphHeight;

		public WorldBox SelectingPool;

		double connectionWeight;

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

			if (SelectedCreature != null)
			{
				SelectedCreature.Age = 0;
				SelectedCreature.Energy.SetToMax();
			}

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
							SelectedCreature = (Creature)entity;
							SetNewNeuralNet((RMP_Net)SelectedCreature.Brain);
						}
					}
				}
			}

			if (NeuralNet == null)
				return;

			if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.D1))
				connectionWeight = 10 * (1.0 / 10) - 5;
			else if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.D2))
				connectionWeight = 10 * (2.0 / 10) - 5;
			else if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.D3))
				connectionWeight = 10 * (3.0 / 10) - 5;
			else if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.D4))
				connectionWeight = 10 * (4.0 / 10) - 5;
			else if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.D5))
				connectionWeight = 10 * (5.0 / 10) - 5;
			else if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.D6))
				connectionWeight = 10 * (6.0 / 10) - 5;
			else if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.D7))
				connectionWeight = 10 * (7.0 / 10) - 5;
			else if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.D8))
				connectionWeight = 10 * (8.0 / 10) - 5;
			else if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.D9))
				connectionWeight = 10 * (9.0 / 10) - 5;
			else if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.D0))
				connectionWeight = 10 * (10.0 / 10) - 5;
			else if (KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.Q))
				connectionWeight = Globals.Random.NextDouble(-10, 10);

			if (IsUnderMouse)
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
						if (MouseInput.IsPressed(TakaGUI.Services.MouseButtons.Right))
						{
							neurons[i].Activation += 100;
						}
						else if (MouseInput.IsClicked(TakaGUI.Services.MouseButtons.Left))
						{
							selectedNeuron = i;
						}
						else if (selectedNeuron != -1 && KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.C))
						{
							var con = new RMP_Connection();
							con.Weight = connectionWeight;

							con.Source = neurons[selectedNeuron];
							con.Target = neurons[i];

							neurons[selectedNeuron].Connections.Add(con);
						}
					}
				}
			}

			if (IsUnderMouse && KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.A))
			{
				var posList = new List<Vector2>(neuronPositions);
				var neuronList = new List<RMP_Neuron>(neurons);

				var neuron = new RMP_Neuron(NeuralNet);
				neuron.SigmoidFunction = (SigmoidFunction)neuronList[0].SigmoidFunction.Clone();
				NeuralNet.AddHiddenNeuron(neuron);

				float x = MouseInput.X - (Width / 2 - graphWidth / 2 + RealX);
				float y = MouseInput.Y - (Height / 2 - graphHeight / 2 + RealY);
				posList.Add(new Vector2(x, y));
				neuronList.Add(neuron);

				neuronPositions = posList.ToArray();
				neurons = neuronList.ToArray();
			}

			if (IsUnderMouse && KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.R))
			{
				foreach (var n in NeuralNet.HiddenNeurons)
					n.Activation = Globals.Random.NextDouble(-1, 1);
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
				string numStr = Math.Round(neurons[i].Output, 1).ToString();
				var vec = neuronPositions[i] + new Vector2(originX, originY) - new Vector2(Font.CharWidth / 2, Font.CharHeight / 2);
				var strSize = Font.MeasureString(numStr);
				render.DrawRect(new Rectangle((int)vec.X - 1, (int)vec.Y - 1, strSize.X + 1, strSize.Y + 1), Color.White);
				Font.DrawString(numStr, new Point((int)vec.X, (int)vec.Y), Color.Black, render);
			}

			render.End();
		}

		public void SetNewNeuralNet(RMP_Net net)
		{
			NeuralNet = net;

			NeuralNet.RemoveAllHiddenNeurons();

			foreach (var inputN in NeuralNet.InputNeurons)
				inputN.Connections.Clear();

			var neuronList = new List<RMP_Neuron>();
			neuronList.AddRange(net.HiddenNeurons);
			neuronList.AddRange(net.InputNeurons);
			neuronList.AddRange(net.OutputNeurons);
			neurons = neuronList.ToArray();

			const int inputHiddenMargin = 10;
			const int hiddenOutputMargin = 30;

			neuronPositions = new Vector2[neurons.Length];

			const int inputNeuronMargin = 2;
			int inputY = 0;
			int i;
			for (i = 0; i < net.HiddenNeurons.Count + net.InputNeurons.Count; i++)
			{
				neuronPositions[i].Y = inputY;
				inputY += inputNeuronMargin + Circle.Height;
			}

			const int outputNeuronMargin = 2;
			int outputY = 0;
			for (; i < net.HiddenNeurons.Count + net.InputNeurons.Count + net.OutputNeurons.Count; i++)
			{
				neuronPositions[i].X = Circle.Width + inputHiddenMargin + Circle.Width + hiddenOutputMargin;
				neuronPositions[i].Y = outputY;
				outputY += outputNeuronMargin + Circle.Height; ;
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
