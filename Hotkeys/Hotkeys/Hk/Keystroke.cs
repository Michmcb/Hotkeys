using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows.Forms;

namespace Hotkeys.Hk
{
	public struct Keystroke : IEquatable<Keystroke>
	{
		public const uint MOD_NONE = 0x00;
		public const uint MOD_ALT = 0x01;
		public const uint MOD_CTRL = 0x02;
		public const uint MOD_SHIFT = 0x04;
		public const uint MOD_WIN = 0x08;
		public const uint MOD_NOREPEAT = 0x4000;
		public uint Vk { get; }
		public uint Modifiers { get; private set; }
		public bool HasAlt => (Modifiers & MOD_ALT) == MOD_ALT;
		public bool HasCtrl => (Modifiers & MOD_CTRL) == MOD_CTRL;
		public bool HasShift => (Modifiers & MOD_SHIFT) == MOD_SHIFT;
		public bool HasWin => (Modifiers & MOD_WIN) == MOD_WIN;
		public Keystroke(uint vk)
		{
			Vk = vk;
			Modifiers = MOD_NOREPEAT;
		}
		public Keystroke(uint vk, uint modifiers)
		{
			Vk = vk;
			Modifiers = modifiers;
		}
		public Keystroke(uint vk, bool ctrl, bool alt, bool shift, bool win)
		{
			Vk = vk;
			Modifiers = MOD_NOREPEAT;
			if (shift)
			{
				Modifiers |= MOD_SHIFT;
			}
			if (alt)
			{
				Modifiers |= MOD_ALT;
			}
			if (ctrl)
			{
				Modifiers |= MOD_CTRL;
			}
			if (win)
			{
				Modifiers |= MOD_WIN;
			}
		}
		public Keystroke(Keys key)
		{
			Modifiers = MOD_NOREPEAT;
			if ((key & Keys.Shift) == Keys.Shift)
			{
				Modifiers |= MOD_SHIFT;
			}
			if ((key & Keys.Alt) == Keys.Alt)
			{
				Modifiers |= MOD_ALT;
			}
			if ((key & Keys.Control) == Keys.Control)
			{
				Modifiers |= MOD_CTRL;
			}
			Vk = (uint)(key & Keys.KeyCode);
		}
		public override string ToString()
		{
			if (Modifiers != 0)
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
		public override bool Equals(object? obj)
		{
			return obj is Keystroke keystroke && Equals(keystroke);
		}
		public bool Equals([AllowNull] Keystroke other)
		{
			return Vk == other.Vk &&
				   Modifiers == other.Modifiers;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(Vk, Modifiers);
		}
		public static bool operator ==(Keystroke left, Keystroke right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(Keystroke left, Keystroke right)
		{
			return !(left == right);
		}
	}
}
