using System.Text;
using System.Windows.Forms;

namespace Hotkeys
{
	public struct Keystroke
	{
		public const uint MOD_NONE = 0x00;
		public const uint MOD_ALT = 0x01;
		public const uint MOD_CTRL = 0x02;
		public const uint MOD_SHIFT = 0x04;
		public const uint MOD_WIN = 0x08;
		public const uint MOD_NOREPEAT = 0x4000;

		private uint _modifiers;
		public uint Vk { get; }
		public uint Modifiers => _modifiers;
		public bool HasAlt => (Modifiers & MOD_ALT) == MOD_ALT;
		public bool HasCtrl => (Modifiers & MOD_CTRL) == MOD_CTRL;
		public bool HasShift => (Modifiers & MOD_SHIFT) == MOD_SHIFT;
		public bool HasWin => (Modifiers & MOD_WIN) == MOD_WIN;

		public Keystroke(uint vk)
		{
			Vk = vk;
			_modifiers = MOD_NOREPEAT;
		}
		public Keystroke(uint vk, uint modifiers)
		{
			Vk = vk;
			_modifiers = modifiers;
		}
		public Keystroke(Keys key)
		{
			_modifiers = MOD_NOREPEAT;
			if ((key & Keys.Shift) == Keys.Shift)
			{
				_modifiers |= MOD_SHIFT;
			}
			if ((key & Keys.Alt) == Keys.Alt)
			{
				_modifiers |= MOD_ALT;
			}
			if ((key & Keys.Control) == Keys.Control)
			{
				_modifiers |= MOD_CTRL;
			}
			Vk = (uint)(key & Keys.KeyCode);
		}
		public void NoRepeat(bool set)
		{
			if (set)
			{
				_modifiers |= MOD_NOREPEAT;
			}
			else
			{
				_modifiers &= ~MOD_NOREPEAT;
			}
		}
		public void Alt(bool set)
		{
			if (set)
			{
				_modifiers |= MOD_ALT;
			}
			else
			{
				_modifiers &= ~MOD_ALT;
			}
		}
		public void Ctrl(bool set)
		{
			if (set)
			{
				_modifiers |= MOD_CTRL;
			}
			else
			{
				_modifiers &= ~MOD_CTRL;
			}
		}
		public void Shift(bool set)
		{
			if (set)
			{
				_modifiers |= MOD_SHIFT;
			}
			else
			{
				_modifiers &= ~MOD_SHIFT;
			}
		}
		public void Win(bool set)
		{
			if (set)
			{
				_modifiers |= MOD_WIN;
			}
			else
			{
				_modifiers &= ~MOD_WIN;
			}
		}
		public override string ToString()
		{
			if (_modifiers != 0)
			{
				StringBuilder sb = new StringBuilder();
				if (HasAlt)
				{
					sb.Append("Alt");
				}
				if (HasCtrl)
				{
					sb.Append("Ctrl");
				}
				if (HasShift)
				{
					sb.Append("Shift");
				}
				if (HasWin)
				{
					sb.Append("Win");
				}
				sb.Append("+");
				sb.Append(((Keys)Vk).ToString());
				return sb.ToString();
			}
			return "";
		}
		public override bool Equals(object obj)
		{
			if (!(obj is Keystroke))
			{
				return false;
			}

			var keystroke = (Keystroke)obj;
			return _modifiers == keystroke._modifiers &&
				   Vk == keystroke.Vk;
		}
		public override int GetHashCode()
		{
			var hashCode = -1313466586;
			hashCode = hashCode * -1521134295 + _modifiers.GetHashCode();
			hashCode = hashCode * -1521134295 + Vk.GetHashCode();
			return hashCode;
		}
		public static bool operator ==(Keystroke keystroke1, Keystroke keystroke2)
		{
			return keystroke1.Equals(keystroke2);
		}
		public static bool operator !=(Keystroke keystroke1, Keystroke keystroke2)
		{
			return !(keystroke1 == keystroke2);
		}
	}
}
