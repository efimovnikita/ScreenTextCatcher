# Implementation Plan — Внедрение бренд-комплекта иконок приложения и системного трея

## Фаза 1: Генерация концепта и создание мастер-графики (AI Generation & Design Approval)
- [x] Task: Генерация вариантов концепта иконки через AI
    - [x] Составить детализированный графический промпт для `image-generator` субагента (видоискатель, угловые метки, литера «T», градиент Mistral AI янтарный/индиго, темный/прозрачный фон)
    - [x] Запустить генерацию вариантов концепта через `image-generator`
    - [x] Сохранить сгенерированные изображения в рабочей директории и подготовить превью для согласования
- [x] Task: Согласование концепта с пользователем и финализация мастер-изображения
    - [x] Предоставить варианты пользователю для выбора финального концепта
    - [x] Сохранить выбранный мастер-ассет как `src/ScreenTextCatcher/Assets/app.png` с прозрачным фоном и правильным масштабированием
- [x] Task: Conductor - User Manual Verification 'Фаза 1: Генерация концепта и создание мастер-графики' (Protocol in workflow.md)

## Фаза 2: Подготовка и сборка многослойных .ico ассетов (Asset Pipeline)
- [x] Task: Генерация цветовых модификаций для системного трея
    - [x] Создать версию для состояния Idle (фирменный оранжево-фиолетовый градиент)
    - [x] Создать версию для состояния Processing (янтарно-золотистая гамма)
    - [x] Создать версию для состояния Error (ало-красная гамма)
- [x] Task: Компиляция мастер-иконки приложения и иконок трея в формат .ico
    - [x] Собрать `src/ScreenTextCatcher/Assets/app.ico` со всеми стандартными слоями Windows (16, 20, 24, 32, 48, 64, 128, 256 px)
    - [x] Собрать `src/ScreenTextCatcher/Assets/tray_idle.ico` (слои 16x16, 24x24, 32x32)
    - [x] Собрать `src/ScreenTextCatcher/Assets/tray_busy.ico` (слои 16x16, 24x24, 32x32)
    - [x] Собрать `src/ScreenTextCatcher/Assets/tray_error.ico` (слои 16x16, 24x24, 32x32)
    - [x] Проверить целостность и валидность всех собранных `.ico` файлов
- [x] Task: Conductor - User Manual Verification 'Фаза 2: Подготовка и сборка многослойных .ico ассетов' (Protocol in workflow.md)

## Фаза 3: Интеграция иконок в проект и рефакторинг TrayIconManager (Core & UI Integration)
- [x] Task: Конфигурация проекта ScreenTextCatcher.csproj и окон приложения
    - [x] Настроить `<ApplicationIcon>Assets\app.ico</ApplicationIcon>` в `ScreenTextCatcher.csproj`
    - [x] Включить иконки в ресурсы сборки (`<Resource Include="Assets\*.ico" />`)
    - [x] Повысить версию проекта до `1.2.2` в `ScreenTextCatcher.csproj`
    - [x] Указать `Icon="Assets/app.ico"` в разметке окон (`MainWindow.xaml`, `SettingsWindow.xaml`, `HistoryWindow.xaml`, `LogViewerWindow.xaml`)
- [x] Task: Рефакторинг TrayIconManager для загрузки встроенных .ico ресурсов
    - [x] Заменить метод `GenerateIcons()` и процедурный `CreateSolidColorIcon()` на загрузку иконок из ресурсов сборки
    - [x] Обеспечить безопасную загрузку ресурсов и фоллбэк при отсутствии ресурса
    - [x] Проверить корректное освобождение ресурсов `Icon` в методе `Dispose()`
- [x] Task: Модульное тестирование и проверка сборки
    - [x] Добавить или обновить модульные тесты для проверки корректности инициализации и работы с ресурсами иконок трея
    - [x] Запустить `dotnet test` и убедиться в успешном прохождении всех тестов
    - [x] Выполнить `dotnet build` и проверить отсутствие ошибок и предупреждений
- [x] Task: Conductor - User Manual Verification 'Фаза 3: Интеграция иконок в проект и рефакторинг TrayIconManager' (Protocol in workflow.md)

## Фаза 4: Комплексная верификация и релиз на GitHub (Release v1.2.2)
- [x] Task: Комплексная визуальная и функциональная проверка
    - [x] Проверить отображение иконки исполняемого файла `ScreenTextCatcher.exe` в Проводнике и на панели задач
    - [x] Проверить отображение иконки в заголовках окон `MainWindow`, `SettingsWindow`, `HistoryWindow`, `LogViewerWindow`
    - [x] Проверить отображение и смену иконок в системном трее (Idle, Processing, Error)
- [x] Task: Фиксация коммита трека и создание релиза на GitHub
    - [x] Закоммитить изменения трека с подробной сводкой согласно правилам `workflow.md`
    - [x] Создать git-тег `v1.2.2` и отправить в репозиторий GitHub (`git push origin v1.2.2`)
    - [x] Верифицировать успешное выполнение GitHub Actions Release workflow и публикацию архива `ScreenTextCatcher-win-x64.zip`
- [x] Task: Conductor - User Manual Verification 'Фаза 4: Комплексная верификация и релиз на GitHub' (Protocol in workflow.md)
