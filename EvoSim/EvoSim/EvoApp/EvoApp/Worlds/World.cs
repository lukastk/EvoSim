using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using System.IO;
using System.Xml;
using EvoSim.RandomNumberGenerators;
using EvoSim;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;

namespace EvoApp.Worlds
{
	/// <summary>
	/// Represents the world, and it contains all the entities within the world.
	/// </summary>
	public class World : BinarySerializable, IWorld
	{
		/// <summary>
		/// Generates pseudo-random numbers.
		/// </summary>
		public UniformRandomGenerator Random { get; set; }
		/// <summary>
		/// Generates gaussian random numbers.
		/// </summary>
		public NormalRandomGenerator GaussianRandom { get; set; }

		int width = 300;
		/// <summary>
		/// The width of the simulation map in pixels.
		/// </summary>
		public int Width
		{
			get { return width; }
			set { width = value; }
		}
		/// <summary>
		/// The height of the simulation map in pixels.
		/// </summary>
		int height = 300;
		public int Height
		{
			get { return height; }
			set { height = value; }
		}
		/// <summary>
		/// If false, an entity that moves over a border will appear on the other side of the map.
		/// If true, the entity won't be able to move past the map-borders.
		/// </summary>
		public bool WorldHasEdges
		{
			get;
			set;
		}

		List<ICreature> creatureList = new List<ICreature>();
		List<IEntity> entityList = new List<IEntity>();
		/// <summary>
		/// Exclusively holds the entities that are of the Creature class.
		/// </summary>
		public ReadOnlyCollection<ICreature> CreatureList
		{
			get;
			protected set;
		}
		/// <summary>
		/// Holds all entities within the world.
		/// </summary>
		public ReadOnlyCollection<IEntity> EntityList
		{
			get;
			protected set;
		}

		public List<SpawnPoint> SpawnPointList { get; private set; }

		/// <summary>
		/// An array of entities within the World-regions.
		/// </summary>
		public List<IEntity>[,] Regions { get; private set; }
		/// <summary>
		/// The number of regions on the x-axis.
		/// </summary>
		public int RegionLengthX { get; private set; }
		/// <summary>
		/// The number of regions on the y-axis.
		/// </summary>
		public int RegionLengthY { get; private set; }
		/// <summary>
		/// The width and height of a region in pixels.
		/// </summary>
		public int RegionSize { get; private set; }

