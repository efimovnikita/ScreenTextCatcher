# Implementation Plan — Внедрение бренд-комплекта иконок приложения и системного трея

## Фаза 1: Генерация концепта и создание мастер-графики (AI Generation & Design Approval)
- [ ] Task: Генерация вариантов концепта иконки через AI
    - [ ] Составить детализированный графический промпт для `image-generator` субагента (видоискатель, угловые метки, литера «T», градиент Mistral AI янтарный/индиго, темный/прозрачный фон)
    - [ ] Запустить генерацию вариантов концепта через `image-generator`
    - [ ] Сохранить сгенерированные изображения в рабочей директории и подготовить превью для согласования
- [ ] Task: Согласование концепта с пользователем и финализация мастер-изображения
    - [ ] Предоставить варианты пользователю для выбора финального концепта
    - [ ] Сохранить выбранный мастер-ассет как `src/ScreenTextCatcher/Assets/app.png` с прозрачным фоном и правильным масштабированием
- [ ] Task: Conductor - User Manual Verification 'Фаза 1: Генерация концепта и создание мастер-графики' (Protocol in workflow.md)

## Фаза 2: Подготовка и сборка многослойных .ico ассетов (Asset Pipeline)
- [ ] Task: Генерация цветовых модификаций для системного трея
    - [ ] Создать версию для состояния Idle (фирменный оранжево-фиолетовый градиент)
    - [ ] Создать версию для состояния Processing (янтарно-золотистая гамма)
    - [ ] Создать версию для состояния Error (ало-красная гамма)
- [ ] Task: Компиляция мастер-иконки приложения и иконок трея в формат .ico
    - [ ] Собрать `src/ScreenTextCatcher/Assets/app.ico` со всеми стандартными слоями Windows (16, 20, 24, 32, 48, 64, 128, 256 px)
    - [ ] Собрать `src/ScreenTextCatcher/Assets/tray_idle.ico` (слои 16x16, 24x24, 32x32)
    - [ ] Собрать `src/ScreenTextCatcher/Assets/tray_busy.ico` (слои 16x16, 24x24, 32x32)
    - [ ] Собрать `src/ScreenTextCatcher/Assets/tray_error.ico` (слои 16x16, 24x24, 32x32)
    - [ ] Проверить целостность и валидность всех собранных `.ico` файлов
- [ ] Task: Conductor - User Manual Verification 'Фаза 2: Подготовка и сборка многослойных .ico ассетов' (Protocol in workflow.md)

## Фаза 3: Интеграция иконок в проект и рефакторинг TrayIconManager (Core & UI Integration)
- [ ] Task: Конфигурация проекта ScreenTextCatcher.csproj и окон приложения
    - [ ] Настроить `<ApplicationIcon>Assets\app.ico</ApplicationIcon>` в `ScreenTextCatcher.csproj`
    - [ ] Включить иконки в ресурсы сборки (`<Resource Include="Assets\*.ico" />`)
    - [ ] Повысить версию проекта до `1.2.2` в `ScreenTextCatcher.csproj`
    - [ ] Указать `Icon="Assets/app.ico"` в разметке окон (`MainWindow.xaml`, `SettingsWindow.xaml`, `HistoryWindow.xaml`, `LogViewerWindow.xaml`)
- [ ] Task: Рефакторинг TrayIconManager для загрузки встроенных .ico ресурсов
    - [ ] Заменить метод `GenerateIcons()` и процедурный `CreateSolidColorIcon()` на загрузку иконок из ресурсов сборки
    - [ ] Обеспечить безопасную загрузку ресурсов и фоллбэк при отсутствии ресурса
    - [ ] Проверить корректное освобождение ресурсов `Icon` в методе `Dispose()`
- [ ] Task: Модульное тестирование и проверка сборки
    - [ ] Добавить или обновить модульные тесты для проверки корректности инициализации и работы с ресурсами иконок трея
    - [ ] Запустить `dotnet test` и убедиться в успешном прохождении всех тестов
    - [ ] Выполнить `dotnet build` и проверить отсутствие ошибок и предупреждений
- [ ] Task: Conductor - User Manual Verification 'Фаза 3: Интеграция иконок в проект и рефакторинг TrayIconManager' (Protocol in workflow.md)

## Фаза 4: Комплексная верификация и релиз на GitHub (Release v1.2.2)
- [ ] Task: Комплексная визуальная и функциональная проверка
    - [ ] Проверить отображение иконки исполняемого файла `ScreenTextCatcher.exe` в Проводнике и на панели задач
    - [ ] Проверить отображение иконки в заголовках окон `MainWindow`, `SettingsWindow`, `HistoryWindow`, `LogViewerWindow`
    - [ ] Проверить отображение и смену иконок в системном трее (Idle, Processing, Error)
- [ ] Task: Фиксация коммита трека и создание релиза на GitHub
    - [ ] Закоммитить изменения трека с подробной сводкой согласно правилам `workflow.md`
    - [ ] Создать git-тег `v1.2.2` и отправить в репозиторий GitHub (`git push origin v1.2.2`)
    - [ ] Верифицировать успешное выполнение GitHub Actions Release workflow и публикацию архива `ScreenTextCatcher-win-x64.zip`
- [ ] Task: Conductor - User Manual Verification 'Фаза 4: Комплексная верификация и релиз на GitHub' (Protocol in workflow.md)
