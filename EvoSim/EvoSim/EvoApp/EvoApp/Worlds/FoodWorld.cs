using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;
using EvoSim;
using EvoSim.RandomNumberGenerators;
using EvoApp.Menu.EntitiesMenu;

namespace EvoApp.Worlds
{
	public class FoodWorld : World
	{
		public Food FoodToClone;

		public int MinimumFood = 70;
		public int FoodSpawnRate = 1;
		int TimeUntilFoodSpawn = 0;

		public FoodWorld()
		{
			FoodToClone = new Food();
		}
		public FoodWorld(int regionSize, int gaussianMin, int gaussianMax)
			: base(regionSize, gaussianMin, gaussianMax)
		{
			FoodToClone = new Food();
		}
		public FoodWorld(int regionSize, UniformRandomGenerator random, NormalRandomGenerator gaussianRandom)
			: base(regionSize, random, gaussianRandom)
		{
			FoodToClone = new Food();
		}

		public void CreateInitalFood()
		{
			for (int i = 0; i < MinimumFood; i++)
				CreateFood();
		}

		public override void Work(Microsoft.Xna.Framework.GameTime gameTime)
		{
			base.Work(gameTime);

			int foodCount = 0;
			foreach (Entity entity in EntityList)
				if (entity.EntityName == "Food")
					foodCount += 1;

			if (foodCount < MinimumFood)
			{
				TimeUntilFoodSpawn -= 1;

				if (TimeUntilFoodSpawn < 0)
					TimeUntilFoodSpawn = FoodSpawnRate;

				if (TimeUntilFoodSpawn == 0)
				{
					CreateFood();
					TimeUntilFoodSpawn = 0;
				}
			}
		}

		void CreateFood()
		{
			Food food = (Food)FoodToClone.Clone();
			AddEntity(food);
			
			food.Position = new EntityPosition(Pseudo.Random.Next(Width), Pseudo.Random.Next(Height));
		}

		protected override IWorld GetNewInstance()
		{
			return new FoodWorld();
		}
		protected override void CloneValues(IWorld world)
		{
			base.CloneValues(world);

			var clone = (FoodWorld)world;

			clone.FoodToClone = (Food)FoodToClone.Clone();
			clone.MinimumFood = MinimumFood;
			clone.FoodSpawnRate = FoodSpawnRate;
		}

		public static Func<bool> GUI_Edit(SingleSlotBox container, FoodWorld world)
		{
			var builder = new FieldBuilder();
			builder.BuildSessionStart(container);

			Panel panel = new Panel();
			panel.Initialize();
			builder.AddDrawBoxAsField(panel, DrawBoxAlignment.GetFull());

			Func<bool> isReadyFunc = World.GUI_Edit(panel, world);

			var minFood = builder.AddIntegerField("Minimum Food: ");
			minFood.Value = world.MinimumFood;
			var foodSpawnRate = builder.AddIntegerField("Food Spawn-rate: ");
			foodSpawnRate.Value = world.FoodSpawnRate;

			builder.AddResizableButtonField("Edit Food", delegate(object sender)
			{
				EditEntityForm.ShowDialogue(container.Parent, world.FoodToClone);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			container.IsClosing += delegate(object sender)
			{
				world.MinimumFood = (int)minFood.Value;
				world.FoodSpawnRate = (int)foodSpawnRate.Value;
			};

			builder.BuildSessionEnd();

			return delegate()
			{
				return isReadyFunc();
			};
		}

		protected override void WriteInfo(System.IO.BinaryWriter w)
		{
			base.WriteInfo(w);

			WriteNullableObject(FoodToClone, w);
		}
		public override void Load(System.IO.BinaryReader r, uint id)
		{
			base.Load(r, id);

			FoodToClone = ReadNullableObject<Food>(r);
		}
	}
}
