using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using EvoSim.RandomNumberGenerators;
using System.Diagnostics;
using EvoSim.Genes;
using System.IO;
using EvoSim;
using TakaGUI;
using TakaGUI.DrawBoxes;
using EvoApp.Forms;

namespace EvoApp
{
	public class Creature : Entity, ICreature
	{
		public Genome CreatureGenome { get; set; }
		public INeuralNet Brain { get; set; }

		public int EyeNeuronsAmount { get; set; }
		public double EyeSpan  { get; set; }
		public int ViewDistance  { get; set; }
		//EntityColor
		public double Speed;
		public double RotationSpeed;
		DoubleMinMax energy = new DoubleMinMax(0, 0);
		public DoubleMinMax Energy
		{
			get { return energy; }
			set { energy = value; }
		}

		public bool CanSeeCreatures = true;

		public double EnergyLossByTick = 1;
		public double EnergyLossByRotation = 0.1;
		public double EnergyLossByMove = 0.1;
		public double EnergyLossByEating = 0.1;
		public double EnergyLossByMating = 60 * 30;
		public double EnergyLossByAttacking = 100;
		public double EatingBiteSize = 1000;

		public Creature MatingCandidate;
		public int CyclesInMating = 0;
		public int CyclesNeededForMating = 1;

		public int MatingCooldownCyclesLeft = 0;
		public int MatingCooldown = 60 * 10;
		public int AttackingCooldownCyclesLeft = 0;
		public int AttackingCooldown = 20;
		public double AttackMultiplier = 300;

		public int MaxAge = 60 * 60 * 4;

		#region Neurons
		public IInputNeuron[] EyeR_I;
		public IInputNeuron[] EyeG_I;
		public IInputNeuron[] EyeB_I;
		public IInputNeuron[] EyeDistance_I;

		protected IInputNeuron random_I;
		protected IInputNeuron clock_I;
		protected IInputNeuron energy_I;

		protected IOutputNeuron rotate_O;
		protected IOutputNeuron move_O;
		protected IOutputNeuron eat_O;
		protected IOutputNeuron mate_O;
		protected IOutputNeuron attack_O;

		#endregion

		public double PixelsRotatedLeft;
		public double PixelsRotatedRight;
		public double PixelsMovedForwards;
		public double PixelsMovedBackwards;
		public int TimesHaveEaten;
		public int TimesMated;

		public bool LoseEnergy = true;
		public bool IsThinking = true;
		public bool DisableMating = false;
		public bool DisableAttacking = false;

		public bool AlwaysMate = false;
		public bool AlwaysEat = true;

		public Creature()
			: base("Creature")
		{
			EyeSpan = Math.PI / 2;
			ViewDistance = 200;

			Speed = 3;
			RotationSpeed = (Math.PI * 2) / (60);
			energy.Max = 60 * 60 * 1;
			energy.SetToMax();
			Radius = 16;

			EntityColor = Color.Red;

			MatingCooldownCyclesLeft = MatingCooldown;
		}

		public virtual void Randomize()
		{
			CreatureGenome.BodyGenes.Clear();

			CreatureGenome.BodyGenes.Add(new DoubleGene("R", 0, 1, Pseudo.Random.NextDouble()));
			CreatureGenome.BodyGenes.Add(new DoubleGene("G", 0, 1, Pseudo.Random.NextDouble()));
			CreatureGenome.BodyGenes.Add(new DoubleGene("B", 0, 1, Pseudo.Random.NextDouble()));

			//CreatureGenome.CreateRandom(5, 3, 4, 20);

			int i = 0;
			CreatureGenome.NetChromosome.Outputs[i++].Name = "Rotate";
			CreatureGenome.NetChromosome.Outputs[i++].Name = "Move";
			CreatureGenome.NetChromosome.Outputs[i++].Name = "Eat";
			CreatureGenome.NetChromosome.Outputs[i++].Name = "Mate";

			//((DoubleGene)((DefaultSigmoid)((RMP_NeuronGene)CreatureGenome.NetChromosome.Outputs[0]).SigmoidFunction).GeneList[0]).Value = -1;
			//((DefaultSigmoid)((RMP_NeuronGene)CreatureGenome.NetChromosome.Outputs[0]).SigmoidFunction).GenesHasChanged();
			////((DoubleGene)((DefaultSigmoid)((RMP_NeuronGene)CreatureGenome.NetChromosome.Outputs[1]).SigmoidFunction).GeneList[0]).Value = -1;

			ImprintGenome(CreatureGenome);
			Energy.SetToMax();
		}

