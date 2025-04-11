-- Удаление существующих таблиц (если они есть)
DROP TABLE IF EXISTS Answers CASCADE;
DROP TABLE IF EXISTS QuizResults CASCADE;
DROP TABLE IF EXISTS QuestionOptions CASCADE;
DROP TABLE IF EXISTS Questions CASCADE;
DROP TABLE IF EXISTS Quizzes CASCADE;
DROP TABLE IF EXISTS Users CASCADE;

-- Создание таблицы Users
CREATE TABLE Users (
    Id SERIAL PRIMARY KEY,
    TelegramId BIGINT NOT NULL UNIQUE,
    Username VARCHAR(255),
    FirstName VARCHAR(255),
    LastName VARCHAR(255),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Создание таблицы Quizzes
CREATE TABLE Quizzes (
    Id SERIAL PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Создание таблицы Questions
CREATE TABLE Questions (
    Id SERIAL PRIMARY KEY,
    QuizId INTEGER NOT NULL,
    Text TEXT NOT NULL,
    CorrectAnswer TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (QuizId) REFERENCES Quizzes(Id) ON DELETE CASCADE
);

-- Создание таблицы QuestionOptions
CREATE TABLE QuestionOptions (
    Id SERIAL PRIMARY KEY,
    QuestionId INTEGER NOT NULL,
    Text TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE
);

-- Создание таблицы QuizResults
CREATE TABLE QuizResults (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL,
    QuizId INTEGER NOT NULL,
    Score INTEGER NOT NULL,
    CompletedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (QuizId) REFERENCES Quizzes(Id) ON DELETE CASCADE
);

-- Создание таблицы Answers
CREATE TABLE Answers (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL,
    QuestionId INTEGER NOT NULL,
    SelectedAnswer TEXT NOT NULL,
    IsCorrect BOOLEAN NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE
);

-- Создание индексов для оптимизации запросов
CREATE INDEX idx_users_telegram_id ON Users(TelegramId);
CREATE INDEX idx_questions_quiz_id ON Questions(QuizId);
CREATE INDEX idx_question_options_question_id ON QuestionOptions(QuestionId);
CREATE INDEX idx_quiz_results_user_id ON QuizResults(UserId);
CREATE INDEX idx_quiz_results_quiz_id ON QuizResults(QuizId);
CREATE INDEX idx_answers_user_id ON Answers(UserId);
CREATE INDEX idx_answers_question_id ON Answers(QuestionId);

-- Комментарии к таблицам
COMMENT ON TABLE Users IS 'Таблица пользователей Telegram';
COMMENT ON TABLE Quizzes IS 'Таблица квизов';
COMMENT ON TABLE Questions IS 'Таблица вопросов для квизов';
COMMENT ON TABLE QuestionOptions IS 'Таблица вариантов ответов для вопросов';
COMMENT ON TABLE QuizResults IS 'Таблица результатов прохождения квизов';
COMMENT ON TABLE Answers IS 'Таблица ответов пользователей на вопросы';

-- Комментарии к колонкам
COMMENT ON COLUMN Users.TelegramId IS 'ID пользователя в Telegram';
COMMENT ON COLUMN Users.Username IS 'Имя пользователя в Telegram';
COMMENT ON COLUMN Quizzes.Title IS 'Название квиза';
COMMENT ON COLUMN Quizzes.Description IS 'Описание квиза';
COMMENT ON COLUMN Questions.Text IS 'Текст вопроса';
COMMENT ON COLUMN Questions.CorrectAnswer IS 'Правильный ответ на вопрос';
COMMENT ON COLUMN QuestionOptions.Text IS 'Текст варианта ответа';
COMMENT ON COLUMN QuizResults.Score IS 'Количество набранных баллов';
COMMENT ON COLUMN Answers.SelectedAnswer IS 'Выбранный пользователем ответ';
COMMENT ON COLUMN Answers.IsCorrect IS 'Флаг правильности ответа'; 