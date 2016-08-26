using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using System.IO;
using System.Xml;
using EvoSim.RandomNumberGenerators;

namespace EvoSim
{
	/// <summary>
	/// Represents the world, and it contains all the entities within the world.
	/// </summary>
	public interface IWorld : IBinarySerializable
	{
		/// <summary>
		/// Generates pseudo-random numbers.
		/// </summary>
		UniformRandomGenerator Random { get; set;}
		/// <summary>
		/// Generates gaussian random numbers.
		/// </summary>
		NormalRandomGenerator GaussianRandom { get; set;}

		/// <summary>
		/// The width of the simulation map in pixels.
		/// </summary>
		int Width { get; set;}
		/// <summary>
		/// The height of the simulation map in pixels.
		/// </summary>
		int Height { get; set;}
		/// <summary>
		/// If false, an entity that moves over a border will appear on the other side of the map.
		/// If true, the entity won't be able to move past the map-borders.
		/// </summary>
		bool WorldHasEdges { get; set;}

		/// <summary>
		/// Exclusively holds the entities that are of the Creature class.
		/// </summary>
		ReadOnlyCollection<ICreature> CreatureList { get;}
		/// <summary>
		/// Holds all entities within the world.
		/// </summary>
		ReadOnlyCollection<IEntity> EntityList { get;}

		List<SpawnPoint> SpawnPointList { get; }

		/// <summary>
		/// An array of entities within the World-regions.
		/// </summary>
		List<IEntity>[,] Regions { get; }
		/// <summary>
		/// The number of regions on the x-axis.
		/// </summary>
		int RegionLengthX { get; }
		/// <summary>
		/// The number of regions on the y-axis.
		/// </summary>
		int RegionLengthY { get; }
		/// <summary>
		/// The width and height of a region in pixels.
		/// </summary>
		int RegionSize { get; }

		/// <summary>
		/// Adds a new entity to the world.
		/// </summary>
		/// <param name="entity"></param>
		void AddEntity(IEntity entity);
		/// <summary>
		/// Removes an entity from the world.
		/// </summary>
		/// <param name="entity"></param>
		void RemoveEntity(IEntity entity);

		/// <summary>
		/// Removes all entities from the world.
		/// </summary>
		void RemoveAllEntities();

		void ReloadRegions();

		void Update(GameTime gameTime);

		void Work(GameTime gameTime);

		IWorld Clone();
	}
}
