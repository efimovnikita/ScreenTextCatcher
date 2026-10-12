# Implementation Plan — Окно просмотра захваченной области («Pin / Закрепить»)

## Phase 1: Локализация и вспомогательные сервисы позиционирования (Core & TDD)

- [ ] Task: Строковые ресурсы локализации (TDD)
    - [ ] Написать модульные тесты для новых ключей `Loc_AnnotateToolbarPinTooltip` и `Loc_PinnedViewerTitle` в `LocalizationManagerTests.cs`
    - [ ] Добавить ключи `Loc_AnnotateToolbarPinTooltip` и `Loc_PinnedViewerTitle` в `LocalizationManager.cs` для русской и английской локалей
- [ ] Task: Хелпер позиционирования и каскадирования окон (TDD)
    - [ ] Написать модульные тесты для расчета размеров окна и центрирования/каскадирования с учетом рабочей области и DPI
    - [ ] Реализовать логику расчета координат и каскадного смещения (+24 px) в хелпере позиционирования
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Локализация и вспомогательные сервисы позиционирования' (Protocol in workflow.md)

---

## Phase 2: Создание окна PinnedImageViewerWindow (UI & Window Logic)

- [ ] Task: Разметка PinnedImageViewerWindow.xaml
    - [ ] Создать файл `PinnedImageViewerWindow.xaml` со стилем `SingleBorderWindow`, `ResizeMode="CanResize"`, темным фоном `#212121`, иконкой `/Assets/app.ico` и контейнером `ScrollViewer` + `Image`
- [ ] Task: Логика PinnedImageViewerWindow.xaml.cs
    - [ ] Реализовать конструктор с загрузкой PNG байт в `BitmapImage` (`CacheOption = BitmapCacheOption.OnLoad`, `Freeze()`)
    - [ ] Реализовать определение монитора захвата через `Screen.FromRectangle` и центрирование с ограничением 90% WorkingArea и каскадированием
    - [ ] Установить локализованный заголовок окна с фактическим разрешением кадра
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Создание окна PinnedImageViewerWindow' (Protocol in workflow.md)

---

## Phase 3: Добавление кнопки в OverlayWindow и передача события (UI & Export)

- [ ] Task: Разметка кнопки закрепления в OverlayWindow.xaml
    - [ ] Добавить кнопку `BtnPin` с иконкой `📌`, стилем `ToolbarButtonStyle` и тултипом перед `BtnCopyImage`
- [ ] Task: Логика события закрепления в OverlayWindow.xaml.cs
    - [ ] Объявить публичное событие `public event Action<byte[], Rect>? PinRequested;`
    - [ ] Реализовать обработчик `OnPinClick`, выполняющий композитный экспорт изображения с аннотациями в PNG `byte[]`, закрытие оверлея и вызов события `PinRequested`
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Добавление кнопки в OverlayWindow и передача события' (Protocol in workflow.md)

---

## Phase 4: Интеграция в MainWindow и оркестрация (Integration)

- [ ] Task: Подписка на событие PinRequested в MainWindow.xaml.cs
    - [ ] Подписаться на `overlay.PinRequested` в методе `StartOcrCapture`
    - [ ] Реализовать создание и отображение `PinnedImageViewerWindow` в `Dispatcher.Invoke`
    - [ ] Обеспечить сброс статуса трея в `TrayStatus.Idle`, логирование события через `Log.Information` и восстановление дашборда при необходимости
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Интеграция в MainWindow и оркестрация' (Protocol in workflow.md)

---

## Phase 5: Комплексное тестирование и верификация (Verification & Quality)

- [ ] Task: Автоматическое тестирование и сборка
    - [ ] Запустить полный набор модульных тестов (`dotnet test`)
    - [ ] Проверить компиляцию решения без предупреждений (`dotnet build ScreenTextCatcher.sln`)
- [ ] Task: Комплексная ручная проверка функционала
    - [ ] Проверить захват без аннотаций: клик `📌` открывает окно по центру монитора
    - [ ] Проверить захват с аннотациями: рамки, стрелки и линии отображаются в окне
    - [ ] Проверить мультиоконность и каскадный сдвиг (+24 px) при открытии второго окна
    - [ ] Проверить отображение в панели задач и `Alt+Tab`
    - [ ] Проверить отсутствие записей на диск, в буфер обмена и в историю SQLite
- [ ] Task: Conductor - User Manual Verification 'Phase 5: Комплексное тестирование и верификация' (Protocol in workflow.md)
