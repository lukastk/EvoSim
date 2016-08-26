using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections.ObjectModel;
using System.IO;
using TakaGUI.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

namespace EvoSim
{
	public interface IBinarySerializable
	{
		uint ID { get; }
		/// <summary>
		/// The elementName of the target.
		/// </summary>
		string Name { get; set; }

		void Load(BinaryReader r, uint id);
		void Save(BinaryWriter w);
	}

	public abstract class BinarySerializable : IBinarySerializable
	{
		protected const byte IsNormal = 0;
		protected const byte IsIdRef = 1;
		protected const byte IsNull = 2;

		/// <summary>
		/// Identifies a single target, so that when inheriting it you can see where the target is originally from.
		/// </summary>
		public uint ID { get; private set; }
		/// <summary>
		/// The elementName of the target.
		/// </summary>
		public string Name { get; set; }

		private static uint nextId = 1;

		static Dictionary<string, int> idMap = new Dictionary<string, int>();

		public BinarySerializable(string name)
		{
			Name = name;
			ID = nextId;
			nextId++;
		}

		static Dictionary<uint, KeyValuePair<uint, uint>> idDict = new Dictionary<uint, KeyValuePair<uint, uint>>();
		static Dictionary<string, uint> stringDict = new Dictionary<string, uint>();

		static Dictionary<uint, IBinarySerializable> referenceDict = new Dictionary<uint, IBinarySerializable>();
		static Dictionary<uint, string> uintDict = new Dictionary<uint, string>();
		static Dictionary<string, bool> isLoadedInfo = new Dictionary<string, bool>();

		public virtual void Save(BinaryWriter w)
		{
			if (idDict.ContainsKey(ID))
			{
				w.Write(IsIdRef);
				w.Write(ID);
				return;
			}

			w.Write(IsNormal);

			uint type = GetIntFromString(this.GetType().FullName);
			uint assembly = GetIntFromString(this.GetType().Assembly.FullName);

			idDict.Add(ID, new KeyValuePair<uint,uint>(type, assembly));

			w.Write(ID);
			w.Write(GetIntFromString(Name));

			WriteInfo(w);
		}
		protected virtual void WriteInfo(BinaryWriter w)
		{
		}

		protected static uint GetIntFromString(string str)
		{
			if (stringDict.ContainsKey(str))
				return stringDict[str];

			uint key = (uint)stringDict.Values.Count;
			stringDict.Add(str, key);
			return key;
		}
		protected static string GetStringFromInt(uint num)
		{
			return uintDict[num];
		}

		public static T GetObject<T>(BinaryReader r)
			where T : IBinarySerializable
		{
			byte flag = r.ReadByte();
			if (flag == IsNull)
				return default(T);
			uint id = r.ReadUInt32();
			T obj = (T)referenceDict[id];

			if (flag == IsIdRef)
			{
				return obj;
			}
			else if (flag == IsNormal)
			{
				obj.Load(r, id);
				return obj;
			}
			else
				throw new FormatException();
		}

		public static void LoadIntoBuffer(BinaryReader r)
		{
			while (true)
			{
				if (r.BaseStream.Position == r.BaseStream.Length)
					break;

				string s = r.ReadString();

				if (s == "STOP")
					break;

				uint u = r.ReadUInt32();
				uintDict.Add(u, s);
			}

			while (true)
			{
				if (r.BaseStream.Position == r.BaseStream.Length)
					break;

				uint id = r.ReadUInt32();
				string typeName = GetStringFromInt(r.ReadUInt32());
				string assemblyName = GetStringFromInt(r.ReadUInt32());

				IBinarySerializable obj = (IBinarySerializable)Activator.CreateInstance(assemblyName, typeName).Unwrap();
				referenceDict.Add(id, obj);
			}
		}

		public virtual void Load(BinaryReader r, uint id)
		{
			ID = id;
			if (ID >= nextId)
				nextId = ID + 1;

			Name = GetStringFromInt(r.ReadUInt32());
		}

		public static void WriteSaveInfo(BinaryWriter w)
		{
			foreach (var key in stringDict)
			{
				w.Write(key.Key);
				w.Write(key.Value);
			}

			w.Write("STOP");

			foreach (var id in idDict.Keys)
			{
				w.Write(id);
				w.Write(idDict[id].Key);
				w.Write(idDict[id].Value);
			}
		}
		public static void ClearSaveBuffer()
		{
			idDict.Clear();
			stringDict.Clear();
		}
		public static void ClearLoadBuffer()
		{
			referenceDict.Clear();
			uintDict.Clear();
			isLoadedInfo.Clear();
		}

		public static void WriteListInfo(IList l, BinaryWriter w)
		{
			w.Write(l.Count);
			foreach (IBinarySerializable elem in l)
				elem.Save(w);
		}
		public static void LoadListInfo<T>(List<T> l, BinaryReader r)
			where T : IBinarySerializable
		{
			int length = r.ReadInt32();
			for (int i = 0; i < length; i++)
				l.Add(BinarySerializable.GetObject<T>(r));
		}
		public static T[] LoadArrayInfo<T>(BinaryReader r)
			where T : IBinarySerializable
		{
			var l = new List<T>();

			LoadListInfo(l, r);

			return l.ToArray();
		}

		public static void WriteNullableObject(IBinarySerializable obj, BinaryWriter w)
		{
			if (obj != null)
			{
				w.Write(BinarySerializable.IsNormal);
				obj.Save(w);
			}
			else
				w.Write(BinarySerializable.IsNull);
		}
		public static T ReadNullableObject<T>(BinaryReader r)
			where T : IBinarySerializable
		{
			byte isNullOrNormal = r.ReadByte();
			if (isNullOrNormal == BinarySerializable.IsNormal)
				return BinarySerializable.GetObject<T>(r);

			return default(T);
		}
	}
}
