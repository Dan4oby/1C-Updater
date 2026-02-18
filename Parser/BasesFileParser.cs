using System;

using static CommonUtils;
using static PathConstants;

public static class BasesFileParser
{
    public static Dictionary<string, string> GetBases()
    {
        string[] lines = File.ReadAllLines(Path.Combine(AppData, "1C", "1CEStart", "ibases.v8i"));

        var bases = new Dictionary<string, string>();

        string baseName = "";
        bool nextTokenIsConnectString = false;
        foreach (string line in lines)
        {
            if (line.StartsWith('['))
            {
                // [Информационная база #1]
                baseName = line.Replace("[", "");
                baseName = baseName.Replace("]", "");
                nextTokenIsConnectString = true;
                continue;
            }
            if (nextTokenIsConnectString)
            {
                // Connect=File="C:\Users\Ученик\Documents\Учебная база 1С";
                string[] tokens = line.Split('=');
                string connectionString = tokens[1] + "=" + tokens[2];

                bases.Add(baseName, connectionString);

                nextTokenIsConnectString = false;
            }
        }

        return bases;
    }
}