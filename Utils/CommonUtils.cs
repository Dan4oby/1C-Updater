using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Text.RegularExpressions;

public static class CommonUtils 
{

    public static async Task PlayLookAtMeSound()
    {
        await Task.Run(() =>
        {
			try
			{
				// Нота 1 (До) - 261.63 Гц
				using (var wo = new WaveOutEvent())
				{
					var note1 = new SignalGenerator(44100, 1)
					{
						Type = SignalGeneratorType.Sin,
						Frequency = 261.63,
						Gain = 0.02
					};
					wo.Init(note1);
					wo.Play();
					Thread.Sleep(400);
				} 

				// Нота 2 (Ми) - 329.63 Гц
				using (var wo = new WaveOutEvent())
				{
					var note2 = new SignalGenerator(44100, 1)
					{
						Type = SignalGeneratorType.Sin,
						Frequency = 329.63,
						Gain = 0.02
					};
					wo.Init(note2);
					wo.Play();
					Thread.Sleep(400);
				}

				// Нота 3 (Соль) - 392.00 Гц
				using (var wo = new WaveOutEvent())
				{
					var note3 = new SignalGenerator(44100, 1)
					{
						Type = SignalGeneratorType.Sin,
						Frequency = 392.00,
						Gain = 0.02
					};
					wo.Init(note3);
					wo.Play();
					Thread.Sleep(500);
				}
			}
            catch (Exception ex)
            {
                // Логирование ошибки
                System.Diagnostics.Debug.WriteLine($"Sound error: {ex.Message}");
            }
        });
    }


	public static void SetTitleStatus(string status)
	{
		if (string.IsNullOrEmpty(status)) Console.Title = "[SanCity] 1C обновления";
		else Console.Title = "[SanCity] - " + status;
	}

	public static void SetState(string state, string status)
	{
		state = state.ToUpper();
		state.Trim();
		SetTitleStatus(status);
		Log.Clear();
		Log.WithColor($"================{state}================\n", ConsoleColor.Magenta);
		Log.SkipLine();
	}

	public static void Pause()
	{
		Console.Write("Нажмите Enter для продолжения...");
		Console.ReadLine();
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