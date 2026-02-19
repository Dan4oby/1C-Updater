using static CommonUtils;

public static class InputUtils
{
    public static string ControlUserForCorrectInput(string askInputMessage, string wrongInputMessage, Func<string, bool> validationRule)
    {
        while (true)
        {
            Log.WriteLine(askInputMessage);
            string input = KernelInput.ReadLine();
            input = input.Trim();
            if (!validationRule(input)) {
                Log.Error(wrongInputMessage);
                Log.Error($"Ваш ввод: {input}");
            }
            else return input;
        }
    }

    public static bool IsEmpty(string? str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsValidString(string str)
    {
        return str[0] != ' ' && str.Last() != ' ';
    }

    public static bool AskYOrN(string askInputMessage, bool inverseEnter=false)
    {
        while (true)
        {
            Log.Write(askInputMessage);
            Log.SkipLine();
            if (!inverseEnter)
            {
                Log.WithColor("[д/y] ", ConsoleColor.Green);
                Log.WithColor("[н/n/Ent]\n", ConsoleColor.Red);
            }
            else
            {
                Log.WithColor("[д/y/Ent] ", ConsoleColor.Green);
                Log.WithColor("[н/n]\n", ConsoleColor.Red);
            }

            string input = KernelInput.ReadLine();
            input = input.Trim();
            input = input.ToLower();
            if (!string.IsNullOrEmpty(input) && !input.Equals("y") && !input.Equals("n") && !input.Equals("д") && !input.Equals("н")) {
                Log.Error("Неверный ввод");
            }
            else 
            {
                if (input.Equals("y") || input.Equals("д") || (inverseEnter && IsEmpty(input))) {
                    return true;
                }
                return false;
            }
        }
    }

    public static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo keyInfo;

        do
        {
            keyInfo = Console.ReadKey(true); // true - не отображать нажатую клавишу
            
            if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Backspace)
            {
                password += keyInfo.KeyChar;
                Console.Write("*");
            }
            else if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1);
                Console.Write("\b \b"); 
            }
        }
        while (keyInfo.Key != ConsoleKey.Enter);

        Log.SkipLine();
        return password;
    }

    public static int UserHaveToChooseBetween(string askInputMessage, string[] variants)
    {
        while (true)
        {
            SetState("Выбор варианта работы", "Выбор...");

            Log.Write(askInputMessage);
            Log.SkipLine();
            for (int i = 0; i < variants.Length; i++)
            {
                Log.WriteLine($"[{i + 1}] {variants[i]}");
            }
            Log.SkipLine();
            Log.Write("Ваш выбор: ");
            string input = KernelInput.ReadLine();
            input = input.Trim();
            int choice;
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
            if (choice < 1 || choice >= variants.Length) continue;

            return choice;
        }
    }
}