[Russian readme](https://github.com/efefew/ClapDetector/blob/master/README_RUS.md)
# ClapDetector: Control keys with claps 🎤👏⌨️

**Application for detecting claps with subsequent emulation of key presses.**
Use claps to control your media player, presentations, games and other applications!

## 📦 Installation
1. **Requirements**:
- .NET 6.0+
- Microphone
- [NAudio](https://github.com/naudio/NAudio) (will be installed automatically)

2. **Build**:
git clone https://github.com/yourusername/clap2key.git
cd clap2key
dotnet restore
## 🚀 Quick start
```csharp
var detector = new ClapDetector(threshold: 0.85f);
detector.OnClapDetected += () => KeyImitation.PressKey(Key.Space);
detector.Start();
Console.ReadLine();
detector.Stop();
```
⚙️ Detection settings
| Parameter | Default | Description |
|-------------|--------------|------------------------------|
| threshold | 0.9f | Sensitivity (0.0-1.0) |
| clapCooldown| 500 ms | Delay between triggers|
```csharp
// Custom setting
new ClapDetector(
threshold: 0.8f,
clapCooldown: TimeSpan.FromMilliseconds(300)
);
```
## 🔍 How does it work?
Detection algorithm:

- Loudness analysis
- Real-time peak amplitude detection.
- Frequency analysis (FFT)
- Buffer size: 1024 samples
- Frequency ranges:
Low (0-1000 Hz)
High (2000-6000 Hz)
- False alarm filtering
- Frequency ratio check
- Anti-bounce (cooldown)

## 🛠 Usage examples
- Video pause:
```csharp
detector.OnClapDetected += () => {
KeyImitation.PressKey(Key.Space);
};
```
- Media player control:
```csharp
detector.OnClapDetected += () => {
KeyImitation.PressKey(Key.MediaPlayPause);
};
```
- Presentation navigation:
```csharp
detector.OnClapDetected += () => {
KeyImitation.PressKey(Key.RightArrow); // Next slide
};
```
## 📋 List of supported keys
Full list in KeyImitation.Key class, including:

- Basic (Space, Enter, Esc)
- Multimedia (VolumeUp, MediaNext)
- System (Win, Alt, Ctrl)
- Numpad and function keys

## ⚠️ Limitations
Requires microphone access rights
Possible false triggers with:
- Sharp claps
- Background noise (drill, clatter of dishes)
- Loud low-frequency sounds
