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
using Microsoft.Xna.Framework.Graphics;

namespace EvoApp.EntityPainters
{
	public class AnimatedCreaturePainter : EntityPainter,  IEntityPainter
	{
		string[] directions = new string[] { "E", "SE", "S", "SW", "W", "NW", "N", "NE" };

		public AnimatedCreaturePainter()
			: base("Simulation/EntityPainters/AnimatedCreaturePainter")
		{
		}

		protected override List<Type> GetAllowedTypes()
		{
			return new List<Type>() { typeof(ICreature) };
		}

		public ISprite AnimationSheet;

		List<AnimationPack.Animation> animations = new List<AnimationPack.Animation>();

		Dictionary<uint, AnimationPack> animationPacks = new Dictionary<uint, AnimationPack>();
		protected override void Init(ISkinFile skin)
		{
			base.Init(skin);

			AnimationSheet = GetSprite(skin, "AnimationSheet");

			AnimationPack.AnimationTexture[,] textures = AnimationPack.Animation.GetAnimationTextureArray(AnimationSheet, new Size(32, 64), 0, 0);
			
			var anim = AnimationPack.Animation.GetAnimationFromArrayPartition("N", textures, new Rectangle(0, 0, 8, 1), 400);
			animations.Add(anim);

			anim = AnimationPack.Animation.GetAnimationFromArrayPartition("NE", textures, new Rectangle(0, 1, 8, 1), 400);
			animations.Add(anim);

			anim = AnimationPack.Animation.GetAnimationFromArrayPartition("E", textures, new Rectangle(0, 2, 8, 1), 400);
			animations.Add(anim);

			anim = AnimationPack.Animation.GetAnimationFromArrayPartition("SE", textures, new Rectangle(0, 3, 8, 1), 400);
			animations.Add(anim);

			anim = AnimationPack.Animation.GetAnimationFromArrayPartition("S", textures, new Rectangle(0, 4, 8, 1), 400);
			animations.Add(anim);

			anim = AnimationPack.Animation.GetAnimationFromArrayPartition("SW", textures, new Rectangle(0, 5, 8, 1), 400);
			animations.Add(anim);

			anim = AnimationPack.Animation.GetAnimationFromArrayPartition("W", textures, new Rectangle(0, 6, 8, 1), 400);
			animations.Add(anim);

			anim = AnimationPack.Animation.GetAnimationFromArrayPartition("NW", textures, new Rectangle(0, 7, 8, 1), 400);
			animations.Add(anim);
		}

		public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
		{
			foreach (var pack in animationPacks.Values)
				pack.Update(gameTime);
		}

		void setUpPack(IEntity entity)
		{
			var pack = new AnimationPack(animations);

			animationPacks.Add(entity.ID, pack);

			pack.ChangeAnimation(getDirection((float)entity.Rotation));
		}

		string getDirection(float rotation)
		{
			const double step = Math.PI / 4;

			return directions[(int)Math.Round(rotation - step / 2, 0)];
		}

		public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, IEntity entity, int x, int y, float scale, TakaGUI.Services.IRender render)
		{
			if (!animationPacks.ContainsKey(entity.ID))
				setUpPack(entity);

			var pack = animationPacks[entity.ID];

			pack.Draw(render, new Vector2(x, y) - new Vector2(pack.Width / 2, pack.Height / 2));
		}

