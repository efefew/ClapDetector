using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Win32;

/// <summary>
/// Запись включения и выключения компьютера
/// </summary>
internal class Autorun
{
    private const string AUTORUN_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private readonly string APPLICATION_NAME = Assembly.GetEntryAssembly()!.GetName().Name!;
    private readonly string APPLICATION_PATH = Assembly.GetEntryAssembly()!.Location;

    public Autorun()
    {
        AddAutoLoad(true);
    }

    /// <summary>
    /// Подключение автозагрузки
    /// </summary>
    /// <param name="autorun">подключить</param>
    [SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы")]
    private void AddAutoLoad(bool autorun)
    {
        using RegistryKey? registry = Registry.CurrentUser.OpenSubKey(AUTORUN_PATH, true);
        if (autorun)
        {
            if (registry!.GetValueNames().All(t => APPLICATION_NAME != t))
                registry.SetValue(APPLICATION_NAME, $"\"{APPLICATION_PATH}\"");
        }
        else
        {
            registry!.DeleteValue(APPLICATION_NAME, false);
        }
    }
}