		public bool CheckMateValidity(ICreature parent2)
		{
			return true;

			//TODO: check if this is right
			if (parent2.CreatureGenome.ID == CreatureGenome.Parent1 ||
				parent2.CreatureGenome.ID == CreatureGenome.Parent2)
				return false;

			if (!parent2.CreatureGenome.IsFirstGeneration)
			{
				if (parent2.CreatureGenome.Parent1 == CreatureGenome.Parent1 ||
					parent2.CreatureGenome.Parent1 == CreatureGenome.Parent2 ||
					parent2.CreatureGenome.Parent1 == CreatureGenome.ID)
					return false;
			}

			if (!parent2.CreatureGenome.IsFirstGeneration)
			{
				if (parent2.CreatureGenome.Parent2 == CreatureGenome.Parent1 ||
					parent2.CreatureGenome.Parent2 == CreatureGenome.Parent2 ||
					parent2.CreatureGenome.Parent2 == CreatureGenome.ID)
					return false;
			}

			return true;
		}
		public ICreature CreateOffspring(ICreature parent2, double posX, double posY)
		{
			return CreateOffspring((ICreature)GetNewInstance(), parent2, posX, posY);
		}
		public virtual ICreature CreateOffspring(ICreature child, ICreature parent2, double posX, double posY)
		{
			var parent1 = this;
			child.World = parent1.World;
			child.ImprintGenome(parent1.CreatureGenome.GetChildGenome(parent2.CreatureGenome));
			child.Position = new EntityPosition(posX, posY);

			energy.Value /= 2;
			child.Energy = new DoubleMinMax(Energy);

			return child;
		}

		protected virtual void HandleInputNeurons()
		{
			HandleEyeInput();

			random_I.FireFrequency = Pseudo.Random.NextDouble(0, 1);

			//clock_I
			energy_I.FireFrequency = 1 - Energy.Value / Energy.Max;
		}
		protected virtual void HandleEyeInput()
		{
			const double tau = Math.PI * 2;

			double rotationStep = EyeSpan / EyeNeuronsAmount;
			double rotation = (Rotation + EyeSpan / 2);

			if (rotation < 0)
				rotation = tau + rotation;
			else if (rotation > tau)
				rotation = rotation - tau;

			var entityCollection = new List<IEntity>();
			int regionsToAdd = (int)Math.Ceiling((double)ViewDistance / World.RegionSize);
			int roofX = Math.Min(Region.X + regionsToAdd, World.RegionLengthX);
			int roofY = Math.Min(Region.Y + regionsToAdd, World.RegionLengthY);
			for (int rX = Math.Max(0, Region.X - regionsToAdd); rX < roofX; rX++)
			{
				for (int rY = Math.Max(0, Region.Y - regionsToAdd); rY < roofY; rY++)
				{
					entityCollection.AddRange(World.Regions[rX, rY]);
				}
			}

			for (int i = 0; i < EyeNeuronsAmount; i++)
			{
				double distance = -1;
				double r = -1;
				double g = -1;
				double b = -1;

				foreach (Entity entity in entityCollection)
				{
					if (entity == this)
						continue;

					if (!CanSeeCreatures && entity.EntityName == "Creature")
						continue;

					double dx = entity.Position.X - Position.X;
					double dy = entity.Position.Y - Position.Y;
					double entityDistance = Math.Sqrt(dx * dx + dy * dy);

					if (entityDistance == 0)
						continue;

					double angle = Math.Acos(dx / entityDistance);

					if (dy < 0)
						angle = tau - angle;

					double angleDifference = rotation - angle;

					if (angleDifference < 0)
						angleDifference = tau + angleDifference;

					if (angleDifference >= 0 &&
						angleDifference < rotationStep &&
						entityDistance < ViewDistance &&
						(distance == -1 || entityDistance < distance))
					{
						distance = entityDistance;
						r = entity.EntityColor.R / (double)255;
						g = entity.EntityColor.G / (double)255;
						b = entity.EntityColor.B / (double)255;
					}
				}

				int n = EyeNeuronsAmount - i - 1;

				if (distance != -1)
					EyeDistance_I[n].FireFrequency = 1 - distance / ViewDistance;
				else
					EyeDistance_I[n].FireFrequency = -1;
				EyeR_I[n].FireFrequency = Math.Max(0, r);
				EyeG_I[n].FireFrequency = Math.Max(0, g);
				EyeB_I[n].FireFrequency = Math.Max(0, b);

				rotation -= rotationStep;
				if (rotation < 0)
					rotation = tau + rotation;
				else if (rotation > tau)
					rotation = rotation - tau;
			}
		}

