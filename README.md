# ScreenTextCatcher

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20x64-lightgrey.svg)](https://microsoft.com/windows)

**ScreenTextCatcher** — легковесная системная утилита для Windows, работающая в системном трее и предназначенная для мгновенного оптического распознавания текста (OCR) с любого участка экрана с помощью облачного сервиса **Mistral OCR** и автоматического копирования результата в буфер обмена Windows.

---

## Возможности

- 🚀 **Мгновенный захват экрана**:
  - Активация по глобальной горячей клавише (по умолчанию `Win + Shift + X`).
  - Оверлей с затемнением на 50% и курсором-перекрестием (`Crosshair`).
  - Интерактивное выделение прямоугольной области мышью.
  - Мгновенная отмена по нажатию `Esc` или клику правой кнопкой мыши.
- 🧠 **Распознавание через Mistral OCR**:
  - Прямая отправка скриншота в Mistral OCR API в формате base64 data-URL.
  - Обработка изображений исключительно в оперативной памяти (RAM) без сохранения временных файлов на диск.
  - Извлечение форматированного текста (Markdown) и мгновенное копирование в буфер обмена (`Ctrl+V`).
- 🌐 **Поддержка прокси**:
  - Возможность работы через HTTP, HTTPS и SOCKS5 прокси-серверы с опциональной аутентификацией.
  - Встроенный тестер подключения к прокси и проверки Mistral API ключа прямо из окна настроек.
- 🕒 **История распознаваний**:
  - Локальное сохранение последних 100 распознанных фрагментов.
  - Быстрый доступ к истории через контекстное меню системного трея.
  - Повторное копирование любого фрагмента в один клик.
- 📋 **Журнал событий и диагностика**:
  - Встроенный Log Viewer для отслеживания запросов к API, статусов сети и возможных ошибок.
- 🔒 **Безопасность**:
  - Шифрование чувствительных данных (Mistral API Key, пароли прокси) с использованием Windows Data Protection API (DPAPI).
- 🌍 **Многоязычность**:
  - Поддержка интерфейса на русском и английском языках (автоопределение и ручное переключение).

---

## Архитектура проекта

Решение разделено на три проекта:

```text
ScreenTextCatcher/
├── src/
│   ├── ScreenTextCatcher/           # WPF GUI, системный трей, оверлей затемнения, диалоги
│   └── ScreenTextCatcher.Core/      # Бизнес-логика, Mistral OCR клиент, настройки, история, логирование
├── tests/
│   └── ScreenTextCatcher.Tests/     # Модульные и интеграционные тесты (xUnit, Moq, FluentAssertions)
└── conductor/                       # Conductor спецификации, планы и workflow
```

---

## Системные требования

- Windows 10 (1809+) или Windows 11 (x64)
- .NET 9.0 Runtime (или автономная self-contained сборка)
- Учётная запись и API-ключ [Mistral AI](https://console.mistral.ai/)

---

## Сборка и запуск

### Клонирование репозитория

```powershell
git clone https://github.com/efimovnikita/ScreenTextCatcher.git
cd ScreenTextCatcher
```

### Сборка проекта

```powershell
dotnet build ScreenTextCatcher.slnx
```

### Запуск модульных тестов

```powershell
dotnet test
```

### Запуск приложения

```powershell
dotnet run --project src/ScreenTextCatcher/ScreenTextCatcher.csproj
```

### Публикация автономного исполняемого файла (Self-contained Single-file)

```powershell
dotnet publish src/ScreenTextCatcher/ScreenTextCatcher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

Собранный файл `ScreenTextCatcher.exe` будет находиться в папке `./publish` и не требует предварительно установленного .NET Runtime на целевом компьютере.

---

## Настройка

1. При первом запуске откройте **Настройки** (Settings) через меню иконки в системном трее.
2. Введите ваш **Mistral API Key**.
3. При необходимости включите прокси-сервер (HTTP/HTTPS/SOCKS5) и укажите хост, порт и реквизиты доступа.
4. Нажмите кнопку **Тест подключения**, чтобы проверить доступность API и прокси.
5. Задайте желаемую комбинацию горячих клавиш (по умолчанию `Win + Shift + X`).
6. Сохраните настройки. Теперь ScreenTextCatcher готов к работе!

---

## Лицензия

Проект распространяется под лицензией [MIT](LICENSE).

Copyright (c) 2026 Nikita Efimov
