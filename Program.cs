using System.Runtime.InteropServices;

using NAudio.Dsp;
using NAudio.Wave;

internal class ClapDetector(float threshold = 0.9f, float clapCooldown = 500)
{
    private WaveInEvent _waveIn = null!;
    private DateTime? _lastClapTime;
    private TimeSpan _clapCooldown = TimeSpan.FromMilliseconds(clapCooldown);
    private const int FFT_LENGTH = 1024;

    public void Start()
    {
        _waveIn = new WaveInEvent
        {
            DeviceNumber = 0,
            WaveFormat = new WaveFormat(44100, 16, 1),
            BufferMilliseconds = 50
        };

        _waveIn.DataAvailable += OnStartDataAvailable;
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.StartRecording();
    }
    private void OnStartDataAvailable(object? sender, WaveInEventArgs e)
    {
        _waveIn.DataAvailable -= OnStartDataAvailable;
        Console.WriteLine("Слушаем хлопки... Нажмите Enter для выхода.");
    }
    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        // Анализ громкости
        float maxVolume = GetMaxVolume(e.Buffer);

        // Анализ частотного спектра
        (float highFreqEnergy, float lowFreqEnergy) = AnalyzeFrequency(e.Buffer);

        bool isLoudSound = maxVolume > threshold;
        bool isClapCooldown = _lastClapTime == null || DateTime.Now - _lastClapTime > _clapCooldown;
        float deltaFreq = MathF.Abs(lowFreqEnergy - highFreqEnergy);
        bool isInFreqRange = deltaFreq is < 0.1f and > (float)1E-7;
        // Комплексная проверка на хлопок
        if (!isLoudSound || !isInFreqRange || !isClapCooldown)
        {
            return;
        }

        _lastClapTime = DateTime.Now;
        OnClapDetected?.Invoke();
    }

    private static float GetMaxVolume(byte[] buffer)
    {
        float max = 0;
        for (int i = 0; i < buffer.Length; i += 2)
        {
            short sample = BitConverter.ToInt16(buffer, i);
            float volume = Math.Abs(sample / 32768f);
            if (volume > max)
            {
                max = volume;
            }
        }

        return max;
    }

    private static (float high, float low) AnalyzeFrequency(byte[] buffer)
    {
        Complex[] complexBuffer = new Complex[FFT_LENGTH];
        for (int i = 0; i < FFT_LENGTH; i++)
        {
            complexBuffer[i].X = i * 2 < buffer.Length
                ? BitConverter.ToInt16(buffer, i * 2) / 32768f
                : 0;
            complexBuffer[i].Y = 0;
        }

        FastFourierTransform.FFT(true, (int)Math.Log(FFT_LENGTH, 2), complexBuffer);

        float high = 0, low = 0;
        for (int i = 0; i < FFT_LENGTH / 2; i++)
        {
            float magnitude = (complexBuffer[i].X * complexBuffer[i].X) +
                            (complexBuffer[i].Y * complexBuffer[i].Y);
            float freq = i * 44100f / FFT_LENGTH;

            if (freq < 1000)
            {
                low += magnitude;
            }
            else if (freq is >= 2000 and <= 6000)
            {
                high += magnitude;
            }
        }

        return (high, low);
    }

    public event Action? OnClapDetected;

    public void Stop()
    {
        _waveIn.StopRecording();
        _waveIn.Dispose();
    }
}

internal static partial class KeyImitation
{
    public enum KeyState : uint
    {
        /// <summary>
        /// Клавиша нажата (значение по умолчанию)
        /// </summary>
        Down = 0x0000,

        /// <summary>
        /// Использовать расширенный скан-код (для мультимедийных клавиш)
        /// </summary>
        Extended = 0x0001,

        /// <summary>
        /// Клавиша отпущена
        /// </summary>
        Up = 0x0002,

        /// <summary>
        /// Использовать скан-код вместо виртуального кода клавиши
        /// </summary>
        ScanCode = 0x0008,

        /// <summary>
        /// Отправлять Unicode-символ вместо виртуального кода
        /// </summary>
        Unicode = 0x0004
    }
    public enum Key : byte
    {
        // Базовые клавиши
        LeftMouse = 0x01,
        RightMouse = 0x02,
        Cancel = 0x03,
        Backspace = 0x08,
        Tab = 0x09,
        Clear = 0x0C,
        Enter = 0x0D,
        Shift = 0x10,
        Control = 0x11,
        Alt = 0x12,
        Pause = 0x13,
        CapsLock = 0x14,
        Escape = 0x1B,
        Space = 0x20,

