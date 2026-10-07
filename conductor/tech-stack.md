# Tech Stack — ScreenTextCatcher

## Базовая платформа и среда выполнения
- **Язык**: C# 12 / 13
- **Платформа**: .NET 9.0 (целевой фреймворк: `net9.0-windows`)
- **Тип приложения**: Windows Desktop Application (фоновое приложение в трее без консоли)

## Графический интерфейс (UI/UX)
- **UI Фреймворк**: WPF (Windows Presentation Foundation)
  - Полноэкранный прозрачный оверлей выбора области (`AllowsTransparency="True"`, `WindowStyle="None"`, 50% затемнение, Canvas с контрастной рамкой).
  - Компактные минималистичные окна: Настройки (Settings), Просмотр логов (Log Viewer), История (History).
- **Системный трей и бренд-ассеты**: Интеграция с треем Windows с контекстным меню и индикацией статуса. Встроенные предкомпилированные многослойные ресурсы .ico (16×16 до 256×256 px) для исполняемого файла, окон WPF и состояний трея (Idle, Processing, Error) без процедурной GDI+ отрисовки.
- **Глобальные хоткеи**: Win32 P/Invoke (`RegisterHotKey` / `UnregisterHotKey`) через `HwndSource`.
- **Автозапуск Windows**: Win32 COM Interop (`IShellLinkW` / `IPersistFile`) для генерации ярлыков в `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup` без сторонних зависимостей.

## Сеть, OCR и захват экрана
- **Сетевой клиент**: `System.Net.Http.HttpClient` с `SocketsHttpHandler`:
  - Поддержка HTTP/HTTPS и SOCKS5 прокси (`WebProxy`, `socks5://`).
  - Методы тестирования подключения к прокси и проверки Mistral API ключа.
- **OCR Сервис**: Mistral OCR Cloud API:
  - Отправка вырезанного фрагмента изображения в base64 в памяти.
  - Автоматическое копирование распознанного текста в `System.Windows.Clipboard`.
- **Захват экрана в RAM**:
  - `Graphics.CopyFromScreen` / Win32 `BitBlt` для захвата мониторов с учетом DPI.
  - Кадрирование и кодирование в `MemoryStream` (строго без сохранения на диск в режиме OCR).
- **Сервис скриншотов, аннотаций и буфера обмена**:
  - `IScreenshotService` / `ScreenshotService` для сохранения фрагментов экрана в PNG с авто-разрешением коллизий имен файлов.
  - Подсистема аннотаций и экспорта: геометрические хелперы (`AnnotationGeometryHelper`, `ToolbarPositioningHelper`), динамический оверлей со стеком отмены (Undo), композитный рендеринг через `AnnotationExportHelper` (`RenderTargetBitmap` с сохранением DPI масштабирования).
  - Алгоритм строгой валидации содержимого буфера обмена и аккумуляции путей с поддержкой разделителей (`NewLine`, `Space`).
  - Поддержка помещения графического изображения в буфер обмена Windows (форматы `BitmapSource` и `PNG` поток с механизмом retry) через метод `IScreenshotService.SaveAndCopyImageToClipboard`.

## Хранение данных и логирование
- **Конфигурация**: `System.Text.Json` (`settings.json` в `%APPDATA%\ScreenTextCatcher\`).
- **История (100 записей)**: `Microsoft.Data.Sqlite` (`history.db` в `%APPDATA%\ScreenTextCatcher\`).
- **Логирование**: `Serilog` с ротацией файлов логов в `%APPDATA%\ScreenTextCatcher\logs\` и потоком событий в диалог просмотра логов.

## CI/CD и релизные пайплайны
- **Платформа автоматизации**: GitHub Actions (Windows runner `windows-latest`).
- **Непрерывная интеграция (CI)**: Автоматическая сборка и прогон xUnit тестов при каждом пуше и PR в `master`.
- **Релизный пайплайн (Release)**: Публикация автономного single-file приложения `ScreenTextCatcher.exe` (`win-x64`) и упаковка в zip-архив при пуше тегов `v*` или ручном запуске (`workflow_dispatch`).