		public World()
			: base("World")
		{
			SpawnPointList = new List<SpawnPoint>();
			CreatureList = creatureList.AsReadOnly();
			EntityList = entityList.AsReadOnly();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="regionSize"></param>
		/// <param name="gaussianMin">Used to create a new NormalRandomGenerator instance.</param>
		/// <param name="gaussianMax">Used to create a new NormalRandomGenerator instance.</param>
		public World(int regionSize, int gaussianMin, int gaussianMax)
			: base("World")
		{
			SpawnPointList = new List<SpawnPoint>();
			Initialize(regionSize, gaussianMin, gaussianMax);
		}
		public World(int regionSize, UniformRandomGenerator random, NormalRandomGenerator gaussianRandom)
			: base("World")
		{
			SpawnPointList = new List<SpawnPoint>();
			Initialize(regionSize, random, gaussianRandom);
		}

		void Initialize(int regionSize, int gaussianMin, int gaussianMax, bool loading = false)
		{
			CreatureList = creatureList.AsReadOnly();
			EntityList = entityList.AsReadOnly();

			CreateRegions(regionSize, loading);

			Random = new UniformRandomGenerator();
			GaussianRandom = new NormalRandomGenerator(gaussianMin, gaussianMax, Random);
		}
		void Initialize(int regionSize, UniformRandomGenerator random, NormalRandomGenerator gaussianRandom, bool loading = false)
		{
			CreatureList = creatureList.AsReadOnly();
			EntityList = entityList.AsReadOnly();

			CreateRegions(regionSize, loading);

			Random = random;
			GaussianRandom = gaussianRandom;
		}

		/// <summary>
		/// Creates new regions based on the given size.
		/// </summary>
		/// <param name="regionSize"></param>
		public void CreateRegions(int regionSize)
		{
			CreateRegions(regionSize, false);
		}
		/// <summary>
		/// Creates new regions based on the given size.
		/// </summary>
		/// <param name="regionSize"></param>
		/// <param name="loading">If true, the CheckRegion method of the entities won't be called. Used when loading a world from a file.</param>
		protected void CreateRegions(int regionSize, bool loading)
		{
			RegionSize = regionSize;

			ReloadRegions(loading);
		}
		/// <summary>
		/// Reloads the regions.
		/// </summary>
		public void ReloadRegions()
		{
			ReloadRegions(false);
		}
		/// <summary>
		/// Reloads the regions.
		/// </summary>
		/// <param name="loading">If true, the CheckRegion method of the entities won't be called. Used when loading a world from a file.</param>
		protected void ReloadRegions(bool loading)
		{
			Regions = new List<IEntity>[(int)Math.Ceiling((double)Width / RegionSize), (int)Math.Ceiling((double)Height / RegionSize)];

			RegionLengthX = Regions.GetLength(0);
			RegionLengthY = Regions.GetLength(1);

			for (int x = 0; x < RegionLengthX; x++)
				for (int y = 0; y < RegionLengthY; y++)
					Regions[x, y] = new List<IEntity>();

			if (!loading)
				foreach (var entity in EntityList)
					entity.CheckRegion();
		}

		/// <summary>
		/// Adds a new entity to the world.
		/// </summary>
		/// <param name="entity"></param>
		public void AddEntity(IEntity entity)
		{
			entityList.Add(entity);
			entity.World = this;

			if (typeof(ICreature).IsAssignableFrom(entity.GetType()))
				creatureList.Add((ICreature)entity);

			entity.Position = entity.Position;
		}
		/// <summary>
		/// Removes an entity from the world.
		/// </summary>
		/// <param name="entity"></param>
		public void RemoveEntity(IEntity entity)
		{
			entityList.Remove(entity);

			if (entity.GetType().IsInstanceOfType(typeof(ICreature)))
				creatureList.Remove((ICreature)entity);

			entity.DeleteFromRegion();
		}

		/// <summary>
		/// Removes all entities from the world.
		/// </summary>
		public void RemoveAllEntities()
		{
			entityList.Clear();
			creatureList.Clear();
		}

		public virtual void Update(GameTime gameTime)
		{
			Pseudo.CurrentWorld_Random = Random;
			Pseudo.CurrentWorld_GaussianRandom = GaussianRandom;

			foreach (IEntity entity in new List<IEntity>(entityList))
				entity.Update(gameTime);

			foreach (IEntity entity in new List<IEntity>(entityList))
				if (entity.IsDead)
					RemoveEntity(entity);

            foreach (SpawnPoint sp in SpawnPointList)
                sp.Update(gameTime);

			Work(gameTime);

			Pseudo.CurrentWorld_Random = null;
			Pseudo.CurrentWorld_GaussianRandom = null;
		}

		public virtual void Work(GameTime gameTime)
		{
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			Random.Save(w);
			GaussianRandom.Save(w);

			w.Write(Width);
			w.Write(Height);
			w.Write(WorldHasEdges);
			w.Write(RegionSize);

			WriteListInfo(EntityList, w);
		}

		protected virtual IWorld GetNewInstance()
		{
			return new World();
		}
		public IWorld Clone()
		{
			var clone = GetNewInstance();

			CloneValues(clone);

			return clone;
		}
		protected virtual void CloneValues(IWorld world)
		{
			var clone = (World)world;

			clone.Random = new UniformRandomGenerator();
			clone.GaussianRandom = new NormalRandomGenerator(GaussianRandom.Min, GaussianRandom.Max, clone.Random);

			clone.Width = width;
			clone.Height = Height;
			clone.WorldHasEdges = WorldHasEdges;

			clone.CreateRegions(RegionSize);
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			Random = BinarySerializable.GetObject<UniformRandomGenerator>(r);
			GaussianRandom = BinarySerializable.GetObject<NormalRandomGenerator>(r);

			Width = r.ReadInt32();
			Height = r.ReadInt32();
			WorldHasEdges = r.ReadBoolean();
			RegionSize = r.ReadInt32();

			LoadListInfo(entityList, r);

			foreach (var entity in entityList)
			{
				if (entity.GetType().IsSubclassOf(typeof(ICreature)) || entity.GetType().IsEquivalentTo(typeof(ICreature)))
					creatureList.Add((ICreature)entity);
			}

			Initialize(RegionSize, Random, GaussianRandom, true);
		}

		public static Func<bool> GUI_Edit(SingleSlotBox container, World world)
		{
			var builder = new FieldBuilder();

			builder.BuildSessionStart(container);

			var width = builder.AddIntegerField("World Width: ");
			width.MinValue = 1;
			width.Value = world.Width;

			var height = builder.AddIntegerField("World Height: ");
			height.MinValue = 1;
			height.Value = world.Height;

			var worldHasEdges = builder.AddCheckBoxField("World Edges");
			worldHasEdges.Checked = world.WorldHasEdges;

			var regionSize = builder.AddIntegerField("Region Size: ");
			regionSize.MinValue = 1;
			regionSize.Value = world.RegionSize;

			var gaussianMin = builder.AddIntegerField("Gassian Generator Min: ");
			gaussianMin.Value = 0;

			var gaussianMax = builder.AddIntegerField("Gassian Generator Max: ");
			gaussianMax.Value = 1000;

			container.IsClosing += delegate(object sender)
			{
				world.Width = (int)width.Value;
				world.Height = (int)height.Value;
				world.WorldHasEdges = worldHasEdges.Checked;
				world.RegionSize = (int)regionSize.Value;

				world.Random = new UniformRandomGenerator();
				world.GaussianRandom = new NormalRandomGenerator((int)gaussianMin.Value, (int)gaussianMax.Value, world.Random);

				foreach (var e in world.EntityList)
					e.Position = new EntityPosition(Math.Min(e.Position.X, world.Width - 1), Math.Min(e.Position.Y, world.Height - 1));

				world.ReloadRegions();
			};

			builder.BuildSessionEnd();

			return delegate()
			{
				return true;
			};
		}
	}
}
