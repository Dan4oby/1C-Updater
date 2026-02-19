public static class CommonUtils 
{
	public static void SetTitleStatus(string status)
	{
		if (string.IsNullOrEmpty(status)) Console.Title = "[SanCity] 1C обновления";
		else Console.Title = "[SanCity] - " + status;
	}

	public static void SetState(string header, string title)
	{
		header = header.ToUpper();
		header.Trim();
		SetTitleStatus(title);
		Console.Clear();
		Log.WithColor($"================={header}==================\n", ConsoleColor.Magenta);
		Log.SkipLine();
		Program.headerState = header;
		Program.titleState = title;
	}

	public static void UpdateState() {
		SetState(Program.headerState, Program.titleState);
	}

	public static void Pause()
	{
		Console.Write("Нажмите Enter для продолжения...");
		Console.ReadLine();
	}


	public static class Log {	
		public static void WithColor(string? message, ConsoleColor color)
		{
			ConsoleColor originalColor = Console.ForegroundColor;

			Console.ForegroundColor = color;
			Console.Write(message);
			Console.ForegroundColor = originalColor;
		}

		public static void Success(string? message) => WithColor(message + "\n", ConsoleColor.Green);

		public static void Error(string? message) => WithColor("[ERR] " + message + "\n", ConsoleColor.Red);

		public static void Warn(string? message) => WithColor("[WARN] " + message + "\n", ConsoleColor.Yellow);

		public static void SkipLine() => Console.WriteLine();

		public static void SkipLine(int n) {
			for (int i = 0; i < n; i++) SkipLine();
		}

		public static void Write(string? message) => Console.Write(message);

		public static void WriteLine(string? message) => Console.Write(message + "\n");
	}
}