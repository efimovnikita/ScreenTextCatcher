# Implementation Plan — Окно просмотра захваченной области («Pin / Закрепить»)

## Phase 1: Локализация и вспомогательные сервисы позиционирования (Core & TDD)

- [x] Task: Строковые ресурсы локализации (TDD)
    - [x] Написать модульные тесты для новых ключей `Loc_AnnotateToolbarPinTooltip` и `Loc_PinnedViewerTitle` в `LocalizationManagerTests.cs`
    - [x] Добавить ключи `Loc_AnnotateToolbarPinTooltip` и `Loc_PinnedViewerTitle` в `LocalizationManager.cs` для русской и английской локалей
- [x] Task: Хелпер позиционирования и каскадирования окон (TDD)
    - [x] Написать модульные тесты для расчета размеров окна и центрирования/каскадирования с учетом рабочей области и DPI
    - [x] Реализовать логику расчета координат и каскадного смещения (+24 px) в хелпере позиционирования
- [x] Task: Conductor - User Manual Verification 'Phase 1: Локализация и вспомогательные сервисы позиционирования' (Protocol in workflow.md)

---

## Phase 2: Создание окна PinnedImageViewerWindow (UI & Window Logic)

- [x] Task: Разметка PinnedImageViewerWindow.xaml
    - [x] Создать файл `PinnedImageViewerWindow.xaml` со стилем `SingleBorderWindow`, `ResizeMode="CanResize"`, темным фоном `#212121`, иконкой `/Assets/app.ico` и контейнером `ScrollViewer` + `Image`
- [x] Task: Логика PinnedImageViewerWindow.xaml.cs
    - [x] Реализовать конструктор с загрузкой PNG байт в `BitmapImage` (`CacheOption = BitmapCacheOption.OnLoad`, `Freeze()`)
    - [x] Реализовать определение монитора захвата через `Screen.FromRectangle` и центрирование с ограничением 90% WorkingArea и каскадированием
    - [x] Установить локализованный заголовок окна с фактическим разрешением кадра
- [x] Task: Conductor - User Manual Verification 'Phase 2: Создание окна PinnedImageViewerWindow' (Protocol in workflow.md)

---

## Phase 3: Добавление кнопки в OverlayWindow и передача события (UI & Export)

- [x] Task: Разметка кнопки закрепления в OverlayWindow.xaml
    - [x] Добавить кнопку `BtnPin` с иконкой `📌`, стилем `ToolbarButtonStyle` и тултипом перед `BtnCopyImage`
- [x] Task: Логика события закрепления в OverlayWindow.xaml.cs
    - [x] Объявить публичное событие `public event Action<byte[], Rect>? PinRequested;`
    - [x] Реализовать обработчик `OnPinClick`, выполняющий композитный экспорт изображения с аннотациями в PNG `byte[]`, закрытие оверлея и вызов события `PinRequested`
- [x] Task: Conductor - User Manual Verification 'Phase 3: Добавление кнопки в OverlayWindow и передача события' (Protocol in workflow.md)

---

## Phase 4: Интеграция в MainWindow и оркестрация (Integration)

- [x] Task: Подписка на событие PinRequested в MainWindow.xaml.cs
    - [x] Подписаться на `overlay.PinRequested` в методе `StartOcrCapture`
    - [x] Реализовать создание и отображение `PinnedImageViewerWindow` в `Dispatcher.Invoke`
    - [x] Обеспечить сброс статуса трея в `TrayStatus.Idle`, логирование события через `Log.Information` и восстановление дашборда при необходимости
- [x] Task: Conductor - User Manual Verification 'Phase 4: Интеграция в MainWindow и оркестрация' (Protocol in workflow.md)

---

## Phase 5: Комплексное тестирование и верификация (Verification & Quality)

- [x] Task: Автоматическое тестирование и сборка
    - [x] Запустить полный набор модульных тестов (`dotnet test`)
    - [x] Проверить компиляцию решения без предупреждений (`dotnet build`)
- [x] Task: Комплексная ручная проверка функционала
    - [x] Проверить захват без аннотаций: клик `📌` открывает окно по центру монитора
    - [x] Проверить захват с аннотациями: рамки, стрелки и линии отображаются в окне
    - [x] Проверить мультиоконность и каскадный сдвиг (+24 px) при открытии второго окна
    - [x] Проверить отображение в панели задач и `Alt+Tab`
    - [x] Проверить отсутствие записей на диск, в буфер обмена и в историю SQLite
- [x] Task: Conductor - User Manual Verification 'Phase 5: Комплексное тестирование и верификация' (Protocol in workflow.md)
