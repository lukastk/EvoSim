using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TakaGUI.Services;
using TakaGUI;
using System.Diagnostics;
using EvoSim.RandomNumberGenerators;
using System.Reflection;
using EvoApp.Menu;

namespace EvoApp
{
	public static class Globals
	{
		public static Game Game;

		public static Window GameScreen;
		public static IGUIManager GUIManager;

		public static Editor Editor;

		public static UniformRandomGenerator Random;
		public static NormalRandomGenerator GaussianRandom;

		public static string EditorDataSaveDir;

		static internal List<Type> GetAllTypesDeriving(Type baseType, Assembly currentAssembly, bool allowAllTypes = false)
		{
			var typeList = new List<Type>();

			var assemblyNameList = new List<AssemblyName>();
			assemblyNameList.Add(currentAssembly.GetName());
			assemblyNameList.AddRange(currentAssembly.GetReferencedAssemblies());

			foreach (var assemblyName in assemblyNameList)
			{
				var assembly = Assembly.Load(assemblyName);
				foreach (var type in assembly.GetTypes())
					if (baseType.IsAssignableFrom(type))
					{
						if (!allowAllTypes)
						{
							if (!type.IsClass)
								continue;
							if (type.IsAbstract)
								continue;
							if (type.IsGenericType)
								continue;
						}

						typeList.Add(type);
					}
			}

			return typeList;
		}
	}
}