		public virtual void ImprintGenome(Genome genome)
		{
			CreatureGenome = genome;

			var entityColor = new Color();
			entityColor.R = Convert.ToByte((genome.BodyGenes[0].Value) * 255);
			entityColor.G = Convert.ToByte((genome.BodyGenes[1].Value) * 255);
			entityColor.B = Convert.ToByte((genome.BodyGenes[2].Value) * 255);
			entityColor.A = 255;
			EntityColor = entityColor;

			EyeNeuronsAmount = genome.NetChromosome.EyeNeuronsCount;

			Brain = genome.NetChromosome.GetNeuralNet();
			genome.NetChromosome.Mutate();
			SetUpNeurons();
		}
		void SetUpNeurons()
		{
			if (Brain == null)
				return;

			int i = 0;
			EyeR_I = GetListSliceAsArray(i, i += EyeNeuronsAmount, Brain.Inputs);
			EyeG_I = GetListSliceAsArray(i, i += EyeNeuronsAmount, Brain.Inputs);
			EyeB_I = GetListSliceAsArray(i, i += EyeNeuronsAmount, Brain.Inputs);
			EyeDistance_I = GetListSliceAsArray(i, i += EyeNeuronsAmount, Brain.Inputs);

			random_I = Brain.Inputs[i++];
			clock_I = Brain.Inputs[i++];
			energy_I = Brain.Inputs[i++];

			i = 0;
			rotate_O = Brain.Outputs[i++];
			move_O = Brain.Outputs[i++];
			eat_O = Brain.Outputs[i++];
			mate_O = Brain.Outputs[i++];
			attack_O = Brain.Outputs[i++];

			i = 0;
			//BodyGenes
		}

