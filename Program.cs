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
	public static ProgramStateBuffer stateBuffer;
	static Program() {
		stateBuffer = new ProgramStateBuffer();

		Console.OutputEncoding = Encoding.UTF8;
		Console.InputEncoding = Encoding.UTF8;

		//Console.BackgroundColor = ConsoleColor.DarkCyan;
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.White;
	}

	static void Main(string[] args) {
		
		Log.WriteLine("Введите пароль, чтобы пользоваться программой");
		string password = ReadPassword();

		if (!password.Equals("1256")) return;
		
		IStateExecuter stateToExecute = null;
		while (true)
		{
			int choice = UserHaveToChooseBetween(
				"Выберите режим работы программы: ",
				new string[] {
					"",
					"",
					""
					}
			);

			if (choice == 1) stateToExecute = new Update1CState();
			if (choice == 2) stateToExecute = null;
			if (choice == 3) stateToExecute = null;
			
			stateBuffer.StateRedraw();
			if (stateToExecute != null)
			{
				Console.Clear();
				stateToExecute.Execute();
				stateToExecute = null;
			}
		}
	}
}