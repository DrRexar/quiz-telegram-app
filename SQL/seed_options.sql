-- Добавление вариантов ответов для квиза по истории
WITH history_questions AS (
    SELECT q.id, q.text 
    FROM Questions q
    JOIN Quizzes qz ON q.QuizId = qz.id
    WHERE qz.Title = 'История России'
)
INSERT INTO QuestionOptions (QuestionId, Text)
SELECT hq.id, opt.text
FROM history_questions hq
CROSS JOIN (VALUES
    ('В каком году состоялась Куликовская битва?', '1380'),
    ('В каком году состоялась Куликовская битва?', '1480'),
    ('В каком году состоялась Куликовская битва?', '1240'),
    ('В каком году состоялась Куликовская битва?', '1410'),
    ('Кто был первым императором России?', 'Пётр I'),
    ('Кто был первым императором России?', 'Иван Грозный'),
    ('Кто был первым императором России?', 'Александр I'),
    ('Кто был первым императором России?', 'Павел I')
) AS opt(question, text)
WHERE hq.text = opt.question;

-- Добавление вариантов ответов для квиза по географии
WITH geography_questions AS (
    SELECT q.id, q.text 
    FROM Questions q
    JOIN Quizzes qz ON q.QuizId = qz.id
    WHERE qz.Title = 'География мира'
)
INSERT INTO QuestionOptions (QuestionId, Text)
SELECT gq.id, opt.text
FROM geography_questions gq
CROSS JOIN (VALUES
    ('Какая самая высокая гора в мире?', 'Эверест'),
    ('Какая самая высокая гора в мире?', 'Килиманджаро'),
    ('Какая самая высокая гора в мире?', 'Монблан'),
    ('Какая самая высокая гора в мире?', 'Эльбрус'),
    ('Какая самая длинная река в мире?', 'Нил'),
    ('Какая самая длинная река в мире?', 'Амазонка'),
    ('Какая самая длинная река в мире?', 'Янцзы'),
    ('Какая самая длинная река в мире?', 'Миссисипи')
) AS opt(question, text)
WHERE gq.text = opt.question;

-- Добавление вариантов ответов для квиза по биологии
WITH biology_questions AS (
    SELECT q.id, q.text 
    FROM Questions q
    JOIN Quizzes qz ON q.QuizId = qz.id
    WHERE qz.Title = 'Биология'
)
INSERT INTO QuestionOptions (QuestionId, Text)
SELECT bq.id, opt.text
FROM biology_questions bq
CROSS JOIN (VALUES
    ('Какой орган человека отвечает за выработку инсулина?', 'Поджелудочная железа'),
    ('Какой орган человека отвечает за выработку инсулина?', 'Печень'),
    ('Какой орган человека отвечает за выработку инсулина?', 'Щитовидная железа'),
    ('Какой орган человека отвечает за выработку инсулина?', 'Надпочечники'),
    ('Сколько камер в сердце человека?', '4'),
    ('Сколько камер в сердце человека?', '2'),
    ('Сколько камер в сердце человека?', '3'),
    ('Сколько камер в сердце человека?', '5')
) AS opt(question, text)
WHERE bq.text = opt.question;

-- Добавление вариантов ответов для квиза по физике
WITH physics_questions AS (
    SELECT q.id, q.text 
    FROM Questions q
    JOIN Quizzes qz ON q.QuizId = qz.id
    WHERE qz.Title = 'Физика'
)
INSERT INTO QuestionOptions (QuestionId, Text)
SELECT pq.id, opt.text
FROM physics_questions pq
CROSS JOIN (VALUES
    ('Какая единица измерения силы тока?', 'Ампер'),
    ('Какая единица измерения силы тока?', 'Вольт'),
    ('Какая единица измерения силы тока?', 'Ватт'),
    ('Какая единица измерения силы тока?', 'Ом'),
    ('Какая частица имеет положительный заряд?', 'Протон'),
    ('Какая частица имеет положительный заряд?', 'Электрон'),
    ('Какая частица имеет положительный заряд?', 'Нейтрон'),
    ('Какая частица имеет положительный заряд?', 'Фотон')
) AS opt(question, text)
WHERE pq.text = opt.question;

-- Добавление вариантов ответов для квиза по информатике
WITH cs_questions AS (
    SELECT q.id, q.text 
    FROM Questions q
    JOIN Quizzes qz ON q.QuizId = qz.id
    WHERE qz.Title = 'Информатика'
)
INSERT INTO QuestionOptions (QuestionId, Text)
SELECT cq.id, opt.text
FROM cs_questions cq
CROSS JOIN (VALUES
    ('Сколько бит в одном байте?', '4'),
    ('Сколько бит в одном байте?', '8'),
    ('Сколько бит в одном байте?', '16'),
    ('Сколько бит в одном байте?', '32'),
    ('Какая система счисления используется в компьютерах?', 'Двоичная'),
    ('Какая система счисления используется в компьютерах?', 'Десятичная'),
    ('Какая система счисления используется в компьютерах?', 'Восьмеричная'),
    ('Какая система счисления используется в компьютерах?', 'Шестнадцатеричная'),
    ('Что означает аббревиатура CPU?', 'Central Processing Unit'),
    ('Что означает аббревиатура CPU?', 'Computer Personal Unit'),
    ('Что означает аббревиатура CPU?', 'Control Processing Unit'),
    ('Что означает аббревиатура CPU?', 'Central Program Unit')
) AS opt(question, text)
WHERE cq.text = opt.question; 