		public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
		{
			base.Update(gameTime);

			energy.Value -= EnergyLossByTick;

			HandleInputNeurons();

			if (IsThinking)
				Brain.Update();

			//Rotate
			double rotationDistance = rotate_O.Output * RotationSpeed;
			Rotation -= rotationDistance;
			energy.Value -= rotationDistance * EnergyLossByRotation;
			PixelsRotatedLeft += rotationDistance;

			//Move
			double moveDistance = move_O.Output * Speed;
			Move(moveDistance * Math.Cos(Rotation), moveDistance * Math.Sin(Rotation));
			energy.Value -= Math.Abs(moveDistance) * EnergyLossByMove;
			PixelsMovedForwards += moveDistance;

			//Eat
			if (AlwaysEat || (eat_O.Output > 0.5))
			{
				energy.Value -= EnergyLossByEating;

				foreach (Entity entity in World.EntityList)
				{
					if (entity == this)
						continue;

					var entityRect = new Rectangle((int)entity.Position.X - entity.Radius / 2, (int)entity.Position.Y - entity.Radius / 2, entity.Radius, entity.Radius);
					var thisRect = new Rectangle((int)Position.X - Radius / 2, (int)Position.Y - Radius / 2, Radius, Radius);

					if (entityRect.Intersects(thisRect) &&
						entity.EntityName == "Food")
					{
						Food food = (Food)entity;

						TimesHaveEaten += 1;

						if (food.EnergyStorage >= EatingBiteSize)
						{
							food.EnergyStorage -= EatingBiteSize;
							energy.Value += EatingBiteSize;
						}
						else
						{
							energy.Value += food.EnergyStorage;
							food.EnergyStorage = 0;
						}
					}
				}
			}

			//Mate
			if ((AlwaysMate || (!DisableMating && mate_O.Output > 0.5)) && MatingCooldownCyclesLeft == 0)
			{
				if (MatingCandidate != null)
				{
					var entityRect = new Rectangle((int)MatingCandidate.Position.X, (int)MatingCandidate.Position.Y, MatingCandidate.Radius, MatingCandidate.Radius);
					var thisRect = new Rectangle((int)Position.X, (int)Position.Y, Radius, Radius);

					if (entityRect.Intersects(thisRect))
						CyclesInMating += 1;

					if (CyclesInMating >= CyclesNeededForMating)
					{
						TimesMated += 1;

						energy.Value -= EnergyLossByMating;
						World.AddEntity(CreateOffspring(MatingCandidate, this.Position.X, this.Position.Y));
						MatingCandidate = null;
						CyclesInMating = 0;
						MatingCooldownCyclesLeft = MatingCooldown;
					}
				}
				else
				{
					CyclesInMating = 0;

					foreach (Entity entity in World.EntityList)
					{
						if (entity == this)
							continue;

						Rectangle entityRect = new Rectangle((int)entity.Position.X, (int)entity.Position.Y, entity.Radius, entity.Radius);
						Rectangle thisRect = new Rectangle((int)Position.X, (int)Position.Y, Radius, Radius);

						if (entity.EntityName == "Creature")
						{
							var c = (Creature)entity;

							if (CheckMateValidity(c) && entityRect.Intersects(thisRect) &&
								c.MatingCandidate != this)
							{
								MatingCandidate = c;
								break;
							}
						}
					}
				}
			}

			//Attack
			if ((!DisableAttacking && attack_O.Output > 0.5) && AttackingCooldownCyclesLeft == 0)
			{
				foreach (Entity entity in World.EntityList)
				{
					if (entity == this)
						continue;

					Rectangle entityRect = new Rectangle((int)entity.Position.X, (int)entity.Position.Y, entity.Radius, entity.Radius);
					Rectangle thisRect = new Rectangle((int)Position.X, (int)Position.Y, Radius, Radius);

					if (entity.EntityName == "Creature" && entityRect.Intersects(thisRect))
					{
						var c = (Creature)entity;
						
						c.energy.Value -= Math.Abs(AttackMultiplier * Speed * move_O.Output);
						AttackingCooldownCyclesLeft = AttackingCooldown;
					}
				}
			}

			if (MatingCooldownCyclesLeft != 0)
				MatingCooldownCyclesLeft -= 1;
			if (AttackingCooldownCyclesLeft != 0)
				AttackingCooldownCyclesLeft -= 1;

			if (!LoseEnergy)
				Energy.SetToMax();

			if (Energy.Value <= 1000 || Age >= MaxAge)
				Kill();
		}

		protected static double GetDoubleFromGene(DoubleGene gene, double max)
		{
			return GetDoubleFromGene(gene, 0, max);
		}
		protected static double GetDoubleFromGene(DoubleGene gene, double min, double max)
		{
			return min + (max - min) * gene.Value;
		}
		protected static int GetIntFromGene(DoubleGene gene, int max)
		{
			return GetIntFromGene(gene, 0, max);
		}
		protected static int GetIntFromGene(DoubleGene gene, int min, int max)
		{
			return min + (int)Math.Round((max - min) * gene.Value, 0);
		}
		protected static T[] GetListSliceAsArray<T>(int from, int to, ICollection<T> l)
		{
			return l.Skip(from).Take(to - from).Cast<T>().ToArray();
		}

