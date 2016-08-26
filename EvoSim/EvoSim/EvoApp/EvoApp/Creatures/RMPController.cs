using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using EvoApp.NeuralNets.RMP;
using EvoApp;
using EvoSim;
using TakaGUI;

namespace EvoApp.Creatures
{
	public class RMPController : Creature
	{
		public override void Randomize()
		{
			base.Randomize();

			ShutDownBrain();
		}

		public void ShutDownBrain()
		{
			IsThinking = false;
		}

		protected override void HandleInputNeurons()
		{
			HandleEyeInput();
		}

		public override void Update(GameTime gameTime)
		{
			Age = 0;
			Energy.SetToMax();

			if (DrawBox.KeyboardInput.IsPressed(Microsoft.Xna.Framework.Input.Keys.Left))
				rotate_O.Output = 1;
			else
				rotate_O.Output = 0;
			//if (Pseudo.KeyboardInput.IsPressed(Microsoft.Xna.Framework.Input.Keys.Right))
			//    rotateRight_O.Output = 1;
			//else
			//    rotateRight_O.Output = 0;

			if (DrawBox.KeyboardInput.IsPressed(Microsoft.Xna.Framework.Input.Keys.Up))
				move_O.Output = 1;
			else
				move_O.Output = 0;
			//if (Pseudo.KeyboardInput.IsPressed(Microsoft.Xna.Framework.Input.Keys.Down))
			//    moveBackwards_O.Output = 1;
			//else
			//    moveBackwards_O.Output = 0;

			if (DrawBox.KeyboardInput.IsPressed(Microsoft.Xna.Framework.Input.Keys.Q))
				eat_O.Output = 1;
			else
				eat_O.Output = 0;

			if (DrawBox.KeyboardInput.IsPressed(Microsoft.Xna.Framework.Input.Keys.W))
				mate_O.Output = 1;
			else
				mate_O.Output = 0;

			base.Update(gameTime);
		}
	}
}
