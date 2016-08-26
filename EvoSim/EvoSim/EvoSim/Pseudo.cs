using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using EvoSim.RandomNumberGenerators;
using System.Xml;
using TakaGUI.IO;
using System.IO;

namespace EvoSim
{
	public static class Pseudo
	{
		public static UniformRandomGenerator Random
		{
			get
			{
				if (CurrentWorld_Random == null)
					return Static_Random;
				else
					return CurrentWorld_Random;
			}
		}
		public static NormalRandomGenerator GaussianRandom
		{
			get
			{
				if (CurrentWorld_GaussianRandom == null)
					return Static_GaussianRandom;
				else
					return CurrentWorld_GaussianRandom;
			}
		}

		public static UniformRandomGenerator CurrentWorld_Random;
		public static NormalRandomGenerator CurrentWorld_GaussianRandom;
		public static UniformRandomGenerator Static_Random;
		public static NormalRandomGenerator Static_GaussianRandom;
	}

	[DebuggerDisplay("Value={Value}, Min={Min}, Max={Max}")]
	public struct DoubleMinMax
	{
		double _Value;
		public double Value
		{
			get { return _Value; }
			set
			{
				_Value = value;

				if (Min != -1 && _Value < Min)
					_Value = Min;

				if (Max != -1 && _Value > Max)
					_Value = Max;
			}
		}

		double _Min;
		public double Min
		{
			get { return _Min; }
			set
			{
				_Min = value;

				if (_Min == -1)
					return;

				if (_Min > _Max)
					_Min = _Max;

				Value = Value;
			}
		}

		double _Max;
		public double Max
		{
			get { return _Max; }
			set
			{
				_Max = value;

				if (_Max == -1)
					return;

				if (_Max < _Min)
					_Max = _Min;

				Value = Value;
			}
		}

		public DoubleMinMax(double min, double max)
		{
			_Value = 0;
			_Min = 0;
			_Max = 0;

			Min = min;
			Max = max;
		}
		public DoubleMinMax(double min, double max, double value)
		{
			_Value = 0;
			_Min = 0;
			_Max = 0;

			Min = min;
			Max = max;
			Value = value;
		}
		public DoubleMinMax(DoubleMinMax val)
		{
			_Value = 0;
			_Min = 0;
			_Max = 0;

			Min = val.Min;
			Max = val.Max;
			Value = val.Value;
		}

		public void SetToMin()
		{
			Value = Min;
		}
		public void SetToMax()
		{
			Value = Max;
		}

		public void WriteTo(BinaryWriter w)
		{
			w.Write(Value);
			w.Write(Min);
			w.Write(Max);
		}
		public static DoubleMinMax Read(BinaryReader r)
		{
			var n = new DoubleMinMax();

			n._Value = r.ReadDouble();
			n._Min = r.ReadDouble();
			n._Max = r.ReadDouble();

			n.Value = n.Value;
			n.Min = n.Min;
			n.Max = n.Max;

			return n;
		}
	}

	[DebuggerDisplay("Value={Value}, Min={Min}, Max={Max}")]
	public struct IntMinMax
	{
		int _Value;
		public int Value
		{
			get { return _Value; }
			set
			{
				_Value = value;

				if (Min != -1 && _Value < Min)
					_Value = Min;

				if (Max != -1 && _Value > Max)
					_Value = Max;
			}
		}

		int _Min;
		public int Min
		{
			get { return _Min; }
			set
			{
				_Min = value;

				if (_Min == -1)
					return;

				if (_Min > _Max)
					_Min = _Max;

				Value = Value;
			}
		}

		int _Max;
		public int Max
		{
			get { return _Max; }
			set
			{
				_Max = value;

				if (_Max == -1)
					return;

				if (_Max < _Min)
					_Max = _Min;

				Value = Value;
			}
		}

		public IntMinMax(int min, int max)
		{
			_Value = 0;
			_Min = 0;
			_Max = 0;

			Min = min;
			Max = max;
		}

		public void SetToMin()
		{
			Value = Min;
		}
		public void SetToMax()
		{
			Value = Max;
		}

		public void WriteInXml(BinaryWriter w)
		{
			w.Write(Value);
			w.Write(Min);
			w.Write(Max);
		}

		public static IntMinMax LoadFromXmlTree(BinaryReader r)
		{
			var n = new IntMinMax();

			n._Value = r.ReadInt32();
			n._Min = r.ReadInt32();
			n._Max = r.ReadInt32();

			n.Value = n.Value;
			n.Min = n.Min;
			n.Max = n.Max;

			return n;
		}
	}
}