		protected override void WriteInfo(BinaryWriter w)
		{
			base.WriteInfo(w);

			WriteNullableObject(Brain, w);
			WriteNullableObject(CreatureGenome, w);

			w.Write(EyeNeuronsAmount);
			w.Write(EyeSpan);
			w.Write(ViewDistance);
			w.Write(Speed);
			w.Write(RotationSpeed);
			Energy.WriteTo(w);

			w.Write(CanSeeCreatures);

			w.Write(EnergyLossByTick);
			w.Write(EnergyLossByRotation);
			w.Write(EnergyLossByMove);
			w.Write(EnergyLossByEating);
			w.Write(EnergyLossByMating);
			w.Write(EatingBiteSize);

			if (MatingCandidate != null)
				MatingCandidate.Save(w);
			else
				w.Write(IsNull);
			w.Write(CyclesInMating);
			w.Write(CyclesNeededForMating);

			w.Write(MatingCooldownCyclesLeft);
			w.Write(MatingCooldown);
			w.Write(AttackingCooldownCyclesLeft);
			w.Write(AttackingCooldown);
			w.Write(AttackMultiplier);

			w.Write(MaxAge);

			w.Write(PixelsRotatedLeft);
			w.Write(PixelsRotatedRight);
			w.Write(PixelsMovedForwards);
			w.Write(PixelsMovedBackwards);
			w.Write(TimesHaveEaten);
			w.Write(TimesMated);

			w.Write(LoseEnergy);
			w.Write(IsThinking);
			w.Write(DisableMating);
			w.Write(DisableAttacking);

			w.Write(AlwaysMate);
			w.Write(AlwaysEat);

			if (Brain != null)
			{
				WriteListInfo(EyeR_I, w);
				WriteListInfo(EyeG_I, w);
				WriteListInfo(EyeB_I, w);
				WriteListInfo(EyeDistance_I, w);

				random_I.Save(w);
				clock_I.Save(w);
				energy_I.Save(w);

				rotate_O.Save(w);
				move_O.Save(w);
				eat_O.Save(w);
				mate_O.Save(w);
				attack_O.Save(w);
			}
		}

		public override void Load(BinaryReader r, uint id)
		{
			base.Load(r, id);

			Brain = ReadNullableObject<INeuralNet>(r);
			CreatureGenome = ReadNullableObject<Genome>(r);

			EyeNeuronsAmount = r.ReadInt32();
			EyeSpan = r.ReadDouble();
			ViewDistance = r.ReadInt32();
			Speed = r.ReadDouble();
			RotationSpeed = r.ReadDouble();
			Energy = DoubleMinMax.Read(r);

			CanSeeCreatures = r.ReadBoolean();

			EnergyLossByTick = r.ReadDouble();
			EnergyLossByRotation = r.ReadDouble();
			EnergyLossByMove = r.ReadDouble();
			EnergyLossByEating = r.ReadDouble();
			EnergyLossByMating = r.ReadDouble();
			EatingBiteSize = r.ReadDouble();

			MatingCandidate = BinarySerializable.GetObject<Creature>(r);
			CyclesInMating = r.ReadInt32();
			CyclesNeededForMating = r.ReadInt32();

			MatingCooldownCyclesLeft = r.ReadInt32();
			MatingCooldown = r.ReadInt32();
			AttackingCooldownCyclesLeft = r.ReadInt32();
			AttackingCooldown = r.ReadInt32();
			AttackMultiplier = r.ReadDouble();

			MaxAge = r.ReadInt32();

			PixelsRotatedLeft = r.ReadDouble();
			PixelsRotatedRight = r.ReadDouble();
			PixelsMovedForwards = r.ReadDouble();
			PixelsMovedBackwards = r.ReadDouble();
			TimesHaveEaten = r.ReadInt32();
			TimesMated = r.ReadInt32();

			LoseEnergy = r.ReadBoolean();
			IsThinking = r.ReadBoolean();
			DisableMating = r.ReadBoolean();
			DisableAttacking = r.ReadBoolean();

			AlwaysMate = r.ReadBoolean();
			AlwaysEat = r.ReadBoolean();

			if (Brain != null)
			{
				EyeR_I = LoadArrayInfo<IInputNeuron>(r);
				EyeG_I = LoadArrayInfo<IInputNeuron>(r);
				EyeB_I = LoadArrayInfo<IInputNeuron>(r);
				EyeDistance_I = LoadArrayInfo<IInputNeuron>(r);

				random_I = BinarySerializable.GetObject<IInputNeuron>(r);
				clock_I = BinarySerializable.GetObject<IInputNeuron>(r);
				energy_I = BinarySerializable.GetObject<IInputNeuron>(r);

				rotate_O = BinarySerializable.GetObject<IOutputNeuron>(r);
				move_O = BinarySerializable.GetObject<IOutputNeuron>(r);
				eat_O = BinarySerializable.GetObject<IOutputNeuron>(r);
				mate_O = BinarySerializable.GetObject<IOutputNeuron>(r);
				attack_O = BinarySerializable.GetObject<IOutputNeuron>(r);
			}
		}

