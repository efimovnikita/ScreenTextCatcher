# Implementation Plan — Исправление позиционирования панели инструментов оверлея

## Phase 1: Доработка алгоритма позиционирования панели и модульные тесты (TDD)

- [x] Task: Разработка модульных тестов для граничных условий (`ToolbarPositioningTests.cs`)
    - [x] Написать тест `CalculatePosition_RightEdgeSelection_ClampsWithMargin` на проверку фиксации панели с отступом 8px от правого края экрана
    - [x] Написать тест `CalculatePosition_LeftEdgeSelection_ClampsWithMargin` на проверку отступа 8px от левого края экрана
    - [x] Написать тест `CalculatePosition_BottomEdgeSelection_FlipsAboveWithMargin` на перенос панели наверх с отступом 8px
    - [x] Написать тест `CalculatePosition_FullScreenSelection_ClampsToBottomWithMargin` на прижатие к нижнему краю монитора с отступом 8px при полноэкранном выделении
    - [x] Написать тест `CalculatePosition_MultiMonitorOffset_PositionsWithinSecondaryMonitor` на удержание панели внутри смещенных координат вторичного монитора
- [x] Task: Обновление логики `ToolbarPositioningHelper.cs`
    - [x] Обновить расчет горизонтальных границ `minX` и `maxX` с учетом отступа `margin` и применить `Math.Clamp(x, minX, maxX)`
    - [x] Обновить расчет экстремального вертикального позиционирования с учетом `margin` у верхнего и нижнего краев экрана
    - [x] Запустить `dotnet test` и убедиться в успешном прохождении всех тестов позиционирования
- [x] Task: Conductor - User Manual Verification 'Доработка алгоритма позиционирования панели и модульные тесты (TDD)' (Protocol in workflow.md)

---

## Phase 2: Интеграция в OverlayWindow и поддержка мультимониторов

- [x] Task: Определение границ монитора выделения в `OverlayWindow.xaml.cs`
    - [x] Реализовать метод `GetActiveMonitorCanvasBounds(Rect selectionRect)` для определения рабочего монитора и перевода координат в DIP канваса
- [x] Task: Коррекция замера и позиционирования панели инструментов в `OverlayWindow.xaml.cs`
    - [x] Установить `AnnotationToolbar.Visibility = Visibility.Visible` перед вызовом `Measure`
    - [x] Добавить проверку `DesiredSize` с fallback-значением `new Size(320, 42)`
    - [x] Передать вычисленные границы активного монитора `monitorBounds` в `ToolbarPositioningHelper.CalculatePosition`
    - [x] Проверить компиляцию решения `dotnet build` и прогон тестов `dotnet test`
- [x] Task: Conductor - User Manual Verification 'Интеграция в OverlayWindow и поддержка мультимониторов' (Protocol in workflow.md)

---

## Phase 3: Версионирование и подготовка хотфикс-релиза v1.2.1

- [x] Task: Обновление версий сборки проектов
    - [x] Обновить версию в `src/ScreenTextCatcher/ScreenTextCatcher.csproj` и `src/ScreenTextCatcher.Core/ScreenTextCatcher.Core.csproj` до `1.2.1`
    - [x] Проверить успешность сборки `dotnet build ScreenTextCatcher.sln` и прогона всех тестов `dotnet test`
- [x] Task: Создание релиза в Git и публикация на GitHub
    - [x] Создать аннотированный Git-тег `v1.2.1`
    - [x] Отправить изменения и тег в GitHub (`git push origin master --tags`) для запуска сборки релиза GitHub Actions
- [x] Task: Conductor - User Manual Verification 'Версионирование и подготовка хотфикс-релиза v1.2.1' (Protocol in workflow.md)
