-- Создание таблицы Quizzes
CREATE TABLE IF NOT EXISTS "Quizzes" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Создание таблицы Questions
CREATE TABLE IF NOT EXISTS "Questions" (
    "Id" SERIAL PRIMARY KEY,
    "Text" TEXT NOT NULL,
    "Options" JSONB NOT NULL,
    "CorrectAnswer" VARCHAR(255) NOT NULL,
    "QuizId" INTEGER NOT NULL REFERENCES "Quizzes"("Id") ON DELETE CASCADE
);

-- Создание таблицы Users
CREATE TABLE IF NOT EXISTS "Users" (
    "TelegramId" BIGINT PRIMARY KEY,
    "Username" VARCHAR(255),
    "FirstName" VARCHAR(255) NOT NULL,
    "LastName" VARCHAR(255) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastActivityAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Создание таблицы QuizResults
CREATE TABLE IF NOT EXISTS "QuizResults" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" BIGINT NOT NULL REFERENCES "Users"("TelegramId") ON DELETE CASCADE,
    "QuizId" INTEGER NOT NULL REFERENCES "Quizzes"("Id") ON DELETE CASCADE,
    "Score" INTEGER NOT NULL,
    "CompletedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
); 