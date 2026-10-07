# Implementation Plan — Опция автозапуска приложения при старте Windows

## Фаза 1: Базовая модель данных и сервис автозапуска (Core & TDD)
- [ ] Task: Обновление модели конфигурации AppSettings и словарей локализации
    - [ ] Добавить свойство `AutoStart` в `AppSettings.cs` со значением по умолчанию `false`
    - [ ] Добавить ключи локализации `Loc_AutoStart`, `Loc_AutoStartHelp`, `Loc_TrayAutoStart` в `LocalizationManager.cs` (ru и en)
    - [ ] Обновить модульные тесты `ConfigurationServiceTests.cs` для проверки сохранения и чтения свойства `AutoStart`
- [ ] Task: Разработка модульных тестов для AutoStartService (TDD Red Phase)
    - [ ] Описать интерфейс `IAutoStartService` (методы проверки, включения, выключения и синхронизации ярлыка)
    - [ ] Написать тесты в `AutoStartServiceTests.cs` с изолированной временной директорией Startup для проверки создания, валидации и удаления ярлыка
- [ ] Task: Реализация AutoStartService с генерацией .lnk ярлыка (TDD Green Phase)
    - [ ] Реализовать COM Interop (`IShellLinkW`, `IPersistFile`) для создания ярлыка Windows без сторонних зависимостей
    - [ ] Реализовать методы `IsAutoStartEnabled()`, `EnableAutoStart()`, `DisableAutoStart()`, `SetAutoStart(bool enable)` с обработкой ошибок и логированием
    - [ ] Зарегистрировать `IAutoStartService` в жизненном цикле приложения
    - [ ] Запустить `dotnet test` и убедиться в успешном прохождении тестов
- [ ] Task: Conductor - User Manual Verification 'Фаза 1: Базовая модель данных и сервис автозапуска' (Protocol in workflow.md)

## Фаза 2: Интеграция с пользовательским интерфейсом (SettingsWindow & Tray Menu)
- [ ] Task: Добавление опции автозапуска в окно настроек (SettingsWindow)
    - [ ] Добавить чекбокс `ChkAutoStart` в `SettingsWindow.xaml` с поддержкой динамической локализации
    - [ ] Реализовать загрузку текущего статуса и сохранение настройки в `SettingsWindow.xaml.cs`
- [ ] Task: Добавление переключателя автозапуска в меню трея (TrayIconManager)
    - [ ] Добавить интерактивный пункт с галочкой в контекстное меню трея в `TrayIconManager.cs`
    - [ ] Связать обработчик клика в трее с `IAutoStartService` и сохранением настроек в `IConfigurationService`
    - [ ] Обеспечить двустороннюю синхронизацию отметки между треем и окном настроек при изменении
- [ ] Task: Синхронизация при старте приложения (App.xaml.cs)
    - [ ] Добавить проверку актуальности ярлыка в каталоге автозагрузки при запуске приложения (на случай перемещения исполняемого файла)
    - [ ] Проверить запуск в фоновом режиме системного трея
- [ ] Task: Conductor - User Manual Verification 'Фаза 2: Интеграция с пользовательским интерфейсом' (Protocol in workflow.md)

## Фаза 3: Комплексное тестирование и проверка сценариев
- [ ] Task: Прогон автоматических тестов и сборка
    - [ ] Выполнить полный прогон `dotnet test` и убедиться в отсутствии ошибок и регрессий
    - [ ] Проверить отсутствие предупреждений компилятора (`dotnet build`)
- [ ] Task: Комплексная проверка сценариев пользователя
    - [ ] Проверить создание и удаление ярлыка в `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`
    - [ ] Проверить переключение языков (RU/EN) и сохранение состояния после перезапуска приложения
- [ ] Task: Conductor - User Manual Verification 'Фаза 3: Комплексное тестирование и проверка сценариев' (Protocol in workflow.md)

## Фаза 4: Выпуск релиза на GitHub (Release v1.1.0)
- [ ] Task: Подготовка коммита изменений трека
    - [ ] Проверить статус репозитория и отсутствие лишних файлов
    - [ ] Закоммитить изменения трека согласно протоколу коммитов с подробной сводкой изменений
- [ ] Task: Создание git-тега v1.1.0 и публикация релиза на GitHub
    - [ ] Создать git-тег `v1.1.0` с описанием релиза
    - [ ] Отправить тег на GitHub (`git push origin v1.1.0`)
    - [ ] Дождаться завершения workflow в GitHub Actions и верифицировать создание релиза и дистрибутива `ScreenTextCatcher-win-x64.zip`
- [ ] Task: Conductor - User Manual Verification 'Фаза 4: Выпуск релиза на GitHub' (Protocol in workflow.md)
