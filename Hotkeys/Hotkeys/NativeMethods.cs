namespace Hotkeys
{
	using System;
	using System.Runtime.InteropServices;

	internal static partial class NativeMethods
	{
		[LibraryImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		[LibraryImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static partial bool UnregisterHotKey(IntPtr hWnd, int id);
	}
}
