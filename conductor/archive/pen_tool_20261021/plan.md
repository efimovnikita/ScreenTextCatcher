# Implementation Plan — Инструмент «Ручка» (Freehand Pen with Smoothing) и релиз v1.5.0

## Phase 1: Математика сглаживания и строковые ресурсы (Core & TDD)

- [x] Task: Добавление строковых ресурсов локализации
    - [x] Написать модульные тесты для проверки ключа `Loc_AnnotateToolbarPen` в `LocalizationManagerTests.cs`
    - [x] Добавить ключ `Loc_AnnotateToolbarPen` для русской ("Ручка") и английской ("Pen") локализаций в `LocalizationManager.cs`
- [x] Task: Реализация алгоритмов фильтрации и сглаживания (TDD)
    - [x] Написать модульные тесты для `SimplifyPoints`, `CreateSmoothPathGeometry`, `CreatePenPath`, `UpdatePenPreview`, `FinalizePenStroke` в `AnnotationGeometryTests.cs`
    - [x] Реализовать константы `DefaultPenSmoothingEpsilon = 2.5` и `MinPenPointDistance = 2.0` в `AnnotationGeometryHelper.cs`
    - [x] Реализовать алгоритм Рамера — Дугласа — Пекера (`SimplifyPoints`) в `AnnotationGeometryHelper.cs`
    - [x] Реализовать построение сглаженных сплайнов Безье (`CreateSmoothPathGeometry`) в `AnnotationGeometryHelper.cs`
    - [x] Реализовать фабричные методы и методы обновления превью/финализации (`CreatePenPath`, `UpdatePenPreview`, `FinalizePenStroke`) в `AnnotationGeometryHelper.cs`
- [x] Task: Conductor - User Manual Verification 'Phase 1: Математика сглаживания и строковые ресурсы' (Protocol in workflow.md)

---

## Phase 2: Разметка тулбара и логика выбора инструмента (UI)

- [x] Task: Разметка кнопки инструмента «Ручка» в тулбаре оверлея
    - [x] Добавить кнопку `BtnToolPen` с иконкой `✏`, стилем `ToolbarButtonStyle` и тултипом `{DynamicResource Loc_AnnotateToolbarPen}` в `OverlayWindow.xaml` сразу после `BtnToolLine`
- [x] Task: Обработка выбора и подсветки инструмента «Ручка»
    - [x] Добавить значение `Pen` в перечисление `AnnotationTool` в `OverlayWindow.xaml.cs`
    - [x] Реализовать обработчик `OnToolPenClick` с переключением режима (toggle)
    - [x] Обновить метод `SetActiveTool` для управления подсветкой кнопки `BtnToolPen` и установкой курсора `Cursors.Cross`
- [x] Task: Conductor - User Manual Verification 'Phase 2: Разметка тулбара и логика выбора инструмента' (Protocol in workflow.md)

---

## Phase 3: Интерактивное рисование штрихов и пост-сглаживание (Interaction & Export)

- [x] Task: Интерактивный сбор точек и превью траектории
    - [x] Добавить поле `_currentPenPoints` в `OverlayWindow.xaml.cs`
    - [x] Реализовать начало штриха в `OnCanvasMouseDown` с созданием `Path` через `CreatePenPath` и добавлением на холст `AnnotationCanvas`
    - [x] Реализовать добавление точек с пространственным фильтром (≥ 2.0 px) и вызовом `UpdatePenPreview` в `OnCanvasMouseMove`
- [x] Task: Завершение штриха, пост-сглаживание и отсечение мисскликов
    - [x] Реализовать отсечение штрихов длиной < 3.0 px при `MouseUp` с удалением временной фигуры из холста
    - [x] Реализовать вызов `FinalizePenStroke` для сглаживания и заморозки геометрии при успешном штрихе
    - [x] Зарегистрировать штрих в стеке `_annotationHistory`, обновить состояние `BtnUndo.IsEnabled` и очистить буфер `_currentPenPoints`
- [x] Task: Интеграция с отменой (Undo) и экспортом в высоком DPI
    - [x] Проверить отмену нарисованных контуров ручки по клику на `BtnUndo` и хоткею `Ctrl+Z`
    - [x] Проверить корректность экспорта векторных контуров в высоком DPI через `AnnotationExportHelper` (сохранение на диск, копирование в буфер `📋`, закрепление `📌`)
- [x] Task: Conductor - User Manual Verification 'Phase 3: Интерактивное рисование штрихов и пост-сглаживание' (Protocol in workflow.md)

---

## Phase 4: Финальное тестирование и комплексная верификация (Verification & Quality)

- [x] Task: Автоматическое тестирование и проверка сборки
    - [x] Запустить полный набор модульных тестов решения (`dotnet test`)
    - [x] Проверить сборку решения без предупреждений компилятора (`dotnet build`)
- [x] Task: Комплексная ручная верификация
    - [x] Проверить работу тулбара, локализацию подсказки «Ручка»/«Pen» и смену курсора на перекрестие
    - [x] Проверить рисование плавных рукописных линий без задержек и качество сглаживания при отпускании ЛКМ
    - [x] Проверить отсечение коротких кликов (< 3px), отмену (Undo/Ctrl+Z) и экспорт в PNG
- [x] Task: Conductor - User Manual Verification 'Phase 4: Финальное тестирование и комплексная верификация' (Protocol in workflow.md)

---

## Phase 5: Выпуск релиза на GitHub (Release v1.5.0)

- [x] Task: Подготовка коммита изменений трека и обновление версии
    - [x] Обновить версию в `ScreenTextCatcher.csproj` и `ScreenTextCatcher.Core.csproj` до `1.5.0`
    - [x] Закоммитить изменения трека согласно протоколу коммитов с подробной сводкой
- [x] Task: Создание git-тега v1.5.0 и публикация релиза на GitHub
    - [x] Создать git-тег `v1.5.0` с описанием релиза
    - [x] Отправить тег на GitHub (`git push origin v1.5.0`)
    - [x] Дождаться завершения GitHub Actions Release workflow и верифицировать создание дистрибутива `ScreenTextCatcher-win-x64.zip`
- [x] Task: Conductor - User Manual Verification 'Phase 5: Выпуск релиза на GitHub' (Protocol in workflow.md)
