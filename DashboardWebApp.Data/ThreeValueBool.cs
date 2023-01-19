using System;
using System.Data.SqlTypes;

namespace DashboardWebApp.DataTypes
{
	public struct ThreeValueBool
	{
		public ThreeValueBool(int initialValue)
		{
			this.m_value = (sbyte)initialValue;
		}

		public override string ToString()
		{
			switch (this.m_value)
			{
			case -1:
				return "False";
			case 0:
				return "";
			case 1:
				return "True";
			default:
				throw new InvalidOperationException();
			}
		}

		public bool IsNull
		{
			get
			{
				return this.m_value == 0;
			}
		}

		public bool IsFalse
		{
			get
			{
				return this.m_value < 0;
			}
		}

		public bool IsTrue
		{
			get
			{
				return this.m_value > 0;
			}
		}

		public static implicit operator ThreeValueBool(SqlBoolean x)
		{
			ThreeValueBool result;
			if (x.IsNull)
			{
				result = ThreeValueBool.Null;
			}
			else
			{
				result = x.Value;
			}
			return result;
		}

		public static implicit operator ThreeValueBool(bool x)
		{
			if (!x)
			{
				return ThreeValueBool.False;
			}
			return ThreeValueBool.True;
		}

		public static explicit operator bool(ThreeValueBool x)
		{
			if (x.m_value == 0)
			{
				throw new InvalidOperationException();
			}
			return x.m_value > 0;
		}

		public static ThreeValueBool operator ==(ThreeValueBool x, ThreeValueBool y)
		{
			if (x.m_value == 0 || y.m_value == 0)
			{
				return ThreeValueBool.Null;
			}
			if (x.m_value != y.m_value)
			{
				return ThreeValueBool.False;
			}
			return ThreeValueBool.True;
		}

		public static ThreeValueBool operator !=(ThreeValueBool x, ThreeValueBool y)
		{
			if (x.m_value == 0 || y.m_value == 0)
			{
				return ThreeValueBool.Null;
			}
			if (x.m_value == y.m_value)
			{
				return ThreeValueBool.False;
			}
			return ThreeValueBool.True;
		}

		public static ThreeValueBool operator !(ThreeValueBool x)
		{
			return new ThreeValueBool((int)(-(int)x.m_value));
		}

		public static ThreeValueBool operator &(ThreeValueBool x, ThreeValueBool y)
		{
			return new ThreeValueBool((int)((x.m_value < y.m_value) ? x.m_value : y.m_value));
		}

		public static ThreeValueBool operator |(ThreeValueBool x, ThreeValueBool y)
		{
			return new ThreeValueBool((int)((x.m_value > y.m_value) ? x.m_value : y.m_value));
		}

		public static bool operator true(ThreeValueBool x)
		{
			return x.m_value > 0;
		}

		public static bool operator false(ThreeValueBool x)
		{
			return x.m_value < 0;
		}

		public new static bool Equals(object value1, object value2)
		{
			ThreeValueBool x = (ThreeValueBool)value1;
			ThreeValueBool y = (ThreeValueBool)value2;
			return ThreeValueBool.Equals(x, y);
		}

		public override bool Equals(object o)
		{
			bool result;
			try
			{
				result = (bool)(this == (ThreeValueBool)o);
			}
			catch
			{
				result = false;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return (int)this.m_value;
		}

		public static ThreeValueBool Parse(string sourceString)
		{
			ThreeValueBool result = default(ThreeValueBool);
			if (sourceString == "True")
			{
				result = ThreeValueBool.True;
			}
			else if (sourceString == "False")
			{
				result = ThreeValueBool.False;
			}
			else
			{
				if (!(sourceString == ""))
				{
					throw new ArgumentException("Incorrect string format.", "sourceString");
				}
				result = ThreeValueBool.Null;
			}
			return result;
		}

		public static ThreeValueBool Parse(object source)
		{
			if (source != null && source != DBNull.Value)
			{
				return (bool)source;
			}
			return ThreeValueBool.Null;
		}


		public const string FalseString = "False";
		public const string TrueString = "True";
		public const string NullString = "";
		public static readonly ThreeValueBool Null = new ThreeValueBool(0);
		public static readonly ThreeValueBool False = new ThreeValueBool(-1);
		public static readonly ThreeValueBool True = new ThreeValueBool(1);
		private sbyte m_value;
	}
}
