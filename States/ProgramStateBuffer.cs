using System;

using static CommonUtils;

public class ProgramStateBuffer
{
    public string titleState;
    public string headerState;
    private Dictionary<string, ConsoleColor> stringbuffer;

    public ProgramStateBuffer() {
        stringbuffer = new Dictionary<string, ConsoleColor>();
        titleState = "";
        headerState = "";
    }

    public void AddToStringBuffer(string str, ConsoleColor color)
    {
        stringbuffer.Add(str, color);
    }

    public void NewProgramState(string title, string header)
    {
        stringbuffer.Clear();
        titleState = title;
        headerState = header;
        SetState(header, title);

    }

    public void StateRedraw()
    {
        SetState(titleState, headerState);

        foreach (var str in stringbuffer)
        {
            Log.WithColorNoSaveInBuffer(str.Key, str.Value);
        }
    }
}