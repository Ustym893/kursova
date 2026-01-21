# EduFlow

EduFlow — це курсова робота, яка реалізує систему взаємодії між викладачами та студентами.

Проєкт складається з:
- ASP.NET Core Web API (бекенд)
- PostgreSQL (база даних)
- .NET MAUI Desktop UI (MacCatalyst + Windows)

Функціонал:
- Реєстрація та авторизація користувачів  
- Ролі: викладач / студент  
- Створення та керування завданнями  
- Перегляд і виконання завдань  

---

# Getting Started

Цей гайд допоможе налаштувати та запустити проєкт локально.

---

## Prerequisites

Перед початком переконайтесь, що встановлено:

.NET SDK >= 8  
Перевірка:
dotnet –version

Встановлення:
https://dotnet.microsoft.com/download  

Docker Desktop  
Перевірка:
docker –version
Встановлення:
https://www.docker.com/products/docker-desktop  

Visual Studio 2022 з workload  
“.NET Multi-platform App UI development”  
Встановлення:
https://visualstudio.microsoft.com/vs/  

Git  
Перевірка:
git –version
Встановлення:
https://git-scm.com  

---

## Initial Setup

### 1. Клонування репозиторію
git clone https://github.com/Ustym893/kursova.git

cd kursova

---

### 2. Налаштування змінних середовища

Перейдіть у папку:
backend/EduFlow.Api

(поки можна пропустити)
Скопіюйте файл:
.env.example -> .env
За потреби відкрийте `.env` і змініть значення  
(для локального запуску стандартні значення підходять).

---

### 3. Запуск бази даних (PostgreSQL через Docker)

Перейдіть у папку:
cd docker

Запустіть базу:
docker compose up -d

Перевірте, що контейнер працює:

docker ps
---
(для перевірки maui/ui переходьте до кроку 5)
---
### 4. Запуск бекенду та застосування міграцій

Перейдіть у папку бекенду:
cd backend

cd EduFlow.Api

Відновіть пакети: dotnet restore

Застосуйте міграції: dotnet ef database update

Запустіть API: dotnet run

Відкрийте Swagger у браузері: https://localhost:xxxx/swagger
Swagger UI має відкритися без помилок.

---

### 5. Запуск Desktop UI (MAUI)

#### Варіант A — через Visual Studio (Windows)

1. Відкрити Visual Studio 2022  
2. Відкрити файл: EduFlow.sln
3. Встановити Startup Project: EduFlow.Desktop
4. Обрати таргет: Windows Machine
5. Натиснути **Run**

---

#### Варіант B — через CLI (macOS)

cd desktop/EduFlow.Desktop

dotnet run -f net8.0-maccatalyst

---

## Verification

### Backend

Відкрити: https://localhost:xxxx/swagger
Перевірити:
- Register
- Login
- Create teacher / student
- Create assignments
- Get assignments

Усі ендпоінти повинні повертати 200 або 201.

---

### Desktop UI

У застосунку:
1. Увійти в систему  
2. Обрати роль  
3. Відкрити Dashboard  
4. Переглянути список завдань  

---

## Notes

- База даних запускається через Docker  
- Desktop UI працює на Windows та macOS  
- Backend не залежить від платформи  

Проєкт повністю запускається локально на Windows за цією інструкцією.
