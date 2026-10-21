# Implementation Plan — Инструмент «Ручка» (Freehand Pen with Smoothing) и релиз v1.5.0

## Phase 1: Математика сглаживания и строковые ресурсы (Core & TDD)

- [ ] Task: Добавление строковых ресурсов локализации
    - [ ] Написать модульные тесты для проверки ключа `Loc_AnnotateToolbarPen` в `LocalizationManagerTests.cs`
    - [ ] Добавить ключ `Loc_AnnotateToolbarPen` для русской ("Ручка") и английской ("Pen") локализаций в `LocalizationManager.cs`
- [ ] Task: Реализация алгоритмов фильтрации и сглаживания (TDD)
    - [ ] Написать модульные тесты для `SimplifyPoints`, `CreateSmoothPathGeometry`, `CreatePenPath`, `UpdatePenPreview`, `FinalizePenStroke` в `AnnotationGeometryTests.cs`
    - [ ] Реализовать константы `DefaultPenSmoothingEpsilon = 2.5` и `MinPenPointDistance = 2.0` в `AnnotationGeometryHelper.cs`
    - [ ] Реализовать алгоритм Рамера — Дугласа — Пекера (`SimplifyPoints`) в `AnnotationGeometryHelper.cs`
    - [ ] Реализовать построение сглаженных сплайнов Безье (`CreateSmoothPathGeometry`) в `AnnotationGeometryHelper.cs`
    - [ ] Реализовать фабричные методы и методы обновления превью/финализации (`CreatePenPath`, `UpdatePenPreview`, `FinalizePenStroke`) в `AnnotationGeometryHelper.cs`
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Математика сглаживания и строковые ресурсы' (Protocol in workflow.md)

---

## Phase 2: Разметка тулбара и логика выбора инструмента (UI)

- [ ] Task: Разметка кнопки инструмента «Ручка» в тулбаре оверлея
    - [ ] Добавить кнопку `BtnToolPen` с иконкой `✏`, стилем `ToolbarButtonStyle` и тултипом `{DynamicResource Loc_AnnotateToolbarPen}` в `OverlayWindow.xaml` сразу после `BtnToolLine`
- [ ] Task: Обработка выбора и подсветки инструмента «Ручка»
    - [ ] Добавить значение `Pen` в перечисление `AnnotationTool` в `OverlayWindow.xaml.cs`
    - [ ] Реализовать обработчик `OnToolPenClick` с переключением режима (toggle)
    - [ ] Обновить метод `SetActiveTool` для управления подсветкой кнопки `BtnToolPen` и установкой курсора `Cursors.Cross`
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Разметка тулбара и логика выбора инструмента' (Protocol in workflow.md)

---

## Phase 3: Интерактивное рисование штрихов и пост-сглаживание (Interaction & Export)

- [ ] Task: Интерактивный сбор точек и превью траектории
    - [ ] Добавить поле `_currentPenPoints` в `OverlayWindow.xaml.cs`
    - [ ] Реализовать начало штриха в `OnCanvasMouseDown` с созданием `Path` через `CreatePenPath` и добавлением на холст `AnnotationCanvas`
    - [ ] Реализовать добавление точек с пространственным фильтром (≥ 2.0 px) и вызовом `UpdatePenPreview` в `OnCanvasMouseMove`
- [ ] Task: Завершение штриха, пост-сглаживание и отсечение мисскликов
    - [ ] Реализовать отсечение штрихов длиной < 3.0 px при `MouseUp` с удалением временной фигуры из холста
    - [ ] Реализовать вызов `FinalizePenStroke` для сглаживания и заморозки геометрии при успешном штрихе
    - [ ] Зарегистрировать штрих в стеке `_annotationHistory`, обновить состояние `BtnUndo.IsEnabled` и очистить буфер `_currentPenPoints`
- [ ] Task: Интеграция с отменой (Undo) и экспортом в высоком DPI
    - [ ] Проверить отмену нарисованных контуров ручки по клику на `BtnUndo` и хоткею `Ctrl+Z`
    - [ ] Проверить корректность экспорта векторных контуров в высоком DPI через `AnnotationExportHelper` (сохранение на диск, копирование в буфер `📋`, закрепление `📌`)
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Интерактивное рисование штрихов и пост-сглаживание' (Protocol in workflow.md)

---

## Phase 4: Финальное тестирование и комплексная верификация (Verification & Quality)

- [ ] Task: Автоматическое тестирование и проверка сборки
    - [ ] Запустить полный набор модульных тестов решения (`dotnet test`)
    - [ ] Проверить сборку решения без предупреждений компилятора (`dotnet build`)
- [ ] Task: Комплексная ручная верификация
    - [ ] Проверить работу тулбара, локализацию подсказки «Ручка»/«Pen» и смену курсора на перекрестие
    - [ ] Проверить рисование плавных рукописных линий без задержек и качество сглаживания при отпускании ЛКМ
    - [ ] Проверить отсечение коротких кликов (< 3px), отмену (Undo/Ctrl+Z) и экспорт в PNG
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Финальное тестирование и комплексная верификация' (Protocol in workflow.md)

---

## Phase 5: Выпуск релиза на GitHub (Release v1.5.0)

- [ ] Task: Подготовка коммита изменений трека и обновление версии
    - [ ] Обновить версию в `ScreenTextCatcher.csproj` и `ScreenTextCatcher.Core.csproj` до `1.5.0`
    - [ ] Закоммитить изменения трека согласно протоколу коммитов с подробной сводкой
- [ ] Task: Создание git-тега v1.5.0 и публикация релиза на GitHub
    - [ ] Создать git-тег `v1.5.0` с описанием релиза
    - [ ] Отправить тег на GitHub (`git push origin v1.5.0`)
    - [ ] Дождаться завершения GitHub Actions Release workflow и верифицировать создание дистрибутива `ScreenTextCatcher-win-x64.zip`
- [ ] Task: Conductor - User Manual Verification 'Phase 5: Выпуск релиза на GitHub' (Protocol in workflow.md)
