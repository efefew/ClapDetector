# ClapDetector: Управление клавишами через хлопки 🎤👏⌨️

**Приложение для детекции хлопков с последующей эмуляцией нажатия клавиш.**  
Используйте хлопки для управления медиаплеером, презентациями, играми и другими приложениями!

## 📦 Установка
1. **Требования**:
   - .NET 6.0+
   - Микрофон
   - [NAudio](https://github.com/naudio/NAudio) (установится автоматически)

2. **Сборка**:
git clone https://github.com/yourusername/clap2key.git
cd clap2key
dotnet restore
## 🚀 Быстрый старт
```csharp
var detector = new ClapDetector(threshold: 0.85f);
detector.OnClapDetected += () => KeyImitation.PressKey(Key.Space);
detector.Start();
Console.ReadLine();
detector.Stop();
```
⚙️ Настройка детекции
|   Параметр  | По умолчанию |          Описание            |
|-------------|--------------|------------------------------|
| threshold   | 0.9f         | Чувствительность (0.0-1.0)   |
| clapCooldown| 500 мс       | Задержка между срабатываниями|
```csharp
// Кастомная настройка
new ClapDetector(
    threshold: 0.8f,
    clapCooldown: TimeSpan.FromMilliseconds(300)
);
```
## 🔍 Как это работает?
Алгоритм детекции:

   - Анализ громкости
   - Определение пиковой амплитуды звука в реальном времени.
   - Частотный анализ (FFT)
   - Размер буфера: 1024 сэмпла
   - Частотные диапазоны:
      Низкие (0-1000 Гц)
      Высокие (2000-6000 Гц)
   - Фильтрация ложных срабатываний
   - Проверка соотношения частот
   - Защита от дребезга (cooldown)

## 🛠 Примеры использования
   - Пауза видео:
```csharp
detector.OnClapDetected += () => {
    KeyImitation.PressKey(Key.Space);
};
```
   - Управление медиаплеером:
```csharp
detector.OnClapDetected += () => {
    KeyImitation.PressKey(Key.MediaPlayPause);
};
```
   - Навигация в презентациях:
```csharp
detector.OnClapDetected += () => {
    KeyImitation.PressKey(Key.RightArrow); // Следующий слайд
};
```
## 📋 Список поддерживаемых клавиш
Полный перечень в классе KeyImitation.Key, включая:

   - Базовые (Space, Enter, Esc)
   - Мультимедийные (VolumeUp, MediaNext)
   - Системные (Win, Alt, Ctrl)
   - Numpad и функциональные клавиши

## ⚠️ Ограничения
Требуются права на доступ к микрофону
Возможны ложные срабатывания при:
   - Резких хлопках
   - Фоновых шумах (дрель, стук посуды)
   - Громких низкочастотных звуках
