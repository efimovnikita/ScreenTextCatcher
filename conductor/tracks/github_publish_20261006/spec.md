# Specification: Публикация репозитория ScreenTextCatcher на GitHub с CI/CD и сборкой релизов

## 1. Overview
Цель данного трека — подготовить проект к открытой публикации, настроить CI для автоматической сборки и тестов, добавить workflow для сборки и публикации бинарных релизов (GitHub Releases с архивом .exe для Windows x64), а также опубликовать локальный git-репозиторий как публичный репозиторий на GitHub с помощью GitHub CLI (`gh`).

## 2. Scope & Functional Requirements

### 2.1 Подготовка репозитория и документации
- **Файл README.md**:
  - Описание утилиты ScreenTextCatcher (захват области экрана, распознавание через Mistral OCR, поддержка прокси, история, логгер).
  - Стек (.NET 9, WPF, C#).
  - Инструкция по сборке, запуску и тестированию (`dotnet build`, `dotnet test`, `dotnet run`).
  - Руководство по настройке Mistral API Key и параметров прокси.
- **Файл LICENSE**:
  - Лицензия MIT (2026, Nikita Efimov).

### 2.2 GitHub Actions Workflows
- **CI Workflow (`.github/workflows/ci.yml`)**:
  - Триггеры: `push` в ветку `master` и `pull_request` в `master`.
  - Runner: `windows-latest`.
  - Шаги: установка .NET SDK 9.x, восстановление зависимостей (`dotnet restore`), компиляция (`dotnet build --configuration Release`), запуск модульных тестов (`dotnet test --configuration Release`).
- **Release Workflow (`.github/workflows/release.yml`)**:
  - Триггеры: создание тега `v*` (например, `v1.0.0`) и ручной запуск (`workflow_dispatch`).
  - Runner: `windows-latest`.
  - Шаги:
    - Сборка и публикация исполняемого файла через `dotnet publish` (`-c Release -r win-x64 --self-contained true -p:PublishSingleFile=true`).
    - Упаковка в zip-архив (`ScreenTextCatcher-win-x64.zip`).
    - Создание релиза на GitHub и загрузка собранного архива в релиз.

### 2.3 Публикация через GitHub CLI (`gh`)
- Создание публичного репозитория `efimovnikita/ScreenTextCatcher` с описанием "Windows tray utility for screen capture and OCR via Mistral AI".
- Привязка remote `origin` к локальному репозиторию.
- Отправка ветки `master` (`git push -u origin master`).
- Верификация видимости репозитория и статуса CI пайплайна через `gh`.

## 3. Acceptance Criteria
1. Файлы `README.md`, `LICENSE`, `.github/workflows/ci.yml` и `.github/workflows/release.yml` созданы и закомичены.
2. Проект успешно собирается и тесты проходят локально (`dotnet test`).
3. Публичный репозиторий создан на GitHub под аккаунтом `efimovnikita`.
4. Локальный репозиторий синхронизирован с `origin/master`.
5. GitHub Actions CI workflow запустился и успешно выполняется при пуше.
6. Репозиторий доступен публично на GitHub.

## 4. Out of Scope
- Создание MSIX / MSI инсталляторов (только portable zip релиз).
- Механизм автообновления приложения (in-app auto-updater).
