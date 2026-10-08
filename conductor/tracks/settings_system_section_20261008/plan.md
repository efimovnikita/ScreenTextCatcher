# Implementation Plan — Реорганизация настроек: вынос автозапуска в секцию «Система»

## Фаза 1: Локализация и модульные тесты (TDD)
- [ ] Task: Добавление тестов для ключа Loc_SystemGroup (TDD Red Phase)
    - [ ] Добавить проверку ключа `Loc_SystemGroup` для русского языка в `LocalizationManagerTests.cs`
    - [ ] Добавить проверку ключа `Loc_SystemGroup` для английского языка в `LocalizationManagerTests.cs`
- [ ] Task: Реализация ключей локализации в LocalizationManager (TDD Green Phase)
    - [ ] Добавить `["Loc_SystemGroup"] = "Система"` в `RuDictionary` в `LocalizationManager.cs`
    - [ ] Добавить `["Loc_SystemGroup"] = "System"` в `EnDictionary` в `LocalizationManager.cs`
    - [ ] Запустить `dotnet test` и убедиться в успешном прохождении тестов
- [ ] Task: Conductor - User Manual Verification 'Фаза 1: Локализация и модульные тесты' (Protocol in workflow.md)

## Фаза 2: Реорганизация разметки окна настроек (SettingsWindow UI)
- [ ] Task: Обновление разметки SettingsWindow.xaml
    - [ ] Увеличить высоту окна `Height` с 680 до 740 px
    - [ ] Удалить чекбокс `ChkAutoStart` из карточки `Loc_InterfaceAndSoundGroup`
    - [ ] Добавить карточку «Система» (`Border`) с заголовком `{DynamicResource Loc_SystemGroup}` и чекбоксом `ChkAutoStart` перед кнопками действий
- [ ] Task: Проверка компиляции и интеграции
    - [ ] Выполнить `dotnet build` и проверить отсутствие ошибок компиляции
    - [ ] Проверить корректность привязок `ChkAutoStart` в code-behind `SettingsWindow.xaml.cs`
- [ ] Task: Conductor - User Manual Verification 'Фаза 2: Реорганизация разметки окна настроек' (Protocol in workflow.md)

## Фаза 3: Комплексная верификация и тестирование
- [ ] Task: Автоматическое тестирование
    - [ ] Выполнить запуск `dotnet test` и убедиться в 100% успешном прохождении всех тестов
- [ ] Task: Комплексная ручная проверка
    - [ ] Проверить отображение секции «Система» на русском и английском языках
    - [ ] Проверить всплывающую подсказку у чекбокса `ChkAutoStart`
    - [ ] Проверить сохранение и загрузку состояния автозапуска
- [ ] Task: Conductor - User Manual Verification 'Фаза 3: Комплексная верификация и тестирование' (Protocol in workflow.md)

## Фаза 4: Выпуск релиза на GitHub (Release v1.2.3)
- [ ] Task: Подготовка коммита изменений трека
    - [ ] Обновить версию в `ScreenTextCatcher.csproj` и `ScreenTextCatcher.Core.csproj` до `1.2.3`
    - [ ] Закоммитить изменения трека согласно протоколу коммитов с подробной сводкой
- [ ] Task: Создание git-тега v1.2.3 и публикация релиза на GitHub
    - [ ] Создать git-тег `v1.2.3` с описанием релиза
    - [ ] Отправить тег на GitHub (`git push origin v1.2.3`)
    - [ ] Дождаться завершения GitHub Actions Release workflow и верифицировать создание дистрибутива `ScreenTextCatcher-win-x64.zip`
- [ ] Task: Conductor - User Manual Verification 'Фаза 4: Выпуск релиза на GitHub' (Protocol in workflow.md)
