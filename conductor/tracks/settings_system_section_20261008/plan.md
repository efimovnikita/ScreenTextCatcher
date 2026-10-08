# Implementation Plan — Реорганизация настроек: вынос автозапуска в секцию «Система»

## Фаза 1: Локализация и модульные тесты (TDD)
- [x] Task: Добавление тестов для ключа Loc_SystemGroup (TDD Red Phase)
    - [x] Добавить проверку ключа `Loc_SystemGroup` для русского языка в `LocalizationManagerTests.cs`
    - [x] Добавить проверку ключа `Loc_SystemGroup` для английского языка в `LocalizationManagerTests.cs`
- [x] Task: Реализация ключей локализации в LocalizationManager (TDD Green Phase)
    - [x] Добавить `["Loc_SystemGroup"] = "Система"` в `RuDictionary` в `LocalizationManager.cs`
    - [x] Добавить `["Loc_SystemGroup"] = "System"` в `EnDictionary` в `LocalizationManager.cs`
    - [x] Запустить `dotnet test` и убедиться в успешном прохождении тестов
- [x] Task: Conductor - User Manual Verification 'Фаза 1: Локализация и модульные тесты' (Protocol in workflow.md)

## Фаза 2: Реорганизация разметки окна настроек (SettingsWindow UI)
- [x] Task: Обновление разметки SettingsWindow.xaml
    - [x] Увеличить высоту окна `Height` с 680 до 740 px
    - [x] Удалить чекбокс `ChkAutoStart` из карточки `Loc_InterfaceAndSoundGroup`
    - [x] Добавить карточку «Система» (`Border`) с заголовком `{DynamicResource Loc_SystemGroup}` и чекбоксом `ChkAutoStart` перед кнопками действий
- [x] Task: Проверка компиляции и интеграции
    - [x] Выполнить `dotnet build` и проверить отсутствие ошибок компиляции
    - [x] Проверить корректность привязок `ChkAutoStart` в code-behind `SettingsWindow.xaml.cs`
- [x] Task: Conductor - User Manual Verification 'Фаза 2: Реорганизация разметки окна настроек' (Protocol in workflow.md)

## Фаза 3: Комплексная верификация и тестирование
- [x] Task: Автоматическое тестирование
    - [x] Выполнить запуск `dotnet test` и убедиться в 100% успешном прохождении всех тестов
- [x] Task: Комплексная ручная проверка
    - [x] Проверить отображение секции «Система» на русском и английском языках
    - [x] Проверить всплывающую подсказку у чекбокса `ChkAutoStart`
    - [x] Проверить сохранение и загрузку состояния автозапуска
- [x] Task: Conductor - User Manual Verification 'Фаза 3: Комплексная верификация и тестирование' (Protocol in workflow.md)

## Фаза 4: Выпуск релиза на GitHub (Release v1.2.3)
- [x] Task: Подготовка коммита изменений трека
    - [x] Обновить версию в `ScreenTextCatcher.csproj` и `ScreenTextCatcher.Core.csproj` до `1.2.3`
    - [x] Закоммитить изменения трека согласно протоколу коммитов с подробной сводкой
- [~] Task: Создание git-тега v1.2.3 и публикация релиза на GitHub
    - [ ] Создать git-тег `v1.2.3` с описанием релиза
    - [ ] Отправить тег на GitHub (`git push origin v1.2.3`)
    - [ ] Дождаться завершения GitHub Actions Release workflow и верифицировать создание дистрибутива `ScreenTextCatcher-win-x64.zip`
- [ ] Task: Conductor - User Manual Verification 'Фаза 4: Выпуск релиза на GitHub' (Protocol in workflow.md)
