using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoSim.RandomNumberGenerators;
using TakaGUI;
using EvoSim;
using System.Collections.ObjectModel;
using TakaGUI.DrawBoxes;
using TakaGUI.DrawBoxes.Forms;

namespace EvoApp.Worlds
{
	public class FitnessWorld : World
	{
		int newGenerationTick = 0;
		int newGenerationPeriod = 0;

		public int MinimumFood = 20;
		public int MinimumCreatures = 30;

		public FitnessWorld()
		{
		}
		public FitnessWorld(int regionSize, int gaussianMin, int gaussianMax)
			: base(regionSize, gaussianMin, gaussianMax)
		{
		}
		public FitnessWorld(int regionSize, UniformRandomGenerator random, NormalRandomGenerator gaussianRandom)
			: base(regionSize, random, gaussianRandom)
		{
		}

		void spawnNewGeneration()
		{
			//var orderedList = (from c in CreatureList orderby c.TimesHaveEaten descending select c).ToList();
			//RemoveAllEntities();

			//for (int i = 0; i < 2; i++)
			//{
			//    for (int n = 0; n < 2; n++)
			//    {
			//        var creature1 = orderedList[i];

			//        Creature creature2;
			//        creature2 = orderedList[Pseudo.Random.Next(orderedList.Count)];

			//        double posX = Pseudo.Random.Next(Width);
			//        double posY = Pseudo.Random.Next(Height);

			//        var child = creature1.CreateOffspring(creature2, posX, posY);
			//        child.Rotation = Math.PI * 2 * Pseudo.Random.NextDouble();

			//        child.Energy.SetToMax();

			//        AddEntity(child);
			//    }
			//}
		}
		static bool runTime = true;
		public override void Work(Microsoft.Xna.Framework.GameTime gameTime)
		{
			base.Work(gameTime);

			int foodCount = 0;
			foreach (Entity entity in EntityList)
				if (entity.EntityName == "Food")
					foodCount += 1;

			if (foodCount < MinimumFood)
			{
				var rand = new UniformRandomGenerator();

				for (int i = foodCount; i < MinimumFood; i++)
				{
					Food food = new Food();
					AddEntity(food);

					food.Position = new EntityPosition(rand.Next(Width), rand.Next(Height));
				}
			}

			int creatureCount = 0;
			foreach (Entity entity in EntityList)
				if (entity.EntityName == "Creature")
					creatureCount += 1;

			//if (creatureCount < MinimumCreatures)
			//{
			//    for (int i = creatureCount; i < MinimumCreatures; i++)
			//    {
			//        var creature1 = CreatureList[Pseudo.Random.Next(0, CreatureList.Count)];

			//        Creature creature2;
			//        do
			//        {
			//            creature2 = CreatureList[Pseudo.Random.Next(0, CreatureList.Count)];
			//        } while (creature1 == creature2);

			//        double posX = Pseudo.Random.Next(Width);
			//        double posY = Pseudo.Random.Next(Height);

			//        var child = creature1.CreateOffspring(creature2, posX, posY);
			//        child.Rotation = Math.PI * 2 * Pseudo.Random.NextDouble();

			//        child.Energy.SetToMax();

			//        AddEntity(child);
			//    }
			//}

			if (runTime)
				newGenerationTick += 1;

			if (newGenerationTick >= newGenerationPeriod)
			{
				newGenerationTick = 0;
				spawnNewGeneration();
			}
		}

		protected override IWorld GetNewInstance()
		{
			return new FitnessWorld();
		}
		protected override void CloneValues(IWorld world)
		{
			base.CloneValues(world);

			var clone = (FitnessWorld)world;

			clone.MinimumFood = MinimumFood;
			clone.MinimumCreatures = MinimumCreatures;
		}

		public static Func<bool> GUI_Edit(SingleSlotBox container, FitnessWorld world)
		{
			var builder = new FieldBuilder();
			builder.BuildSessionStart(container);

			Panel panel = new Panel();
			panel.Initialize();
			builder.AddDrawBoxAsField(panel, DrawBoxAlignment.GetFull());

			Func<bool> isReadyFunc = World.GUI_Edit(panel, world);

			var minFood = builder.AddIntegerField("Minimum Food: ");
			var minCreatures = builder.AddIntegerField("Minimum Creatures: ");

			builder.BuildSessionEnd();

			container.IsClosing += delegate(object sender)
			{
				world.MinimumFood = (int)minFood.Value;
				world.MinimumCreatures = (int)minCreatures.Value;
			};

			return delegate()
			{
				return isReadyFunc();
			};
		}
	}
}
