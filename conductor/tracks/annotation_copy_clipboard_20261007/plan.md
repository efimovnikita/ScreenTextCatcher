# Implementation Plan — Кнопка копирования изображения в буфер обмена на панели аннотаций

## Phase 1: Локализация и расширение сервиса скриншотов

- [ ] Task: Добавление ресурсов локализации
    - [ ] Написать модульные тесты для новых ключей локализации (`LocalizationManagerTests.cs`)
    - [ ] Добавить строковые ключи `Loc_AnnotateToolbarCopy`, `Loc_AnnotateToolbarCopyTooltip`, `Loc_LogScreenshotImageCopied` в словари русской и английской локализации в `LocalizationManager.cs`
- [ ] Task: Расширение `IScreenshotService` и реализация в `ScreenshotService` (TDD)
    - [ ] Написать модульные тесты на метод `SaveAndCopyImageToClipboard` с проверкой сохранения файла, вызова `imageClipboardSetter` с байтами PNG и возврата пути (`ScreenshotServiceTests.cs`)
    - [ ] Добавить метод `string SaveAndCopyImageToClipboard(byte[] pngBytes, string folderPath)` в `IScreenshotService`
    - [ ] Реализовать `SaveAndCopyImageToClipboard` в `ScreenshotService` с поддержкой внедрения делегата `imageClipboardSetter` в конструктор и реализацией по умолчанию `SetImageClipboardWithRetry` (до 5 попыток с задержкой 50 мс, форматы `BitmapSource` и `PNG`)
- [ ] Task: Conductor - User Manual Verification 'Локализация и расширение сервиса скриншотов' (Protocol in workflow.md)

---

## Phase 2: Пользовательский интерфейс и разметка панели аннотаций

- [ ] Task: Разметка кнопки копирования на панели аннотаций (`OverlayWindow.xaml`)
    - [ ] Добавить кнопку `BtnCopyImage` со стилем `ToolbarButtonStyle` непосредственно перед кнопкой `BtnDone` с иконкой `📋`, текстом `{DynamicResource Loc_AnnotateToolbarCopy}` и тултипом `{DynamicResource Loc_AnnotateToolbarCopyTooltip}`
- [ ] Task: Логика взаимодействия и событие экспорта скриншота (`OverlayWindow.xaml.cs`)
    - [ ] Обновить событие `ScreenshotReady` с параметром `Action<byte[], bool>? ScreenshotReady` (`copyImageToClipboard`)
    - [ ] Добавить обработчик клика `OnCopyImageClick` с вызовом `FinishAndExportScreenshot(copyImageToClipboard: true)`
    - [ ] Сохранить передачу `copyImageToClipboard: false` при клике по `BtnDone` и нажатии `Enter`
- [ ] Task: Conductor - User Manual Verification 'Пользовательский интерфейс и разметка панели аннотаций' (Protocol in workflow.md)

---

## Phase 3: Интеграция с MainWindow, звуковая индикация и сквозное тестирование

- [ ] Task: Интеграция обработки копирования в MainWindow (`MainWindow.xaml.cs`)
    - [ ] Обновить подписку `overlay.ScreenshotReady` в `MainWindow.xaml.cs`: при `copyImageToClipboard == true` вызывать `SaveAndCopyImageToClipboard`, воспроизводить системный звук при `SoundFeedback == true` и логировать факт копирования
    - [ ] Обеспечить неизменность логики аккумуляции текстовых путей при `copyImageToClipboard == false`
- [ ] Task: Комплексное тестирование и валидация сквозного сценария
    - [ ] Написать интеграционные тесты для проверки изолированного ветвления копирования картинки против аккумуляции текстовых путей
    - [ ] Выполнить полный прогон набора тестов решения `dotnet test` и проверить отсутствие ошибок компиляции
- [ ] Task: Conductor - User Manual Verification 'Интеграция с MainWindow, звуковая индикация и сквозное тестирование' (Protocol in workflow.md)

---

## Phase 4: Подготовка релиза и публикация новой версии (v1.2.0)

- [ ] Task: Обновление версий сборки и синхронизация проектов
    - [ ] Обновить версию в `src/ScreenTextCatcher/ScreenTextCatcher.csproj` и `src/ScreenTextCatcher.Core/ScreenTextCatcher.Core.csproj` до `1.2.0`
    - [ ] Проверить успешность финальной сборки `dotnet build` и тестов `dotnet test`
- [ ] Task: Создание релиза в Git и триггер публикации на GitHub
    - [ ] Создать аннотированный Git-тег `v1.2.0`
    - [ ] Отправить изменения и тег в GitHub (`git push origin master --tags`) для автоматического запуска GitHub Actions сборки релиза `ScreenTextCatcher.exe`
- [ ] Task: Conductor - User Manual Verification 'Подготовка релиза и публикация новой версии (v1.2.0)' (Protocol in workflow.md)
