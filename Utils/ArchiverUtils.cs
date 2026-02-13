using System;

using static PathConstants;

public static class ArchiverUtils
{
    public static string path7Zip;
    public static string pathWinRAR;


    
    public static bool TarExists()
    {
        return File.Exists(Path.Combine(Environment.SystemDirectory, "tar.exe"));
    }

    public static void UnZip(string path)
    {
        string[] tokens = path.Split(".");
        string fileFormat = tokens[tokens.Length - 1];
        string fileName = tokens[tokens.Length - 2];
        
        switch (fileFormat)
        {
            case "zip":
                break;
            case "rar":
                break;
            default:
                break;
        }
    }

    static ArchiverUtils() {
        path7Zip = Find7Zip();
        pathWinRAR = FindWinRAR();
    }

    private static string Find7Zip()
    {
        string pf_7Zip = Path.Combine(programFiles, "7-Zip", "7z.exe");
		string pf86_7Zip = Path.Combine(programFilesX86, "7-Zip", "7z.exe");

        if (File.Exists(pf_7Zip))
			return pf_7Zip;
		else if (File.Exists(pf86_7Zip))
			return pf86_7Zip;
        return "";
    }

    private static string FindWinRAR()
    {
        string pf_WinRAR = Path.Combine(programFiles, "WinRar", "UnRar.exe");
		string pf86_WinRAR = Path.Combine(programFilesX86, "WinRar", "UnRar.exe");

        if (File.Exists(pf_WinRAR))
			return pf_WinRAR;
		else if (File.Exists(pf86_WinRAR))
			return pf86_WinRAR;
        return "";
    }
        
}