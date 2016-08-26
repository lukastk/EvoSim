using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI;
using TakaGUI.Data;
using Microsoft.Xna.Framework;
using EvoSim;
using EvoApp.NeuralNets.SpikeNet;

namespace EvoApp.DrawBoxes
{
	public class SpikeNetVisualizer : DrawBox
	{
		public static string DefaultCategory = "Simulation/SpikeNetVisualizer";
		public ISprite Circle;
		public ISprite SmallCircle;

		int neuronGBValue = 160;

		public SpikeNet NeuralNet { get; private set; }
		Vector2[] neuronPositions;
		SpNeuron[] neurons;
		public SpNeuron SelectedNeuron;
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
							SetNewNeuralNet((SpikeNet)creature.Brain);
						}
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
					Vector2 neuron2Pos = neuronPositions[Array.IndexOf(neurons, connection.Target)];
					render.DrawLine(
						new Vector2(neuronPos.X + Circle.Width / 2 + originX, neuronPos.Y + Circle.Height / 2 + originY),
						new Vector2(neuron2Pos.X + Circle.Width / 2 + originX, neuron2Pos.Y + Circle.Height / 2 + originY),
						Color.Black);
				}
			}

			for (int i = 0; i < neurons.Length; i++)
			{
				int neuronGB = (int)Math.Round(neuronGBValue - (neuronGBValue * (neurons[i].Excitation / neurons[i].Bias)), 0);

				Color color;
				if (neurons[i].NewSpike)
					color = Color.Red;
				else
					color = new Color(255, neuronGB, neuronGB);

				render.DrawSprite(Circle, neuronPositions[i] + new Vector2(originX, originY), color);
			}

			foreach (Spike spike in NeuralNet.Spikes)
			{
				Vector2 pre = neuronPositions[Array.IndexOf(neurons, spike.Source)];
				Vector2 post = neuronPositions[Array.IndexOf(neurons, spike.Target)];

				double dx = pre.X - post.X;
				double dy = pre.Y - post.Y;
				double angle = Math.Atan(dy / dx);

				if (dx >= 0)
					angle += Math.PI;

				double length = Math.Sqrt(
								Math.Pow(pre.X - post.X, 2) +
								Math.Pow(pre.Y - post.Y, 2));
				double distance = length - length * ((double)spike.CyclesToImpact / spike.Connection.Delay);

				float spikeX = (float)Math.Round(Math.Cos(angle) * distance, 0) + pre.X + Circle.Width / 2;
				float spikeY = (float)Math.Round(Math.Sin(angle) * distance, 0) + pre.Y + Circle.Height / 2;

				Color color = new Color(255, 255, 0);

				if (spike.Strength > 0)
					color.R -= (byte)Math.Round(255 * spike.Strength, 0);
				if (spike.Strength < 0)
					color.G += (byte)Math.Round(255 * spike.Strength, 0);

				render.DrawSprite(SmallCircle, new Vector2(spikeX + originX, spikeY + originY), color);
			}

			render.End();
		}

		public void SetNewNeuralNet(SpikeNet net)
		{
			NeuralNet = net;

			var neuronList = new List<SpNeuron>();
			neuronList.AddRange(net.HiddenNeurons);
			neuronList.AddRange(net.InputNeurons);
			neuronList.AddRange(net.OutputNeurons);
			neurons = neuronList.ToArray();

			const int neuronMargin = 15;
			const int inputHiddenMargin = 10;
			const int hiddenOutputMargin = 10;

			double radius = (net.HiddenNeurons.Count * (neuronMargin + Circle.Width) / Math.PI / 2);
			double rotationStep = Math.PI * 2 / net.HiddenNeurons.Count;
			double rotation = 0;

			neuronPositions = new Vector2[neurons.Length];

			int i;
			for (i = 0; i < net.HiddenNeurons.Count; i++)
			{																	//Margin between input neurons and hidden neurons.
				neuronPositions[i].X = (float)(Math.Cos(rotation) * radius) + Circle.Width + inputHiddenMargin + (float)radius;
				neuronPositions[i].Y = (float)(Math.Sin(rotation) * radius);

				rotation += rotationStep;
			}

			const int inputNeuronMargin = 2;
			int inputY = 0;
			for (; i < net.HiddenNeurons.Count + net.InputNeurons.Count; i++)
			{
				neuronPositions[i].Y = inputY;
				inputY += inputNeuronMargin + Circle.Height;
			}

			const int outputNeuronMargin = 2;
			int outputY = 0;
			for (; i < net.HiddenNeurons.Count + net.InputNeurons.Count + net.OutputNeurons.Count; i++)
			{
				neuronPositions[i].X = Circle.Width + inputHiddenMargin + Circle.Width + (float)radius * 2 + hiddenOutputMargin;
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
