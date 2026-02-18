using System;
using System.Runtime.InteropServices;
using System.Text;

// код взят у нейросети, непонятно, как он работает
// но он работает с вводом сложных строк вида
// File="C:\Users\Ученик\Documents\Обучение 1с";
// которые стандартный Console.ReadLine() не принимает
public static class KernelInput
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool ReadConsoleW(
        IntPtr hConsoleInput,
        [Out] StringBuilder lpBuffer,
        uint nNumberOfCharsToRead,
        out uint lpNumberOfCharsRead,
        IntPtr lpReserved);

    private const int STD_INPUT_HANDLE = -10;

    public static string ReadLine()
    {
        IntPtr hStdin = GetStdHandle(STD_INPUT_HANDLE);
        if (hStdin == IntPtr.Zero || hStdin == (IntPtr)(-1))
            throw new InvalidOperationException("No console input handle");

        // Буфер достаточного размера
        StringBuilder sb = new StringBuilder(256);
        uint read = 0;
        if (ReadConsoleW(hStdin, sb, (uint)sb.Capacity, out read, IntPtr.Zero))
        {
            // Убираем завершающие \r\n
            string result = sb.ToString(0, (int)read);
            if (result.EndsWith("\r\n")) return result.Substring(0, result.Length - 2);
            if (result.EndsWith("\n"))   return result.Substring(0, result.Length - 1);
            if (result.EndsWith("\r"))   return result.Substring(0, result.Length - 1);

            return result;
        }
        else
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}