# Implementation Plan — Опция автозапуска приложения при старте Windows

## Фаза 1: Базовая модель данных и сервис автозапуска (Core & TDD)
- [x] Task: Обновление модели конфигурации AppSettings и словарей локализации
    - [x] Добавить свойство `AutoStart` в `AppSettings.cs` со значением по умолчанию `false`
    - [x] Добавить ключи локализации `Loc_AutoStart`, `Loc_AutoStartHelp`, `Loc_TrayAutoStart` в `LocalizationManager.cs` (ru и en)
    - [x] Обновить модульные тесты `ConfigurationServiceTests.cs` для проверки сохранения и чтения свойства `AutoStart`
- [x] Task: Разработка модульных тестов для AutoStartService (TDD Red Phase)
    - [x] Описать интерфейс `IAutoStartService` (методы проверки, включения, выключения и синхронизации ярлыка)
    - [x] Написать тесты в `AutoStartServiceTests.cs` с изолированной временной директорией Startup для проверки создания, валидации и удаления ярлыка
- [x] Task: Реализация AutoStartService с генерацией .lnk ярлыка (TDD Green Phase)
    - [x] Реализовать COM Interop (`IShellLinkW`, `IPersistFile`) для создания ярлыка Windows без сторонних зависимостей
    - [x] Реализовать методы `IsAutoStartEnabled()`, `EnableAutoStart()`, `DisableAutoStart()`, `SetAutoStart(bool enable)` с обработкой ошибок и логированием
    - [x] Зарегистрировать `IAutoStartService` в жизненном цикле приложения
    - [x] Запустить `dotnet test` и убедиться в успешном прохождении тестов
- [x] Task: Conductor - User Manual Verification 'Фаза 1: Базовая модель данных и сервис автозапуска' (Protocol in workflow.md)

## Фаза 2: Интеграция с пользовательским интерфейсом (SettingsWindow & Tray Menu)
- [x] Task: Добавление опции автозапуска в окно настроек (SettingsWindow)
    - [x] Добавить чекбокс `ChkAutoStart` в `SettingsWindow.xaml` с поддержкой динамической локализации
    - [x] Реализовать загрузку текущего статуса и сохранение настройки в `SettingsWindow.xaml.cs`
- [x] Task: Добавление переключателя автозапуска в меню трея (TrayIconManager)
    - [x] Добавить интерактивный пункт с галочкой в контекстное меню трея в `TrayIconManager.cs`
    - [x] Связать обработчик клика в трее с `IAutoStartService` и сохранением настроек в `IConfigurationService`
    - [x] Обеспечить двустороннюю синхронизацию отметки между треем и окном настроек при изменении
- [x] Task: Синхронизация при старте приложения (App.xaml.cs)
    - [x] Добавить проверку актуальности ярлыка в каталоге автозагрузки при запуске приложения (на случай перемещения исполняемого файла)
    - [x] Проверить запуск в фоновом режиме системного трея
- [x] Task: Conductor - User Manual Verification 'Фаза 2: Интеграция с пользовательским интерфейсом' (Protocol in workflow.md)

## Фаза 3: Комплексное тестирование и проверка сценариев
- [x] Task: Прогон автоматических тестов и сборка
    - [x] Выполнить полный прогон `dotnet test` и убедиться в отсутствии ошибок и регрессий
    - [x] Проверить отсутствие предупреждений компилятора (`dotnet build`)
- [x] Task: Комплексная проверка сценариев пользователя
    - [x] Проверить создание и удаление ярлыка в `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`
    - [x] Проверить переключение языков (RU/EN) и сохранение состояния после перезапуска приложения
- [x] Task: Conductor - User Manual Verification 'Фаза 3: Комплексное тестирование и проверка сценариев' (Protocol in workflow.md)

## Фаза 4: Выпуск релиза на GitHub (Release v1.1.0)
- [x] Task: Подготовка коммита изменений трека
    - [x] Проверить статус репозитория и отсутствие лишних файлов
    - [x] Закоммитить изменения трека согласно протоколу коммитов с подробной сводкой изменений
- [x] Task: Создание git-тега v1.1.0 и публикация релиза на GitHub
    - [x] Создать git-тег `v1.1.0` с описанием релиза
    - [x] Отправить тег на GitHub (`git push origin v1.1.0`)
    - [x] Дождаться завершения workflow в GitHub Actions и верифицировать создание релиза и дистрибутива `ScreenTextCatcher-win-x64.zip`
- [x] Task: Conductor - User Manual Verification 'Фаза 4: Выпуск релиза на GitHub' (Protocol in workflow.md)
