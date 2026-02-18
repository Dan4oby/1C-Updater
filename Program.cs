using System.Diagnostics;
using System.IO.Compression;
using System.Text; 
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using static PathConstants;
using static CommonUtils;
using static InputUtils;


class Program {
	static Program() {

		Console.OutputEncoding = Encoding.UTF8;
		Console.InputEncoding = Encoding.UTF8;

		//Console.BackgroundColor = ConsoleColor.DarkCyan;
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.White;
	}

	static void Main(string[] args) {
		
		Log.WriteLine("Введите пароль, чтобы пользоваться программой");
		string password = ReadPassword();

		if (!password.Equals("1234")) return;
		
		IStateExecuter stateToExecute = null;
		while (true)
		{
			SetState("Выбор режима работы программы", "Выбор режима");

			Log.WriteLine("Выберите, что вы хотите сделать:");
			Log.WriteLine("1"); // обновить 1с
			Log.WriteLine("2"); // удалить кеш
			Log.WriteLine("3"); // восстановить целостность БД
			string input = KernelInput.ReadLine();
			input = input.Trim();

			int choice = 0;
			try
			{
				choice = int.Parse(input);
			} 
			catch
			{
				Log.Error("Неверный ввод");
				Pause();
				continue;
			}

			if (choice < 1 || choice > 3) {
				continue;
			}

			if (choice == 1) stateToExecute = new Update1CState();
			if (choice == 2) stateToExecute = null;
			if (choice == 3) stateToExecute = null;

			if (stateToExecute != null)
			{
				Log.Clear();
				stateToExecute.Execute();
				stateToExecute = null;
			}
		}
	}
}