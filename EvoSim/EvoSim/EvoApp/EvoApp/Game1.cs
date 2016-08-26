using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TakaGUI.Cores;
using TakaGUI;
using TakaGUI.Data;
using TakaGUI.IO;
using System.IO;
using EvoApp.States;
using TakaGUI.Services;
using EvoSim.RandomNumberGenerators;
using EvoSim;

namespace EvoApp
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		string defaultSkin;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();
		}

		void Config()
		{
			var config = new IniFile(Path.Combine(Environment.CurrentDirectory, "config.ini"));

			string section;
			/////////////////////////////////////////////////////////////////////////////////////////////////
			section = "Graphics"; //-------------------------------------------------------------------------
			graphics.PreferredBackBufferWidth = config.SetVal(graphics.PreferredBackBufferWidth, section, "PreferredBackBufferWidth", "ToInt32");
			graphics.PreferredBackBufferHeight = config.SetVal(graphics.PreferredBackBufferHeight, section, "PreferredBackBufferHeight", "ToInt32");
			graphics.IsFullScreen = config.SetVal(graphics.IsFullScreen, section, "IsFullScreen", "ToBoolean");
			this.IsFixedTimeStep = config.SetVal(this.IsFixedTimeStep, section, "IsFixedTimeStep", "ToBoolean");
			defaultSkin = config.SetVal(this.defaultSkin, section, "DefaultSkinFile", "ToString");
			Globals.EditorDataSaveDir = config.SetVal(this.defaultSkin, section, "EditorDataSaveDir", "ToString");
			/////////////////////////////////////////////////////////////////////////////////////////////////

			graphics.ApplyChanges();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			Config();

			Globals.Game = this;

			Menu.EditorData.Load(Globals.EditorDataSaveDir);

			DrawBox.InitializeDefaultServices(this, graphics, defaultSkin);

			DrawBox.KeyboardInput.Enabled = false;
			DrawBox.MouseInput.Enabled = false;

			var guiManager = new GUIManager(this);
			guiManager.Initialize();
			guiManager.Enabled = false;
			guiManager.Visible = false;
			Services.AddService(typeof(GUIManager), guiManager);
			Globals.GUIManager = guiManager;

			Push.InitializeServices(DrawBox.Debug);

			Globals.Random = new EvoSim.RandomNumberGenerators.UniformRandomGenerator();
			Globals.GaussianRandom = new EvoSim.RandomNumberGenerators.NormalRandomGenerator(0, 1000, Globals.Random); //Initialize the Gaussian with the same random nubmer generator.

			Pseudo.Static_Random = Globals.Random;
			Pseudo.Static_GaussianRandom = Globals.GaussianRandom;

			//StateManager.Push(new States.InitializeState(), new GameTime());
			StateManager.Push(new States.InitializeState(), new GameTime());
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			Globals.GameScreen.Close();
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param description="gameTime">Provides a snapshot of timing rowValues.</param>
		protected override void Update(GameTime gameTime)
		{
			StateManager.Update(gameTime);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param description="gameTime">Provides a snapshot of timing rowValues.</param>
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.Black);

			base.Draw(gameTime);
		}
	}
}
