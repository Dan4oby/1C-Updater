using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Text.RegularExpressions;

public static class CommonUtils 
{

	public static void PlayLookAtMeSound()
	{
		var signalGenerator = new SignalGenerator(44100, 1);
		signalGenerator.Type = SignalGeneratorType.Sin;
		signalGenerator.Frequency = 440;
		signalGenerator.Gain = 0.02;
		
		using (var wo = new WaveOutEvent())
		{
			wo.Init(signalGenerator);
			wo.Play();
			Thread.Sleep(500);
		}
	}

	public static void SetTitleStatus(string status)
	{
		if (string.IsNullOrEmpty(status)) Console.Title = "[SanCity] 1C обновления";
		else Console.Title = "[SanCity] - " + status;
	}

	public static class Log {	
		public static void WithColor(string message, ConsoleColor color)
		{
			ConsoleColor originalColor = Console.ForegroundColor;

			Console.ForegroundColor = color;
			Console.Write(message);
			Console.ForegroundColor = originalColor;
		}

		public static void Success(string message) => WithColor(message + "\n", ConsoleColor.Green);

		public static void Error(string message) => WithColor("[ERR] " + message + "\n", ConsoleColor.Red);

		public static void Warn(string message) => WithColor("[WARN] " + message + "\n", ConsoleColor.Yellow);

		public static void SkipLine() => Console.WriteLine();

		public static void SkipLine(int n) {
			for (int i = 0; i < n; i++) SkipLine();
		}

		public static void Write(string message) => Console.Write(message);

		public static void WriteLine(string message) => Console.WriteLine(message);

		public static void Clear() => Console.Clear();
	}
}