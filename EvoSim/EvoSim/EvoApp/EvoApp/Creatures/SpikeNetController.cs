using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using EvoApp.NeuralNets.SpikeNet;
using EvoApp;
using EvoSim;
using TakaGUI;

namespace EvoApp.Creatures
{
	public class SpikeNetController : Creature
	{
		public override void Randomize()
		{
			base.Randomize();

			ShutDownBrain();
		}

		public void ShutDownBrain()
		{
			foreach (var input in Brain.Inputs)
				foreach (var connection in input.Connections)
					((SpConnection)connection).Weight = 0;
		}

		public override void Update(GameTime gameTime)
		{
			if (DrawBox.KeyboardInput.IsPressed(Microsoft.Xna.Framework.Input.Keys.Left))
				((SpOutputNeuron)rotate_O).Spike();
			//if (Pseudo.KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.Right))
			//    ((SpOutputNeuron)rotateRight_O).Spike();

			if (DrawBox.KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.Up))
				((SpOutputNeuron)move_O).Spike();

			//if (Pseudo.KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.Down))
			//    ((SpOutputNeuron)moveBackwards_O).Spike();

			if (DrawBox.KeyboardInput.IsPressed(Microsoft.Xna.Framework.Input.Keys.Q))
				((SpOutputNeuron)eat_O).Spike();

			if (DrawBox.KeyboardInput.IsClicked(Microsoft.Xna.Framework.Input.Keys.W))
				((SpOutputNeuron)mate_O).Spike();

			base.Update(gameTime);
		}
	}
}