		protected override void GUI_Edit_SetColumnListBox(ColumnListBox listBox)
		{
			base.GUI_Edit_SetColumnListBox(listBox);

			listBox.AddRow("EyeNeuronsAmount", EyeNeuronsAmount);
			listBox.AddRow("EyeSpan", EyeSpan);
			listBox.AddRow("ViewDistance", ViewDistance);
			listBox.AddRow("Speed", Speed);
			listBox.AddRow("RotationSpeed", RotationSpeed);
			listBox.AddRow("EnergyMin", Energy.Min);
			listBox.AddRow("EnergyMax", Energy.Max);
			listBox.AddRow("EnergyValue", Energy.Value);

			listBox.AddRow("CanSeeCreatures", CanSeeCreatures);

			listBox.AddRow("EnergyLossByTick", EnergyLossByTick);
			listBox.AddRow("EnergyLossByRotation", EnergyLossByRotation);
			listBox.AddRow("EnergyLossByMove", EnergyLossByMove);
			listBox.AddRow("EnergyLossByEating", EnergyLossByEating);
			listBox.AddRow("EnergyLossByMating", EnergyLossByMating);
			listBox.AddRow("EatingBiteSize", EatingBiteSize);

			listBox.AddRow("CyclesNeededForMating", CyclesNeededForMating);
			listBox.AddRow("MatingCooldown", MatingCooldown);
			listBox.AddRow("AttackingCooldownCyclesLeft", AttackingCooldownCyclesLeft);
			listBox.AddRow("AttackingCooldown", AttackingCooldown);
			listBox.AddRow("AttackMultiplier", AttackMultiplier);

			listBox.AddRow("MaxAge", MaxAge);

			//listBox.AddRow("PixelsRotatedLeft", PixelsRotatedLeft);
			//listBox.AddRow("PixelsRotatedRight", PixelsRotatedRight);
			//listBox.AddRow("PixelsMovedForwards", PixelsMovedForwards);
			//listBox.AddRow("PixelsMovedBackwards", PixelsMovedBackwards);
			//listBox.AddRow("TimesHaveEaten", TimesHaveEaten);
			//listBox.AddRow("TimesMated", TimesMated);

			listBox.AddRow("LoseEnergy", LoseEnergy);
			listBox.AddRow("IsThinking", IsThinking);
			listBox.AddRow("DisableMating", DisableMating);
			listBox.AddRow("DisableAttacking", DisableAttacking);

			listBox.AddRow("AlwaysMate", AlwaysMate);
			listBox.AddRow("AlwaysEat", AlwaysEat);
		}
		protected override void GUI_Edit_SetValues(ColumnListBox listBox)
		{
			base.GUI_Edit_SetValues(listBox);

			Dictionary<string, object> values = new Dictionary<string, object>();
			foreach (var row in listBox.Values)
				values.Add((string)row.Values[0], row.Values[1]);

			EyeNeuronsAmount = (int)values["EyeNeuronsAmount"];
			EyeSpan = (double)values["EyeSpan"];
			ViewDistance = (int)values["ViewDistance"];
			Speed = (double)values["Speed"];
			RotationSpeed = (double)values["RotationSpeed"];
			energy.Min = (double)values["EnergyMin"];
			energy.Max = (double)values["EnergyMax"];
			energy.Value = (double)values["EnergyValue"];

			CanSeeCreatures = (bool)values["CanSeeCreatures"];

			EnergyLossByTick = (double)values["EnergyLossByTick"];
			EnergyLossByRotation = (double)values["EnergyLossByRotation"];
			EnergyLossByMove = (double)values["EnergyLossByMove"];
			EnergyLossByEating = (double)values["EnergyLossByEating"];
			EnergyLossByMating = (double)values["EnergyLossByMating"];
			EatingBiteSize = (double)values["EatingBiteSize"];

			CyclesNeededForMating = (int)values["CyclesNeededForMating"];
			MatingCooldown = (int)values["MatingCooldown"];
			AttackingCooldownCyclesLeft = (int)values["AttackingCooldownCyclesLeft"];
			AttackingCooldown = (int)values["AttackingCooldown"];
			AttackMultiplier = (double)values["AttackMultiplier"];

			MaxAge = (int)values["MaxAge"];

			LoseEnergy = (bool)values["LoseEnergy"];
			IsThinking = (bool)values["IsThinking"];
			DisableMating = (bool)values["DisableMating"];
			DisableAttacking = (bool)values["DisableAttacking"];

			AlwaysMate = (bool)values["AlwaysMate"];
			AlwaysEat = (bool)values["AlwaysEat"];
		}

