using static CommonUtils;
using static PathConstants;

public static class TemplatesFilePathParser
{
    public static string GetTemplatesPath()
    {
        string[] lines = File.ReadAllLines(Path.Combine(AppData, "1C", "1CEStart", "1cestart.cfg"));

        foreach (string line in lines)
        {
            // ConfigurationTemplatesLocation=C:\Users\Ученик\AppData\Roaming\1C\1cv8\tmplts
            if (line.StartsWith("ConfigurationTemplatesLocation"))
            {
                string[] tokens = line.Split('=');
                return tokens[1];
            }
        }

        return "";
    }
}