        // Навигация
        PageUp = 0x21,
        PageDown = 0x22,
        End = 0x23,
        Home = 0x24,
        LeftArrow = 0x25,
        UpArrow = 0x26,
        RightArrow = 0x27,
        DownArrow = 0x28,
        Select = 0x29,
        Print = 0x2A,
        Execute = 0x2B,
        Insert = 0x2D,
        Delete = 0x2E,
        Help = 0x2F,

        // Цифры
        D0 = 0x30,
        D1 = 0x31,
        D2 = 0x32,
        D3 = 0x33,
        D4 = 0x34,
        D5 = 0x35,
        D6 = 0x36,
        D7 = 0x37,
        D8 = 0x38,
        D9 = 0x39,

        // Буквы
        A = 0x41,
        B = 0x42,
        C = 0x43,
        D = 0x44,
        E = 0x45,
        F = 0x46,
        G = 0x47,
        H = 0x48,
        I = 0x49,
        J = 0x4A,
        K = 0x4B,
        L = 0x4C,
        M = 0x4D,
        N = 0x4E,
        O = 0x4F,
        P = 0x50,
        Q = 0x51,
        R = 0x52,
        S = 0x53,
        T = 0x54,
        U = 0x55,
        V = 0x56,
        W = 0x57,
        X = 0x58,
        Y = 0x59,
        Z = 0x5A,

        // Системные клавиши
        LeftWin = 0x5B,
        RightWin = 0x5C,
        Apps = 0x5D,
        Sleep = 0x5F,

        // Numpad
        NumPad0 = 0x60,
        NumPad1 = 0x61,
        NumPad2 = 0x62,
        NumPad3 = 0x63,
        NumPad4 = 0x64,
        NumPad5 = 0x65,
        NumPad6 = 0x66,
        NumPad7 = 0x67,
        NumPad8 = 0x68,
        NumPad9 = 0x69,
        Multiply = 0x6A,
        Add = 0x6B,
        Separator = 0x6C,
        Subtract = 0x6D,
        Decimal = 0x6E,
        Divide = 0x6F,

        // Функциональные клавиши
        F1 = 0x70,
        F2 = 0x71,
        F3 = 0x72,
        F4 = 0x73,
        F5 = 0x74,
        F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 0x78,
        F10 = 0x79,
        F11 = 0x7A,
        F12 = 0x7B,
        F13 = 0x7C,
        F14 = 0x7D,
        F15 = 0x7E,
        F16 = 0x7F,
        F17 = 0x80,
        F18 = 0x81,
        F19 = 0x82,
        F20 = 0x83,
        F21 = 0x84,
        F22 = 0x85,
        F23 = 0x86,
        F24 = 0x87,

        // Состояния
        NumLock = 0x90,
        ScrollLock = 0x91,

        // Модификаторы
        LeftShift = 0xA0,
        RightShift = 0xA1,
        LeftControl = 0xA2,
        RightControl = 0xA3,
        LeftAlt = 0xA4,
        RightAlt = 0xA5,

        // Мультимедиа
        VolumeMute = 0xAD,
        VolumeDown = 0xAE,
        VolumeUp = 0xAF,
        MediaNext = 0xB0,
        MediaPrev = 0xB1,
        MediaStop = 0xB2,
        MediaPlayPause = 0xB3
    }
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    public static void PressKey(Key key, float delay = 0)
    {
        SetStateKey(key, KeyState.Down);
        Thread.Sleep((int)delay);
        SetStateKey(key, KeyState.Up);
    }
    // ReSharper disable once MemberCanBePrivate.Global
    public static void SetStateKey(Key key, KeyState keyState)
    {
        keybd_event((byte)key, 0, (uint)keyState, UIntPtr.Zero);
    }
}
internal static class Program
{
    private static void Main()
    {
        ClapDetector detector = new();
        detector.OnClapDetected += () =>
        {
            Console.WriteLine("Хлопок обнаружен!");
            KeyImitation.PressKey(KeyImitation.Key.Space);
        };
        detector.Start();

        Console.ReadLine();
        detector.Stop();
    }
}