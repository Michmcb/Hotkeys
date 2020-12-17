namespace Hotkeys
{
	using System;
	using System.Runtime.InteropServices;

	internal static class NativeMethods
	{
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
	}
}
