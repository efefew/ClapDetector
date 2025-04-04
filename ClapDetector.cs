using NAudio.Dsp;
using NAudio.Wave;

internal class ClapDetector(float threshold = 0.9f, float clapCooldown = 500)
{
    private WaveInEvent _waveIn = null!;
    private DateTime? _lastClapTime;
    private TimeSpan _clapCooldown = TimeSpan.FromMilliseconds(clapCooldown);
    private const int FFT_LENGTH = 1024;
    public event Action? OnClapDetected;
    private bool _isRun = false;
    public void Start()
    {
        if(_isRun) return;
        _waveIn = new WaveInEvent
        {
            DeviceNumber = 0,
            WaveFormat = new WaveFormat(44100, 16, 1),
            BufferMilliseconds = 50
        };

        _waveIn.DataAvailable += OnStartDataAvailable;
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.StartRecording();
        _isRun = true;
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

            switch (freq)
            {
                case < 1000:
                    low += magnitude;
                    break;
                case >= 2000 and <= 6000:
                    high += magnitude;
                    break;
            }
        }

        return (high, low);
    }
    public void Stop()
    {
        if(!_isRun) return;
        _waveIn.DataAvailable -= OnStartDataAvailable;
        _waveIn.DataAvailable -= OnDataAvailable;
        _waveIn.StopRecording();
        _waveIn.Dispose();
        _isRun = false;
    }
}