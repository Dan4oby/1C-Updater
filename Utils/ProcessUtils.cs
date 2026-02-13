using System;
using System.Diagnostics;

public static class ProcessUtils
{
    static int RunProcess(string fileName, string arguments="", bool useShellExecute=false, bool createWindow=true, bool waitForProcess=true)
	{
		try
		{
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = arguments,
				UseShellExecute = useShellExecute,
				CreateNoWindow = !createWindow
			};
            Process p = Process.Start(psi);
                    
            if (waitForProcess && p != null)
            {
                p.WaitForExit();
                return p.ExitCode;
            }
		}
		catch
		{
			return -1;
		}

        return 0;
	}

    static int RunProcessNoWait(string fileName)
    {
        return RunProcess(fileName, waitForProcess:false);
    }
}
