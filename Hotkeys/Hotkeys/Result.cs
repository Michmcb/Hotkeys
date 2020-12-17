namespace Hotkeys
{
	public struct Result<O, E>
	{
		public O Ok { get; }
		public E Err { get; }
		public string Msg { get; }
		public Result(O ok, E err, string msg = "")
		{
			Ok = ok;
			Err = err;
			Msg = msg;
		}
	}
}
