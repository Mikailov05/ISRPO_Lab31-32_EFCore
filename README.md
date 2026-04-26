# Лабораторная работа №31-32: ASP.NET Core + Entity Framework Core + SQLite

**Студент:** Микаилов Ахмед
**Группа:**  ИСП-231  
**Дата:** [27.04.2026

---

## Краткое описание работы

В данной лабораторной работе разработано REST API для управления списком задач (Todo-приложение) с использованием ASP.NET Core и Entity Framework Core. База данных SQLite хранится в файле `taskdb.db`. Реализованы:

- Полный CRUD для задач
- Поиск с фильтрацией
- Статистика выполнения задач
- Пагинация
- Отложенные задачи (срок выполнения)
- Массовое обновление/удаление

---

## Полезные команды dotnet ef

| Команда | Описание |
|---------|----------|
| `dotnet ef migrations add <Name>` | Создать новую миграцию |
| `dotnet ef database update` | Применить миграции к БД |
| `dotnet ef migrations list` | Показать список миграций |
| `dotnet ef migrations remove` | Удалить последнюю миграцию |
| `dotnet ef migrations script` | Сгенерировать SQL-скрипт |

---

## Структура проекта

```
TaskDb/
├── Controllers/
│   └── TasksController.cs          # API-контроллер задач
├── Data/
│   └── AppDbContext.cs             # Контекст БД (EF Core)
├── Models/
│   ├── TaskItem.cs                 # Модель задачи
│   └── TaskDtos.cs                 # DTO для создания/обновления
├── Migrations/                     # Сгенерированные миграции
├── appsettings.json                # Настройки (строка подключения)
├── appsettings.Development.json    # Логирование SQL
├── Program.cs                      # Настройка сервера и DI
└── taskdb.db                       # Файл базы данных SQLite
```

---

## Список реализованных маршрутов

| Метод | Маршрут | Описание |
|-------|---------|----------|
| GET | `/api/tasks` | Получить все задачи (с фильтрацией) |
| GET | `/api/tasks/{id}` | Получить задачу по ID |
| GET | `/api/tasks/search?query=&priority=&completed=` | Поиск задач |
| GET | `/api/tasks/stats` | Статистика задач |
| GET | `/api/tasks/paged?page=&pagesize=` | Пагинация задач |
| GET | `/api/tasks/overdue` | Просроченные задачи |
| POST | `/api/tasks` | Создать новую задачу |
| PUT | `/api/tasks/{id}` | Полное обновление задачи |
| PATCH | `/api/tasks/{id}/complete` | Переключить статус выполнения |
| PATCH | `/api/tasks/complete-all` | Отметить все задачи как выполненные |
| DELETE | `/api/tasks/{id}` | Удалить задачу |
| DELETE | `/api/tasks/completed` | Удалить все выполненные задачи |

---

## Таблица применённых миграций

| Миграция | Описание изменений |
|----------|---------------------|
| `InitialCreate` | Создание таблицы `Tasks` с полями: Id, Title, Description, IsCompleted, CreatedAt, Priority + начальные данные (3 задачи) |
| `AddDueDateToTask` | Добавление поля `DueDate` (DateTime?, срок выполнения) |

---

## Сравнительная таблица LINQ vs SQL

| LINQ | SQL |
|------|-----|
| `.Where(t => t.IsCompleted == false)` | `WHERE is_completed = 0` |
| `.OrderBy(t => t.CreatedAt)` | `ORDER BY created_at ASC` |
| `.OrderByDescending(t => t.CreatedAt)` | `ORDER BY created_at DESC` |
| `.Take(10)` | `LIMIT 10` |
| `.Skip(20).Take(10)` | `OFFSET 20 LIMIT 10` |
| `.Count()` | `SELECT COUNT(*)` |
| `.Any(t => t.Priority == "High")` | `SELECT EXISTS(...)` |
| `.GroupBy(t => t.Priority)` | `GROUP BY priority` |
| `.Select(t => t.Title)` | `SELECT title` |
| `Contains("sql")` | `LIKE '%sql%'` |

---

## Итоговая сравнительная таблица

| Концепция | Хранение в памяти | EF Core + SQLite |
|-----------|-------------------|------------------|
| **Хранение данных** | `static List<T>` в RAM | Файл `.db` на диске |
| **После перезапуска** | Данные пропадают | Данные сохраняются |
| **Поиск по условию** | LINQ to Objects | LINQ to Entities → SQL |
| **Создание структуры** | Не нужно | Миграции (`dotnet ef`) |
| **Начальные данные** | Хардкод в коде | `HasData()` в миграции |
| **Получение данных** | `list.FirstOrDefault(...)` | `await db.Table.FindAsync(id)` |
| **Добавление** | `list.Add(item)` | `db.Table.Add(item) + SaveChangesAsync()` |
| **Удаление** | `list.Remove(item)` | `db.Table.Remove(item) + SaveChangesAsync()` |
| **Масштабируемость** | Ограничена RAM | Гигабайты данных |
| **Транзакции** | Нет | Встроены в EF Core |

---

## Главные выводы

1. **EF Core — это переводчик между C# и SQL.** Вы пишете LINQ-запросы, а он автоматически генерирует оптимизированные SQL-запросы. Это ускоряет разработку и снижает количество ошибок, связанных с ручным написанием SQL.

2. **Миграции — это Git для структуры базы данных.** Каждое изменение моделей фиксируется в виде миграции, которая хранится в репозитории. Это позволяет команде синхронно обновлять схему БД и откатывать изменения при необходимости.

3. **Code First подход удобнее ручного SQL.** Вы меняете C#-класс, создаёте миграцию, и база обновляется автоматически. Не нужно писать `ALTER TABLE` вручную — EF Core генерирует правильный SQL за вас.

4. **`SaveChangesAsync()` — ключевой момент.** До его вызова все изменения (`Add`, `Remove`, присваивание новых значений) живут только в памяти приложения. Только после `SaveChangesAsync()` они фиксируются в базе данных. Это похоже на транзакцию.

5. **`async/await` при работе с БД — не опционально, а стандарт.** Блокировать поток на время ожидания ответа от БД — плохая практика, особенно в веб-приложениях. `async/await` позволяет серверу обрабатывать другие запросы во время ожидания базы данных.

6. **LINQ-запросы выполняются отложенно (deferred execution).** Запрос уходит в БД только при вызове `.ToListAsync()`, `.CountAsync()` или `.FirstOrDefaultAsync()`. Это позволяет гибко наращивать фильтры без многократных обращений к БД.

7. **Dependency Injection (DI) в ASP.NET Core автоматически управляет временем жизни DbContext.** Мы зарегистрировали `AppDbContext` один раз в `Program.cs`, и фреймворк сам создаёт и уничтожает контекст при каждом HTTP-запросе. Не нужно вручную писать `using` или `new AppDbContext()`.
```