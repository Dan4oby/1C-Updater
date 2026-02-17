using System;

using static PathConstants;
using static ProcessUtils;
public static class ArchiverUtils
{
    public static string path7Zip;
    public static string pathWinRAR;

    public static string pathTar;

    public static bool TarExists()
    {
        return File.Exists(pathTar);
    }

    public static int UnZip(string path, string folderTo, bool waitForUnZip)
    {
        string[] tokens = path.Split(".");
        string fileFormat = tokens[tokens.Length - 1];
        string fileName = tokens[tokens.Length - 2];
        
        string archiverPath = "";
        string arguments = "";
        switch (fileFormat)
        {
            case "zip":

                if (TarExists()) {
                    archiverPath = pathTar;
                    arguments = GetTarArguments(path, folderTo);
                }
                else if (!string.IsNullOrEmpty(path7Zip)) {
                    archiverPath = path7Zip;
                    arguments = Get7ZipArguments(path, folderTo);
                }

                break;
            case "rar":
            
                if (!string.IsNullOrEmpty(pathWinRAR)) {
                    archiverPath = pathWinRAR;
                    arguments = GetWinRarArguments(path, folderTo);
                }
                else if (!string.IsNullOrEmpty(path7Zip)) {
                    archiverPath = path7Zip;
                    arguments = Get7ZipArguments(path, folderTo);
                }

                break;
            default:
                return -1;
        }

        int result = RunProcess(archiverPath, 
                    arguments:arguments,
                    createWindow:false,
                    waitForProcess:waitForUnZip);

        return result;
    }

    static ArchiverUtils() {
        path7Zip = Find7Zip();
        pathWinRAR = FindWinRAR();
        pathTar = Path.Combine(Environment.SystemDirectory, "tar.exe");
    }

    private static string Get7ZipArguments(string path, string where)
    {
        return @"x """ + path + @""" -o""" + where + @"\"" -y";
    }

    private static string GetWinRarArguments(string path, string where)
    {
        return @"x -y """ + path + @""" """ + where + @"\""";
    }

    private static string GetTarArguments(string path, string where)
    {
        return @"-xf """ + path + @""" -C """ + where + @"""";
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

    public static bool CanUnZipRar()
    {
        return !string.IsNullOrEmpty(pathWinRAR) || !string.IsNullOrEmpty(path7Zip);
    }

    public static bool CanUnZipZip()
    {
        return TarExists() || !string.IsNullOrEmpty(path7Zip);
    }
        
}