		public override IEntity GetNewInstance()
		{
			return new Creature();
		}
		protected override void CloneValues(IEntity entity)
		{
			base.CloneValues(entity);

			var clone = (Creature)entity;

			if (CreatureGenome != null)
				clone.CreatureGenome = CreatureGenome.Clone();

			if (clone.CreatureGenome != null && clone.CreatureGenome.NetChromosome != null)
				clone.Brain = clone.CreatureGenome.NetChromosome.GetNeuralNet();

			clone.EyeNeuronsAmount = EyeNeuronsAmount;
			clone.EyeSpan = EyeSpan;
			clone.ViewDistance = ViewDistance;
			clone.Speed = Speed;
			clone.RotationSpeed = RotationSpeed;
			clone.Energy = Energy;
			clone.CanSeeCreatures = CanSeeCreatures;

			clone.EnergyLossByTick = EnergyLossByTick;
			clone.EnergyLossByRotation = EnergyLossByRotation;
			clone.EnergyLossByMove = EnergyLossByMove;
			clone.EnergyLossByEating = EnergyLossByEating;
			clone.EnergyLossByMating = EnergyLossByMating;
			clone.EatingBiteSize = EatingBiteSize;

			clone.MatingCandidate = MatingCandidate;
			clone.CyclesInMating = CyclesInMating;
			clone.CyclesNeededForMating = CyclesNeededForMating;

			clone.MatingCooldownCyclesLeft = MatingCooldownCyclesLeft;
			clone.MatingCooldown = MatingCooldown;
			clone.AttackingCooldownCyclesLeft = AttackingCooldownCyclesLeft;
			clone.AttackingCooldown = AttackingCooldown;
			clone.AttackMultiplier = AttackMultiplier;

			clone.MaxAge = MaxAge;

			clone.PixelsRotatedLeft = PixelsRotatedLeft;
			clone.PixelsRotatedRight = PixelsRotatedRight;
			clone.PixelsMovedForwards = PixelsMovedForwards;
			clone.PixelsMovedBackwards = PixelsMovedBackwards;
			clone.TimesHaveEaten = TimesHaveEaten;
			clone.TimesMated = TimesMated;

			clone.LoseEnergy = LoseEnergy;
			clone.IsThinking = IsThinking;
			clone.DisableMating = DisableMating;
			clone.DisableAttacking = DisableAttacking;

			clone.AlwaysMate = AlwaysMate;
			clone.AlwaysEat = AlwaysEat;

			clone.SetUpNeurons();
		}
	}
}
