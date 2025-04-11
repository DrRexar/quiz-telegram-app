-- Очистка существующих данных
DELETE FROM Answers;
DELETE FROM QuizResults;
DELETE FROM QuestionOptions;
DELETE FROM Questions;
DELETE FROM Quizzes;

-- Квиз 1: История России
INSERT INTO Quizzes (Title, Description) VALUES ('История России', 'Тест на знание ключевых событий истории России') RETURNING id;

WITH history_quiz AS (
    SELECT id FROM Quizzes WHERE Title = 'История России'
)
INSERT INTO Questions (QuizId, Text, CorrectAnswer) VALUES
((SELECT id FROM history_quiz), 'В каком году произошло Крещение Руси?', '988'),
((SELECT id FROM history_quiz), 'Кто был первым русским царём?', 'Иван IV Грозный'),
((SELECT id FROM history_quiz), 'В каком году состоялась Куликовская битва?', '1380'),
((SELECT id FROM history_quiz), 'Кто был первым императором России?', 'Пётр I'),
((SELECT id FROM history_quiz), 'В каком году началась Отечественная война с Наполеоном?', '1812'),
((SELECT id FROM history_quiz), 'Кто был последним российским императором?', 'Николай II'),
((SELECT id FROM history_quiz), 'В каком году произошла Октябрьская революция?', '1917'),
((SELECT id FROM history_quiz), 'Кто был первым президентом России?', 'Борис Ельцин'),
((SELECT id FROM history_quiz), 'В каком году началась Великая Отечественная война?', '1941'),
((SELECT id FROM history_quiz), 'В каком году был запущен первый искусственный спутник Земли?', '1957');

-- Добавление вариантов ответов для первого квиза
INSERT INTO QuestionOptions (QuestionId, Text) VALUES
((SELECT Id FROM Questions WHERE Text = 'В каком году произошло Крещение Руси?'), '988'),
((SELECT Id FROM Questions WHERE Text = 'В каком году произошло Крещение Руси?'), '1054'),
((SELECT Id FROM Questions WHERE Text = 'В каком году произошло Крещение Руси?'), '862'),
((SELECT Id FROM Questions WHERE Text = 'В каком году произошло Крещение Руси?'), '1147');

INSERT INTO QuestionOptions (QuestionId, Text) VALUES
((SELECT Id FROM Questions WHERE Text = 'Кто был первым русским царём?'), 'Иван III'),
((SELECT Id FROM Questions WHERE Text = 'Кто был первым русским царём?'), 'Иван IV Грозный'),
((SELECT Id FROM Questions WHERE Text = 'Кто был первым русским царём?'), 'Дмитрий Донской'),
((SELECT Id FROM Questions WHERE Text = 'Кто был первым русским царём?'), 'Василий III');

-- Продолжаем добавлять варианты для остальных вопросов первого квиза...

-- Квиз 2: География мира
INSERT INTO Quizzes (Title, Description) VALUES ('География мира', 'Проверьте свои знания о странах, континентах и природных явлениях') RETURNING id;

WITH geography_quiz AS (
    SELECT id FROM Quizzes WHERE Title = 'География мира'
)
INSERT INTO Questions (QuizId, Text, CorrectAnswer) VALUES
((SELECT id FROM geography_quiz), 'Какая самая высокая гора в мире?', 'Эверест'),
((SELECT id FROM geography_quiz), 'Какая самая длинная река в мире?', 'Нил'),
((SELECT id FROM geography_quiz), 'Какой самый большой океан?', 'Тихий'),
((SELECT id FROM geography_quiz), 'Какая самая большая страна по площади?', 'Россия'),
((SELECT id FROM geography_quiz), 'На каком континенте находится пустыня Сахара?', 'Африка'),
((SELECT id FROM geography_quiz), 'Какой самый большой остров в мире?', 'Гренландия'),
((SELECT id FROM geography_quiz), 'Какая страна имеет больше всего часовых поясов?', 'Франция'),
((SELECT id FROM geography_quiz), 'Какое самое глубокое озеро в мире?', 'Байкал'),
((SELECT id FROM geography_quiz), 'В какой стране находится Большой Барьерный риф?', 'Австралия'),
((SELECT id FROM geography_quiz), 'Какой континент самый холодный?', 'Антарктида');

-- Квиз 3: Биология
INSERT INTO Quizzes (Title, Description) VALUES ('Биология', 'Тест на знание основ биологии и живых организмов') RETURNING id;

