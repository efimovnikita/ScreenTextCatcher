# Implementation Plan: Публикация репозитория ScreenTextCatcher на GitHub с CI/CD

## Phase 1: Документация и лицензия
- [ ] Task: Создание файла LICENSE
    - [ ] Создать файл LICENSE с лицензией MIT (2026, Nikita Efimov)
- [ ] Task: Создание файла README.md
    - [ ] Подготовить подробный README.md с описанием функционала ScreenTextCatcher, архитектуры, инструкцией по сборке, запуску, тестированию и настройке API/прокси
- [ ] Task: Проверка проекта и запуск тестов
    - [ ] Убедиться в отсутствии конфиденциальных и лишних файлов в рабочей копии
    - [ ] Запустить `dotnet test` для подтверждения готовности проекта
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Документация и лицензия' (Protocol in workflow.md)

## Phase 2: Настройка GitHub Actions (CI & Release)
- [ ] Task: Создание CI workflow (.github/workflows/ci.yml)
    - [ ] Создать каталог `.github/workflows`
    - [ ] Настроить CI workflow: запуск на push в master и PR, runner windows-latest, .NET 9 SDK, dotnet restore, build и test
- [ ] Task: Создание Release workflow (.github/workflows/release.yml)
    - [ ] Настроить Release workflow: запуск по тегам `v*` и вручную (`workflow_dispatch`)
    - [ ] Реализовать шаг сборки `dotnet publish` (win-x64, single-file), упаковку в zip-архив и публикацию GitHub Release
- [ ] Task: Проверка параметров сборки релиза локально
    - [ ] Протестировать команду `dotnet publish` локально для проверки корректности конфигурации сборки
- [ ] Task: Conductor - User Manual Verification 'Phase 2: Настройка GitHub Actions (CI & Release)' (Protocol in workflow.md)

## Phase 3: Публикация репозитория на GitHub
- [ ] Task: Фиксация подготовительных файлов
    - [ ] Закоммитить LICENSE, README.md и GitHub Actions workflows в локальную ветку `master`
- [ ] Task: Создание публичного репозитория через GitHub CLI (`gh`)
    - [ ] Выполнить создание публичного репозитория `efimovnikita/ScreenTextCatcher` с описанием и привязкой remote `origin`
- [ ] Task: Пуш ветки master и верификация
    - [ ] Выполнить отправку изменений в репозиторий (`git push -u origin master`)
    - [ ] Проверить доступность репозитория через `gh repo view`
    - [ ] Проверить запуск и статус пайплайна GitHub Actions через `gh run list`
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Публикация репозитория на GitHub' (Protocol in workflow.md)
