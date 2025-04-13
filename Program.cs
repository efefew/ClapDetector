_ = new Autorun();

ClapDetector detector = new();
detector.OnClapDetected += () =>
{
    Console.WriteLine("Хлопок обнаружен!");
    KeyImitation.PressKey(KeyImitation.Key.MediaPlayPause);
};

while (true)
{
    if (WindowStatus.IsActiveProcess(processName: "chrome"))
    {
        detector.Start();
    }
    else
    {
        detector.Stop();
    }

    Thread.Sleep(millisecondsTimeout: 1000);
}