WITH biology_quiz AS (
    SELECT id FROM Quizzes WHERE Title = 'Биология'
)
INSERT INTO Questions (QuizId, Text, CorrectAnswer) VALUES
((SELECT id FROM biology_quiz), 'Какой орган человека отвечает за выработку инсулина?', 'Поджелудочная железа'),
((SELECT id FROM biology_quiz), 'Сколько камер в сердце человека?', '4'),
((SELECT id FROM biology_quiz), 'Какой процесс происходит в хлоропластах растений?', 'Фотосинтез'),
((SELECT id FROM biology_quiz), 'Как называется наука о клетках?', 'Цитология'),
((SELECT id FROM biology_quiz), 'Какое вещество переносит кислород в крови?', 'Гемоглобин'),
((SELECT id FROM biology_quiz), 'Какая самая маленькая единица жизни?', 'Клетка'),
((SELECT id FROM biology_quiz), 'Как называется процесс деления клетки?', 'Митоз'),
((SELECT id FROM biology_quiz), 'Какой витамин вырабатывается в коже под действием солнечного света?', 'D'),
((SELECT id FROM biology_quiz), 'Сколько пар хромосом в клетках человека?', '23'),
((SELECT id FROM biology_quiz), 'Какой орган отвечает за выработку желчи?', 'Печень');

-- Квиз 4: Физика
INSERT INTO Quizzes (Title, Description) VALUES ('Физика', 'Проверьте свои знания законов физики') RETURNING id;

WITH physics_quiz AS (
    SELECT id FROM Quizzes WHERE Title = 'Физика'
)
INSERT INTO Questions (QuizId, Text, CorrectAnswer) VALUES
((SELECT id FROM physics_quiz), 'Какая единица измерения силы тока?', 'Ампер'),
((SELECT id FROM physics_quiz), 'Чему равна скорость света в вакууме?', '299792458'),
((SELECT id FROM physics_quiz), 'Какой закон описывает силу взаимодействия двух точечных зарядов?', 'Закон Кулона'),
((SELECT id FROM physics_quiz), 'Какая частица имеет положительный заряд?', 'Протон'),
((SELECT id FROM physics_quiz), 'В каких единицах измеряется энергия?', 'Джоуль'),
((SELECT id FROM physics_quiz), 'Какая формула описывает второй закон Ньютона?', 'F=ma'),
((SELECT id FROM physics_quiz), 'Какое явление описывает закон Ома?', 'Электрический ток'),
((SELECT id FROM physics_quiz), 'Какая единица измерения частоты?', 'Герц'),
((SELECT id FROM physics_quiz), 'Какая формула описывает энергию покоя?', 'E=mc²'),
((SELECT id FROM physics_quiz), 'В каких единицах измеряется электрическое напряжение?', 'Вольт');

-- Квиз 5: Информатика
INSERT INTO Quizzes (Title, Description) VALUES ('Информатика', 'Тест на знание основ информатики и программирования') RETURNING id;

WITH cs_quiz AS (
    SELECT id FROM Quizzes WHERE Title = 'Информатика'
)
INSERT INTO Questions (QuizId, Text, CorrectAnswer) VALUES
((SELECT id FROM cs_quiz), 'Сколько бит в одном байте?', '8'),
((SELECT id FROM cs_quiz), 'Какой язык программирования был создан первым?', 'Фортран'),
((SELECT id FROM cs_quiz), 'Что такое HTML?', 'Язык разметки гипертекста'),
((SELECT id FROM cs_quiz), 'Какой протокол используется для отправки электронной почты?', 'SMTP'),
((SELECT id FROM cs_quiz), 'Что означает аббревиатура CPU?', 'Central Processing Unit'),
((SELECT id FROM cs_quiz), 'Какой тип памяти является энергозависимым?', 'RAM'),
((SELECT id FROM cs_quiz), 'Какая система счисления используется в компьютерах?', 'Двоичная'),
((SELECT id FROM cs_quiz), 'Что такое SQL?', 'Язык структурированных запросов'),
((SELECT id FROM cs_quiz), 'Какой протокол используется для просмотра веб-страниц?', 'HTTP'),
((SELECT id FROM cs_quiz), 'Что означает аббревиатура URL?', 'Uniform Resource Locator');

-- Добавляем варианты ответов для всех вопросов
-- Здесь нужно добавить INSERT INTO QuestionOptions для каждого вопроса
-- Примеры для некоторых вопросов:

INSERT INTO QuestionOptions (QuestionId, Text) VALUES
((SELECT Id FROM Questions WHERE Text = 'Сколько бит в одном байте?'), '4'),
((SELECT Id FROM Questions WHERE Text = 'Сколько бит в одном байте?'), '8'),
((SELECT Id FROM Questions WHERE Text = 'Сколько бит в одном байте?'), '16'),
((SELECT Id FROM Questions WHERE Text = 'Сколько бит в одном байте?'), '32');

INSERT INTO QuestionOptions (QuestionId, Text) VALUES
((SELECT Id FROM Questions WHERE Text = 'Какая система счисления используется в компьютерах?'), 'Двоичная'),
((SELECT Id FROM Questions WHERE Text = 'Какая система счисления используется в компьютерах?'), 'Десятичная'),
((SELECT Id FROM Questions WHERE Text = 'Какая система счисления используется в компьютерах?'), 'Восьмеричная'),
((SELECT Id FROM Questions WHERE Text = 'Какая система счисления используется в компьютерах?'), 'Шестнадцатеричная');

-- И так далее для остальных вопросов... 