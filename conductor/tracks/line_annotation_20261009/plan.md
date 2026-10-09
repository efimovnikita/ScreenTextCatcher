# Implementation Plan — Инструмент аннотации «Линия» (Line Annotation Tool) и релиз v1.3.0

## Phase 1: Подготовка хелперов геометрии и строковых ресурсов (Core & TDD)

- [x] Task: Добавление строковых ресурсов локализации
    - [x] Написать модульные тесты для проверки ключа `Loc_AnnotateToolbarLine` в `LocalizationManagerTests.cs`
    - [x] Добавить ключ `Loc_AnnotateToolbarLine` для русской ("Линия") и английской ("Line") локализаций в `LocalizationManager.cs`
- [x] Task: Реализация методов геометрии линии и привязки углов (TDD)
    - [x] Написать модульные тесты для `SnapToAngle`, `CreateLine` и `UpdateLine` в `AnnotationGeometryTests.cs`
    - [x] Реализовать метод `SnapToAngle(Point start, Point current, double stepDegrees = 45.0)` в `AnnotationGeometryHelper.cs`
    - [x] Реализовать методы `CreateLine` и `UpdateLine` в `AnnotationGeometryHelper.cs`
- [x] Task: Conductor - User Manual Verification 'Phase 1: Подготовка хелперов геометрии и строковых ресурсов' (Protocol in workflow.md)

---

## Phase 2: Разметка тулбара и логика переключения инструментов (UI)

- [x] Task: Разметка кнопки инструмента «Линия» в тулбаре
    - [x] Добавить кнопку `BtnToolLine` с глифом `―` и тултипом в разметку `OverlayWindow.xaml` сразу после `BtnToolArrow`
- [x] Task: Обработка выбора и подсветки инструмента «Линия»
    - [x] Добавить значение `Line` в перечисление `AnnotationTool` в `OverlayWindow.xaml.cs`
    - [x] Реализовать обработчик `OnToolLineClick` с переключением режима (toggle)
    - [x] Обновить метод `SetActiveTool` для подсветки кнопки `BtnToolLine` и установки курсора `Cursors.Cross`
- [x] Task: Conductor - User Manual Verification 'Phase 2: Разметка тулбара и логика переключения инструментов' (Protocol in workflow.md)

---

## Phase 3: Интерактивное рисование линии и привязка по Shift (Interaction & Export)

- [x] Task: Интерактивная отрисовка линии и порог клика
    - [x] Реализовать инициализацию `Line` в `OnCanvasMouseDown`
    - [x] Реализовать обновление координат линии в `OnCanvasMouseMove`
    - [x] Реализовать отсечение коротких кликов (< 3px) и сохранение в `_annotationHistory` в `OnCanvasMouseUp`
- [x] Task: Синхронная привязка углов по Shift (Angle Snapping)
    - [x] Подключить проверку зажатия `Shift` и вызов `AnnotationGeometryHelper.SnapToAngle` для Линии в `OnCanvasMouseMove`
    - [x] Подключить `AnnotationGeometryHelper.SnapToAngle` для Стрелки в `OnCanvasMouseMove` при зажатом `Shift`
- [x] Task: Интеграция со стеком отмены (Undo) и экспортом в PNG
    - [x] Проверить поддержку отмены линии через `BtnUndo` и `Ctrl+Z`
    - [x] Проверить экспорт изображения с нарисованными линиями через `AnnotationExportHelper`
- [x] Task: Conductor - User Manual Verification 'Phase 3: Интерактивное рисование линии и привязка по Shift' (Protocol in workflow.md)

---

## Phase 4: Финальное тестирование и комплексная верификация

- [x] Task: Сборка и автоматическое тестирование
    - [x] Запустить полный набор тестов решения (`dotnet test`)
    - [x] Проверить сборку решения без предупреждений и ошибок (`dotnet build`)
- [x] Task: Комплексная ручная верификация
    - [x] Проверить работу кнопки «Линия», тултипа, курсора и отмены клика
    - [x] Проверить выравнивание угла 45° по Shift для Линии и Стрелки
    - [x] Проверить сохранение и экспорт скриншота с нарисованными линиями
- [x] Task: Conductor - User Manual Verification 'Phase 4: Финальное тестирование и комплексная верификация' (Protocol in workflow.md)

---

## Phase 5: Выпуск релиза на GitHub (Release v1.3.0)

- [x] Task: Подготовка коммита изменений трека и обновление версии
    - [x] Обновить версию в `ScreenTextCatcher.csproj` и `ScreenTextCatcher.Core.csproj` до `1.3.0`
    - [x] Закоммитить изменения трека согласно протоколу коммитов с подробной сводкой
- [x] Task: Создание git-тега v1.3.0 и публикация релиза на GitHub
    - [x] Создать git-тег `v1.3.0` с описанием релиза
    - [x] Отправить тег на GitHub (`git push origin v1.3.0`)
    - [x] Дождаться завершения GitHub Actions Release workflow и верифицировать создание дистрибутива `ScreenTextCatcher-win-x64.zip`
- [x] Task: Conductor - User Manual Verification 'Phase 5: Выпуск релиза на GitHub' (Protocol in workflow.md)