		public override IEntityPainter Clone()
		{
			var clone = new DefaultEntityPainter();

			clone.Initialize(DrawBox.DefaultSkinFile);

			return clone;
		}
	}

	public class AnimationPack
	{
		public bool Paused;

		public int Width
		{
			get
			{
				if (CurrentAnimation == null)
					return -1;

				return CurrentAnimation.Width;
			}
		}
		public int Height
		{
			get
			{
				if (CurrentAnimation == null)
					return -1;

				return CurrentAnimation.Height;
			}
		}

		List<Animation> animations = new List<Animation>();
		public ReadOnlyCollection<Animation> Animations;

		Animation currentAnimation;
		public Animation CurrentAnimation
		{
			get { return currentAnimation; }
		}
		public string CurrentAnimationState
		{
			get { return CurrentAnimation.Name; }
		}

		double[] textureTimes;
		double[] textureTimeToEnd;

		double _CurrentTime;
		public double CurrentTime
		{
			get { return _CurrentTime; }
			set
			{
				_CurrentTime = value;

				if (_CurrentTime < 0)
					_CurrentTime = 0;
				else if (_CurrentTime > PlayTime)
					_CurrentTime = _CurrentTime - PlayTime;
			}
		}
		public double PlayTime
		{
			get
			{
				if (CurrentAnimation == null)
					return 0;

				return CurrentAnimation.PlayTime;
			}
		}

		public SpriteEffects SpriteEffect;

		public AnimationPack()
		{
			Animations = new ReadOnlyCollection<Animation>(animations);
		}
		public AnimationPack(List<Animation> _animations)
		{
			animations = _animations;
			Animations = new ReadOnlyCollection<Animation>(animations);
		}
		public AnimationPack(ISprite texture)
		{
			Animations = new ReadOnlyCollection<Animation>(animations);

			AddAnimation(Animation.GetSingleTextureAnimation("idle", texture));

			ChangeAnimation("idle");
		}

		public void AddAnimation(Animation anim)
		{
			anim.pack = this;
			animations.Add(anim);
		}

		void RecalculateTimes()
		{
			textureTimes = new double[currentAnimation.Textures.Length];
			textureTimeToEnd = new double[currentAnimation.Textures.Length];

			for (int n = 0; n < textureTimes.Length; n++)
				textureTimes[n] = currentAnimation.Textures[n].Time * PlayTime;

			for (int n = 0; n < textureTimes.Length; n++)
				textureTimeToEnd[n] = currentAnimation.Textures[n].EndTime * PlayTime;
		}

		public void ChangeAnimation(string name)
		{
			Animation animation = null;
			foreach (Animation elem in Animations)
				if (elem.Name == name)
					animation = elem;

			if (animation == null)
			{
				//Debug.Instance.AddExceptionInClass(this.GetType(), "ChangeAnimation", "Can't change to non-existing animation.");
				return;
			}

			currentAnimation = animation;
			RecalculateTimes();
		}

		public void Update(GameTime gameTime)
		{
			if (!Paused)
				CurrentTime += gameTime.ElapsedGameTime.TotalMilliseconds;
		}

		public void Draw(IRender render, Vector2 position)
		{
			render.AddViewRect(new ViewRect((int)Math.Round(position.X, 0), (int)Math.Round(position.Y, 0), Width, Height));

			if (currentAnimation == null)
				return;

			render.Begin();

			for (int n = 0; n < currentAnimation.Textures.Length; n++)
			{
				if (textureTimes[n] <= CurrentTime && textureTimeToEnd[n] >= CurrentTime)
				{
					Vector2 pos = position;
					if (SpriteEffect == SpriteEffects.FlipHorizontally)
						pos.X += currentAnimation.Width;
					if (SpriteEffect == SpriteEffects.FlipVertically)
						pos.Y += currentAnimation.Height;

					currentAnimation.Textures[n].Draw(render, position, SpriteEffect);
				}
			}

			render.End();
		}

		public struct AnimationTexture
		{
			public double Time
			{
				get;
				private set;
			}
			public double EndTime
			{
				get;
				private set;
			}

			public Rectangle SourceRect;

			public ISprite Texture;
			public Vector2 Position;

			public static AnimationTexture Empty
			{
				get;
				private set;
			}
			public bool IsEmpty
			{
				get;
				private set;
			}
			static AnimationTexture()
			{
				AnimationTexture empty = new AnimationTexture();
				empty.IsEmpty = true;
				Empty = empty;
			}

			public void Draw(IRender render, Vector2 origin, SpriteEffects spriteEffect)
			{
				Vector2 pos;
				if (spriteEffect == SpriteEffects.FlipHorizontally)
					pos.X = origin.X - Position.X;
				else
					pos.X = origin.X + Position.X;
				if (spriteEffect == SpriteEffects.FlipVertically)
					pos.Y = origin.Y - Position.Y;
				else
					pos.Y = origin.Y + Position.Y;

				render.DrawSprite(Texture, pos, SourceRect, new Vector2(0, 0), spriteEffect, Color.White, 0f);
			}

			public void SetTime(double time, double timeToEnd)
			{
				if (time < 0)
					time = 0;
				else if (time > 1)
					time = 1;

				Time = time;

				if (timeToEnd < 0)
					timeToEnd = 0;
				else if (timeToEnd < Time)
					timeToEnd = Time;

				EndTime = timeToEnd;
			}
		}

		public class Animation
		{
			internal AnimationPack pack;

			int _Width;
			public int Width
			{
				get { return _Width; }
				set
				{
					_Width = value;

					if (_Width < 1)
						_Width = 1;
				}
			}
			int _Height;
			public int Height
			{
				get { return _Height; }
				set
				{
					_Height = value;

					if (_Height < 1)
						_Height = 1;
				}
			}

			double _PlayTime;
			public double PlayTime //Miliseconds
			{
				get { return _PlayTime; }
				set
				{
					_PlayTime = value;

					if (_PlayTime < 0)
						_PlayTime = 0;

					if (pack.CurrentAnimation == this)
						pack.RecalculateTimes();
				}
			}

			public string Name;
			public AnimationTexture[] Textures;

			public Animation(string name, AnimationTexture[] textures)
			{
				Name = name;
				Textures = textures;
			}

			public static AnimationTexture[,] GetAnimationTextureArray(ISprite spriteSheet, Size size, int borderSizeX, int borderSizeY)
			{
				return GetAnimationTextureArray(spriteSheet, size, borderSizeX, borderSizeY, new Rectangle(0, 0, spriteSheet.Width, spriteSheet.Height));
			}
			public static AnimationTexture[,] GetAnimationTextureArray(ISprite spriteSheet, Size size, int borderSizeX, int borderSizeY, Rectangle source)
			{
				int w = (source.Width + borderSizeX) / size.Width;
				int h = (source.Height + borderSizeY) / size.Height;
				var textures = new AnimationTexture[w, h];

				int posX = source.X;
				int posY = source.Y;
				for (int x = 0; x < w; x++)
				{
					for (int y = 0; y < h; y++)
					{
						var t = new AnimationTexture();
						t.SourceRect = new Rectangle(posX, posY, size.Width, size.Height);
						t.Texture = spriteSheet;

						textures[x, y] = t;

						posY += size.Height + borderSizeY;
					}

					posX += size.Width + borderSizeX;
					posY = 0;
				}

				return textures;
			}

			public static Animation GetAnimationFromArrayPartition(string name, AnimationTexture[,] textures, Rectangle partition, double playTime)
			{
				List<AnimationTexture> textureList = new List<AnimationTexture>();

				int width = 0;
				int height = 0;
				for (int x = 0; x < textures.GetLength(0); x++)
				{
					for (int y = 0; y < textures.GetLength(1); y++)
					{
						if (x >= partition.Left && x < partition.Right &&
							y >= partition.Top && y < partition.Bottom)
						{
							textureList.Add(textures[x, y]);

							if (textures[x, y].Texture.Width > width)
								width = textures[x, y].SourceRect.Width;
							if (textures[x, y].Texture.Height > height)
								height = textures[x, y].SourceRect.Height;
						}
					}
				}

				double lastTime = 0;
				double time = (playTime / textureList.Count) / playTime;
				var copy = new List<AnimationTexture>(textureList);
				textureList.Clear();
				foreach (var t in copy)
				{
					t.SetTime(lastTime, lastTime += time);
					textureList.Add(t);
				}

				var anim = new Animation(name, textureList.ToArray());
				anim.Width = width;
				anim.Height = height;
				anim._PlayTime = playTime;

				return anim;
			}

			public static Animation GetSingleTextureAnimation(string name, ISprite texture)
			{
				AnimationTexture animationTexture = new AnimationTexture();

				animationTexture.SetTime(0, 1);
				animationTexture.Position = new Vector2(0, 0);
				animationTexture.SourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
				animationTexture.Texture = texture;

				var anim = new Animation(name, new AnimationTexture[] { animationTexture });
				anim._PlayTime = 1000;
				anim.Width = texture.Width;
				anim.Height = texture.Height;

				return anim;
			}
			public static Animation GetSingleTextureAnimation(string name, AnimationTexture animationTexture)
			{
				animationTexture.SetTime(0, 1);

				var anim = new Animation(name, new AnimationTexture[] { animationTexture });
				anim._PlayTime = 1000;
				anim.Width = animationTexture.SourceRect.Width;
				anim.Height = animationTexture.SourceRect.Height;

				return anim;
			}
		}
	}
}
