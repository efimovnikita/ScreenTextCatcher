# Implementation Plan — Инструмент аннотации «Линия» (Line Annotation Tool) и релиз v1.3.0

## Phase 1: Подготовка хелперов геометрии и строковых ресурсов (Core & TDD)

- [ ] Task: Добавление строковых ресурсов локализации
    - [ ] Написать модульные тесты для проверки ключа `Loc_AnnotateToolbarLine` в `LocalizationManagerTests.cs`
    - [ ] Добавить ключ `Loc_AnnotateToolbarLine` для русской ("Линия") и английской ("Line") локализаций в `LocalizationManager.cs`
- [ ] Task: Реализация методов геометрии линии и привязки углов (TDD)
    - [ ] Написать модульные тесты для `SnapToAngle`, `CreateLine` и `UpdateLine` в `AnnotationGeometryTests.cs`
    - [ ] Реализовать метод `SnapToAngle(Point start, Point current, double stepDegrees = 45.0)` в `AnnotationGeometryHelper.cs`
    - [ ] Реализовать методы `CreateLine` и `UpdateLine` в `AnnotationGeometryHelper.cs`
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Подготовка хелперов геометрии и строковых ресурсов' (Protocol in workflow.md)

---

## Phase 2: Разметка тулбара и логика переключения инструментов (UI)

- [ ] Task: Разметка кнопки инструмента «Линия» в тулбаре
    - [ ] Добавить кнопку `BtnToolLine` с глифом `―` и тултипом в разметку `OverlayWindow.xaml` сразу после `BtnToolArrow`
- [ ] Task: Обработка выбора и подсветки инструмента «Линия»
    - [ ] Добавить значение `Line` в перечисление `AnnotationTool` в `OverlayWindow.xaml.cs`
    - [ ] Реализовать обработчик `OnToolLineClick` с переключением режима (toggle)
    - [ ] Обновить метод `SetActiveTool` для подсветки кнопки `BtnToolLine` и установки курсора `Cursors.Cross`
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Разметка тулбара и логика переключения инструментов' (Protocol in workflow.md)

---

## Phase 3: Интерактивное рисование линии и привязка по Shift (Interaction & Export)

- [ ] Task: Интерактивная отрисовка линии и порог клика
    - [ ] Реализовать инициализацию `Line` в `OnCanvasMouseDown`
    - [ ] Реализовать обновление координат линии в `OnCanvasMouseMove`
    - [ ] Реализовать отсечение коротких кликов (< 3px) и сохранение в `_annotationHistory` в `OnCanvasMouseUp`
- [ ] Task: Синхронная привязка углов по Shift (Angle Snapping)
    - [ ] Подключить проверку зажатия `Shift` и вызов `AnnotationGeometryHelper.SnapToAngle` для Линии в `OnCanvasMouseMove`
    - [ ] Подключить `AnnotationGeometryHelper.SnapToAngle` для Стрелки в `OnCanvasMouseMove` при зажатом `Shift`
- [ ] Task: Интеграция со стеком отмены (Undo) и экспортом в PNG
    - [ ] Проверить поддержку отмены линии через `BtnUndo` и `Ctrl+Z`
    - [ ] Проверить экспорт изображения с нарисованными линиями через `AnnotationExportHelper`
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Интерактивное рисование линии и привязка по Shift' (Protocol in workflow.md)

---

## Phase 4: Финальное тестирование и комплексная верификация

- [ ] Task: Сборка и автоматическое тестирование
    - [ ] Запустить полный набор тестов решения (`dotnet test`)
    - [ ] Проверить сборку решения без предупреждений и ошибок (`dotnet build`)
- [ ] Task: Комплексная ручная верификация
    - [ ] Проверить работу кнопки «Линия», тултипа, курсора и отмены клика
    - [ ] Проверить выравнивание угла 45° по Shift для Линии и Стрелки
    - [ ] Проверить сохранение и экспорт скриншота с нарисованными линиями
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Финальное тестирование и комплексная верификация' (Protocol in workflow.md)

---

## Phase 5: Выпуск релиза на GitHub (Release v1.3.0)

- [ ] Task: Подготовка коммита изменений трека и обновление версии
    - [ ] Обновить версию в `ScreenTextCatcher.csproj` и `ScreenTextCatcher.Core.csproj` до `1.3.0`
    - [ ] Закоммитить изменения трека согласно протоколу коммитов с подробной сводкой
- [ ] Task: Создание git-тега v1.3.0 и публикация релиза на GitHub
    - [ ] Создать git-тег `v1.3.0` с описанием релиза
    - [ ] Отправить тег на GitHub (`git push origin v1.3.0`)
    - [ ] Дождаться завершения GitHub Actions Release workflow и верифицировать создание дистрибутива `ScreenTextCatcher-win-x64.zip`
- [ ] Task: Conductor - User Manual Verification 'Phase 5: Выпуск релиза на GitHub' (Protocol in workflow.md)
