using System;
using static Utils;

class CLIScreen
{
    public CLIScreen()
    {
        
    }

    private Action screenActions;
    
    public void SetRunnable(Action screenActions)
    {
        this.screenActions = screenActions;
    }

    public void SetErrorHandlingBehaviour()
    {
        
    }
    public RunnableStatus ShowAndRun()
    {
        try
        {
            screenActions?.Invoke(); // Вызов переданного метода
        } 
        catch (Exception ex)
        {
            Log.Clear();
            
            Log.Error("Произошла ошибка при исполнении кода. Ниже вывожу ошибку: ");
            Log.SkipLine();
            Log.Error($"Исключение: {ex.Message}");
            Log.SkipLine();
            Log.Error($"Метод: {ex.TargetSite}");
            Log.SkipLine();
            Log.Error($"Трассировка стека: {ex.StackTrace}");
            Log.SkipLine(2);
            Log.WriteLine("Введите что-то, если хотите заново выполнить данный блок, либо завершите программу. ");
            Console.ReadLine();
            
        }
        return RunnableStatus.Complete;
        
    }

    public enum RunnableStatus { Complete, WithError }

    public enum ErrorHandlingBehaviour { RelaunchScreen, ReturnError }
}