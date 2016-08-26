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

namespace EvoApp.EntityPainters
{
	public class DefaultEntityPainter : EntityPainter,  IEntityPainter
	{
		public ISprite Texture;

		public DefaultEntityPainter()
			: base("Simulation/EntityPainters/DefaultEntityPainter")
		{
		}

		protected override List<Type> GetAllowedTypes()
		{
			return new List<Type>() { typeof(IEntity) };
		}

		protected override void Init(ISkinFile skin)
		{
			base.Init(skin);

			Texture = GetSprite(skin, "Texture");
		}

		public override void Update(Microsoft.Xna.Framework.GameTime gameTime) { }

		public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, IEntity entity, int x, int y, float scale, TakaGUI.Services.IRender render)
		{
			int radius = (int)Math.Round(entity.Radius * scale, 0);
			var destRect = new Rectangle(x, y, radius * 2, radius * 2);
			render.DrawSprite(Texture, destRect, new Vector2(Texture.Width / 2, Texture.Height / 2), entity.EntityColor, (float)entity.Rotation);
		}

		public override IEntityPainter Clone()
		{
			var clone = new DefaultEntityPainter();

			clone.Texture = Texture;

			return clone;
		}
	}
}
