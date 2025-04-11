-- Создание таблицы пользователей
CREATE TABLE IF NOT EXISTS Users (
    Id SERIAL PRIMARY KEY,
    TelegramId BIGINT NOT NULL UNIQUE,
    Username VARCHAR(255),
    FirstName VARCHAR(255),
    LastName VARCHAR(255),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Создание таблицы квизов
CREATE TABLE IF NOT EXISTS Quizzes (
    Id SERIAL PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Создание таблицы вопросов
CREATE TABLE IF NOT EXISTS Questions (
    Id SERIAL PRIMARY KEY,
    QuizId INTEGER REFERENCES Quizzes(Id) ON DELETE CASCADE,
    Text TEXT NOT NULL,
    CorrectAnswer TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Создание таблицы вариантов ответов
CREATE TABLE IF NOT EXISTS QuestionOptions (
    Id SERIAL PRIMARY KEY,
    QuestionId INTEGER REFERENCES Questions(Id) ON DELETE CASCADE,
    Text TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Создание таблицы результатов
CREATE TABLE IF NOT EXISTS QuizResults (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id) ON DELETE CASCADE,
    QuizId INTEGER REFERENCES Quizzes(Id) ON DELETE CASCADE,
    Score INTEGER NOT NULL,
    CompletedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Создание таблицы ответов пользователей
CREATE TABLE IF NOT EXISTS Answers (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id) ON DELETE CASCADE,
    QuestionId INTEGER REFERENCES Questions(Id) ON DELETE CASCADE,
    SelectedAnswer TEXT NOT NULL,
    IsCorrect BOOLEAN NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
); 