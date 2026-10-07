# Implementation Plan — Исправление позиционирования панели инструментов оверлея

## Phase 1: Доработка алгоритма позиционирования панели и модульные тесты (TDD)

- [ ] Task: Разработка модульных тестов для граничных условий (`ToolbarPositioningTests.cs`)
    - [ ] Написать тест `CalculatePosition_RightEdgeSelection_ClampsWithMargin` на проверку фиксации панели с отступом 8px от правого края экрана
    - [ ] Написать тест `CalculatePosition_LeftEdgeSelection_ClampsWithMargin` на проверку отступа 8px от левого края экрана
    - [ ] Написать тест `CalculatePosition_BottomEdgeSelection_FlipsAboveWithMargin` на перенос панели наверх с отступом 8px
    - [ ] Написать тест `CalculatePosition_FullScreenSelection_ClampsToBottomWithMargin` на прижатие к нижнему краю монитора с отступом 8px при полноэкранном выделении
    - [ ] Написать тест `CalculatePosition_MultiMonitorOffset_PositionsWithinSecondaryMonitor` на удержание панели внутри смещенных координат вторичного монитора
- [ ] Task: Обновление логики `ToolbarPositioningHelper.cs`
    - [ ] Обновить расчет горизонтальных границ `minX` и `maxX` с учетом отступа `margin` и применить `Math.Clamp(x, minX, maxX)`
    - [ ] Обновить расчет экстремального вертикального позиционирования с учетом `margin` у верхнего и нижнего краев экрана
    - [ ] Запустить `dotnet test` и убедиться в успешном прохождении всех тестов позиционирования
- [ ] Task: Conductor - User Manual Verification 'Доработка алгоритма позиционирования панели и модульные тесты (TDD)' (Protocol in workflow.md)

---

## Phase 2: Интеграция в OverlayWindow и поддержка мультимониторов

- [ ] Task: Определение границ монитора выделения в `OverlayWindow.xaml.cs`
    - [ ] Реализовать метод `GetActiveMonitorCanvasBounds(Rect selectionRect)` для определения рабочего монитора и перевода координат в DIP канваса
- [ ] Task: Коррекция замера и позиционирования панели инструментов в `OverlayWindow.xaml.cs`
    - [ ] Установить `AnnotationToolbar.Visibility = Visibility.Visible` перед вызовом `Measure`
    - [ ] Добавить проверку `DesiredSize` с fallback-значением `new Size(320, 42)`
    - [ ] Передать вычисленные границы активного монитора `monitorBounds` в `ToolbarPositioningHelper.CalculatePosition`
    - [ ] Проверить компиляцию решения `dotnet build` и прогон тестов `dotnet test`
- [ ] Task: Conductor - User Manual Verification 'Интеграция в OverlayWindow и поддержка мультимониторов' (Protocol in workflow.md)

---

## Phase 3: Версионирование и подготовка хотфикс-релиза v1.2.1

- [ ] Task: Обновление версий сборки проектов
    - [ ] Обновить версию в `src/ScreenTextCatcher/ScreenTextCatcher.csproj` и `src/ScreenTextCatcher.Core/ScreenTextCatcher.Core.csproj` до `1.2.1`
    - [ ] Проверить успешность сборки `dotnet build ScreenTextCatcher.sln` и прогона всех тестов `dotnet test`
- [ ] Task: Создание релиза в Git и публикация на GitHub
    - [ ] Создать аннотированный Git-тег `v1.2.1`
    - [ ] Отправить изменения и тег в GitHub (`git push origin master --tags`) для запуска сборки релиза GitHub Actions
- [ ] Task: Conductor - User Manual Verification 'Версионирование и подготовка хотфикс-релиза v1.2.1' (Protocol in workflow.md)
