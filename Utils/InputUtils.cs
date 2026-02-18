using System;

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

    public static bool AskYOrN(string askInputMessage)
    {
        while (true)
        {
            Log.Write(askInputMessage);
            Log.WriteLine("(д/н) (y/n)");
            string input = KernelInput.ReadLine();
            input = input.Trim();
            input = input.ToLower();
            if (!input.Equals("y") && !input.Equals("n") && !input.Equals("д") && !input.Equals("н")) {
                Log.Error("Неверный ввод");
            }
            else 
            {
                if (input.Equals("y") || input.Equals("д")) {
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

        Console.WriteLine();
        return password;
    }
}