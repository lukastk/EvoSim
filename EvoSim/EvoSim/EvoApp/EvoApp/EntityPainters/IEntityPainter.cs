using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoSim;
using Microsoft.Xna.Framework;
using TakaGUI.Services;
using TakaGUI.Data;
using TakaGUI;
using System.Collections.ObjectModel;

namespace EvoApp.EntityPainters
{
	public interface IEntityPainter : IBinarySerializable
	{
		ReadOnlyCollection<Type> AllowedTypes { get; }
		List<Type> EntityTypeList { get; set; }
		List<Type> ExcludedEntityTypeList { get; set; }

		void Initialize(ISkinFile skin);
		void Initialize(ISkinFile skin, string loadCategory);

		void Update(GameTime gameTime);
		void Draw(GameTime gameTime, IEntity entity, int x, int y, float scale, IRender render);

		IEntityPainter Clone();

		Func<bool> GUI_Edit(SingleSlotBox container);

		//static List<Type> AllowedTypes
	}
}
