using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TakaGUI.Cores;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI.DrawBoxes;
using EvoApp.DrawBoxes;
using TakaGUI.Services;
using TakaGUI;
using EvoSim;
using System.Xml;
using EvoSim.Genes;
using System.IO;
using System.Diagnostics;
using TakaGUI.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EvoApp.States
{
	public class InitializeState : State
	{
		Menu.Editor editor;

		public override void Initialize()
		{
			Globals.GameScreen = new TakaGUI.Window();
			Globals.GUIManager.GameScreens.Add(Globals.GameScreen);

			DrawBox.KeyboardInput.Enabled = true;
			DrawBox.MouseInput.Enabled = true;
			Globals.GUIManager.Enabled = true;
			Globals.GUIManager.Visible = true;

			//FpsMeter fpsMeter = new FpsMeter();
			//fpsMeter.Initialize();
			//Globals.GameScreen.AddDrawBox(fpsMeter);

			editor = new Menu.Editor();
			editor.Initialize();
			Globals.Editor = editor;
			Globals.GameScreen.AddDrawBox(editor);
		}

		public override void Work()
		{
		}
	}
}
