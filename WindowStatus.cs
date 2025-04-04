using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

internal static class WindowStatus
{
    // Импорт функций из user32.dll
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    
    public static bool IsActiveProcess(string processName)
    {
        const int N_CHARS = 256;
        StringBuilder windowText = new(N_CHARS);
        IntPtr foregroundWindow = GetForegroundWindow();

        if (foregroundWindow == IntPtr.Zero)
            return false;

        // Получаем заголовок окна
#pragma warning disable CA1806
        GetWindowText(foregroundWindow, windowText, N_CHARS);
#pragma warning restore CA1806

        // Получаем ID процесса
        _ = GetWindowThreadProcessId(foregroundWindow, out uint processId);

        // Получаем процесс по ID
        Process process = Process.GetProcessById((int)processId);

        // Проверяем, является ли процесс искомым
        return process.ProcessName.Contains(processName, StringComparison.CurrentCultureIgnoreCase);
    }
}