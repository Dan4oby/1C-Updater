using System;

public static class PathConstants
{
    // %USERPROFILE%
    public static string userProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    // %USERPROFILE%/Desktop
    public static string desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    // C:/Program Files
    public static string programFiles => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    // C:/Program Files x86
    public static string programFilesX86 => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

    // C:/Program Data
    public static string commonProgramData => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

    public static string AppData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static string cExe = "1cv8.exe";

    static string cStarterLnk = "1C Предприятие.lnk";
    static string cStarter = "1cestart.exe";
    public static List<string> cStarterPossiblePaths = new()
    {
        Path.Combine(userProfile, "Desktop", cStarterLnk),
        Path.Combine(programFiles, "1cv8", "common", cStarter),
        Path.Combine(programFilesX86, "1cv8", "common", cStarter),
        Path.Combine(userProfile, "AppData", "Microsoft", "Windows", "Start Menu", "Programs", cStarterLnk),
        Path.Combine(commonProgramData, "Microsoft", "Windows", "Start Menu", "Programs", cStarterLnk)
    };

    public static List<string> cArchivePossiblePaths = new()
    {
        @"C:\Архив 1С",
        @"D:\Архив 1С",
        @"E:\Архив 1С",
        @"C:\Архив 1C",
        @"D:\Архив 1C",
        @"E:\Архив 1C"
    };

    
    public static List<string> possible1CDistributionsPaths = new()
    {
        Path.Combine(programFiles, "1cv8"),
        Path.Combine(programFilesX86, "1cv8"),
        Path.Combine(userProfile, "AppData", "Local", "1cv8"),
    };
}