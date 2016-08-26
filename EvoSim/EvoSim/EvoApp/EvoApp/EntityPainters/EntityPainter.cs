using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoSim;
using TakaGUI;
using Microsoft.Xna.Framework;
using TakaGUI.Services;
using TakaGUI.Data;
using Microsoft.Xna.Framework.Content;
using System.Collections.ObjectModel;

namespace EvoApp.EntityPainters
{
	public abstract class EntityPainter : BinarySerializable,  IEntityPainter
	{
		public string LoadCategory;

		public ReadOnlyCollection<Type> AllowedTypes { get; private set; }
		public List<Type> EntityTypeList
		{
			get;
			set;
		}
		public List<Type> ExcludedEntityTypeList
		{
			get;
			set;
		}

		public EntityPainter(string loadCategory)
			: base("EntityPainter")
		{
			AllowedTypes = new ReadOnlyCollection<Type>(GetAllowedTypes());

			LoadCategory = loadCategory;
			EntityTypeList = new List<Type>();
			ExcludedEntityTypeList = new List<Type>();
		}

		public void Initialize(ISkinFile skin)
		{
			Initialize(skin, LoadCategory);
		}
		public void Initialize(ISkinFile skin, string loadCategory)
		{
			LoadCategory = loadCategory;

			Init(skin);
		}
		protected virtual void Init(ISkinFile skin)
		{
		}

		protected abstract List<Type> GetAllowedTypes();

		protected ISprite GetSprite(ISkinFile skin, string name)
		{
			return skin.GetSprite(0, LoadCategory, name);
		}

		public virtual void Update(Microsoft.Xna.Framework.GameTime gameTime) { }

		public virtual void Draw(Microsoft.Xna.Framework.GameTime gameTime, IEntity entity, int x, int y, float scale, TakaGUI.Services.IRender render) { }

		protected override void WriteInfo(System.IO.BinaryWriter w)
		{
			base.WriteInfo(w);

			w.Write(LoadCategory);

			w.Write(EntityTypeList.Count);
			foreach (var t in EntityTypeList)
				w.Write(t.AssemblyQualifiedName);

			w.Write(ExcludedEntityTypeList.Count);
			foreach (var t in ExcludedEntityTypeList)
				w.Write(t.AssemblyQualifiedName);
		}

		public override void Load(System.IO.BinaryReader r, uint id)
		{
			base.Load(r, id);

			LoadCategory = r.ReadString();

			int length = r.ReadInt32();
			for (int i = 0; i < length; i++)
			{
				EntityTypeList.Add(Type.GetType(r.ReadString()));
			}

			length = r.ReadInt32();
			for (int i = 0; i < length; i++)
			{
				ExcludedEntityTypeList.Add(Type.GetType(r.ReadString()));
			}
		}

		public abstract IEntityPainter Clone();

		public virtual Func<bool> GUI_Edit(SingleSlotBox container)
		{
			return delegate()
			{
				return true;
			};
		}
	}
}
