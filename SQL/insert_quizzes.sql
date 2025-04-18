-- Очистка существующих данных (если нужно)
DELETE FROM "Answers";
DELETE FROM "QuestionOptions";
DELETE FROM "Questions";
DELETE FROM "QuizResults";
DELETE FROM "Quizzes";

-- Вставка квизов
INSERT INTO "Quizzes" ("Id", "Title", "Description", "CreatedAt") VALUES
(1, 'История России', 'Тест по истории России от древности до современности', NOW()),
(2, 'География мира', 'Проверьте свои знания о странах, столицах и географических особенностях', NOW()),
(3, 'Программирование', 'Тест по основам программирования и алгоритмам', NOW()),
(4, 'Кино и сериалы', 'Проверьте свои знания о популярных фильмах и сериалах', NOW()),
(5, 'Наука и техника', 'Тест о научных открытиях и технических достижениях', NOW());

-- Вставка вопросов для квиза "История России"
INSERT INTO "Questions" ("Text", "Options", "CorrectAnswer", "Points", "QuizId") VALUES
('В каком году произошло Крещение Руси?', 
'["988", "1054", "1240", "1380"]', 
'988', 
10, 
1),
('Кто был первым русским царем?', 
'["Иван III", "Иван IV (Грозный)", "Петр I", "Александр I"]', 
'Иван IV (Грозный)', 
10, 
1),
('В каком году была Куликовская битва?', 
'["1240", "1380", "1480", "1552"]', 
'1380', 
10, 
1),
('Кто основал Санкт-Петербург?', 
'["Иван Грозный", "Петр I", "Екатерина II", "Александр I"]', 
'Петр I', 
10, 
1),
('В каком году произошла Октябрьская революция?', 
'["1905", "1914", "1917", "1922"]', 
'1917', 
10, 
1),
('Кто был первым президентом России?', 
'["Михаил Горбачев", "Борис Ельцин", "Владимир Путин", "Дмитрий Медведев"]', 
'Борис Ельцин', 
10, 
1),
('В каком году был запущен первый искусственный спутник Земли?', 
'["1957", "1961", "1969", "1975"]', 
'1957', 
10, 
1),
('Кто написал "Войну и мир"?', 
'["Ф.М. Достоевский", "Л.Н. Толстой", "А.С. Пушкин", "М.Ю. Лермонтов"]', 
'Л.Н. Толстой', 
10, 
1),
('В каком году была отменена крепостное право?', 
'["1801", "1825", "1861", "1905"]', 
'1861', 
10, 
1),
('Кто был последним российским императором?', 
'["Александр II", "Александр III", "Николай II", "Михаил II"]', 
'Николай II', 
10, 
1);

-- Вставка вопросов для квиза "География мира"
INSERT INTO "Questions" ("Text", "Options", "CorrectAnswer", "Points", "QuizId") VALUES
('Какая самая длинная река в мире?', 
'["Нил", "Амазонка", "Янцзы", "Миссисипи"]', 
'Нил', 
10, 
2),
('Какая самая высокая гора в мире?', 
'["К2", "Эверест", "Килиманджаро", "Аконкагуа"]', 
'Эверест', 
10, 
2),
('Какая самая большая пустыня в мире?', 
'["Сахара", "Гоби", "Атакама", "Каракумы"]', 
'Сахара', 
10, 
2),
('Какая самая большая страна по площади?', 
'["Канада", "Китай", "США", "Россия"]', 
'Россия', 
10, 
2),
('Какая столица Австралии?', 
'["Сидней", "Мельбурн", "Канберра", "Брисбен"]', 
'Канберра', 
10, 
2),
('Какое самое глубокое озеро в мире?', 
'["Каспийское море", "Байкал", "Танганьика", "Верхнее"]', 
'Байкал', 
10, 
2),
('Какая самая большая по населению страна?', 
'["Индия", "Китай", "США", "Индонезия"]', 
'Китай', 
10, 
2),
('Какая столица Бразилии?', 
'["Рио-де-Жанейро", "Сан-Паулу", "Бразилиа", "Сальвадор"]', 
'Бразилиа', 
10, 
2),
('Какое море самое соленое?', 
'["Средиземное", "Красное", "Мертвое", "Черное"]', 
'Мертвое', 
10, 
2),
('Какая самая маленькая страна в мире?', 
'["Монако", "Ватикан", "Сан-Марино", "Лихтенштейн"]', 
'Ватикан', 
10, 
2);

-- Вставка вопросов для квиза "Программирование"
INSERT INTO "Questions" ("Text", "Options", "CorrectAnswer", "Points", "QuizId") VALUES
('Какой язык программирования был создан первым?', 
'["Fortran", "C", "Python", "Java"]', 
'Fortran', 
10, 
3),
('Что такое ООП?', 
'["Объектно-ориентированное программирование", "Операционная система", "Открытый исходный код", "Объектная база данных"]', 
'Объектно-ориентированное программирование', 
10, 
3),
('Какой язык используется для веб-разработки?', 
'["Python", "JavaScript", "C++", "Java"]', 
'JavaScript', 
10, 
3),
('Что такое Git?', 
'["Язык программирования", "Система контроля версий", "База данных", "Фреймворк"]', 
'Система контроля версий', 
10, 
3),
('Какой тип данных используется для хранения целых чисел?', 
'["String", "Boolean", "Integer", "Float"]', 
'Integer', 
10, 
3),
('Что такое API?', 
'["Язык программирования", "Интерфейс программирования приложений", "База данных", "Операционная система"]', 
'Интерфейс программирования приложений', 
10, 
3),
('Какой язык используется для стилизации веб-страниц?', 
'["HTML", "JavaScript", "CSS", "PHP"]', 
'CSS', 
10, 
3),
('Что такое SQL?', 
'["Язык программирования", "Язык запросов к базе данных", "Фреймворк", "Библиотека"]', 
'Язык запросов к базе данных', 
10, 
3),
('Какой язык используется для машинного обучения?', 
'["Java", "Python", "C#", "Ruby"]', 
'Python', 
10, 
3),
('Что такое Docker?', 
'["Язык программирования", "Система виртуализации", "База данных", "Фреймворк"]', 
'Система виртуализации', 
10, 
3);

-- Вставка вопросов для квиза "Кино и сериалы"
INSERT INTO "Questions" ("Text", "Options", "CorrectAnswer", "Points", "QuizId") VALUES
('Кто снял фильм "Титаник"?', 
'["Стивен Спилберг", "Джеймс Кэмерон", "Кристофер Нолан", "Квентин Тарантино"]', 
'Джеймс Кэмерон', 
10, 
4),
('Какой фильм получил больше всего Оскаров?', 
'["Титаник", "Властелин колец: Возвращение короля", "Бен-Гур", "Титаник и Властелин колец"]', 
'Властелин колец: Возвращение короля', 
10, 
4),
('Кто сыграл главную роль в "Матрице"?', 
'["Том Круз", "Брэд Питт", "Киану Ривз", "Леонардо ДиКаприо"]', 
'Киану Ривз', 
10, 
4),
('Какой сериал является самым длинным в истории?', 
'["Доктор Кто", "Симсоны", "Санта-Барбара", "Секретные материалы"]', 
'Санта-Барбара', 
10, 
4),
('Кто снял "Крестного отца"?', 
'["Мартин Скорсезе", "Фрэнсис Форд Коппола", "Стивен Спилберг", "Квентин Тарантино"]', 
'Фрэнсис Форд Коппола', 
10, 
4),
('Какой фильм получил первый Оскар за лучший фильм?', 
'["Унесенные ветром", "Крылья", "Касабланка", "Унесенные ветром"]', 
'Крылья', 
10, 
4),
('Кто сыграл Джеймса Бонда в первом фильме?', 
'["Шон Коннери", "Роджер Мур", "Пирс Броснан", "Дэниел Крэйг"]', 
'Шон Коннери', 
10, 
4),
('Какой фильм является самым кассовым в истории?', 
'["Титаник", "Аватар", "Мстители: Финал", "Звездные войны: Пробуждение силы"]', 
'Аватар', 
10, 
4),
('Кто снял "Психо"?', 
'["Альфред Хичкок", "Стэнли Кубрик", "Орсон Уэллс", "Федерико Феллини"]', 
'Альфред Хичкок', 
10, 
4),
('Какой сериал HBO стал самым популярным?', 
'["Сопрано", "Игра престолов", "Настоящий детектив", "Прослушка"]', 
'Игра престолов', 
10, 
4);

-- Вставка вопросов для квиза "Наука и техника"
INSERT INTO "Questions" ("Text", "Options", "CorrectAnswer", "Points", "QuizId") VALUES
('Кто открыл закон всемирного тяготения?', 
'["Альберт Эйнштейн", "Исаак Ньютон", "Галилео Галилей", "Никола Тесла"]', 
'Исаак Ньютон', 
10, 
5),
('Какой ученый открыл радиоактивность?', 
'["Мария Кюри", "Альберт Эйнштейн", "Нильс Бор", "Эрнест Резерфорд"]', 
'Мария Кюри', 
10, 
5),
('Кто изобрел телефон?', 
'["Томас Эдисон", "Александр Белл", "Никола Тесла", "Гульельмо Маркони"]', 
'Александр Белл', 
10, 
5),
('Какой ученый сформулировал теорию относительности?', 
'["Исаак Ньютон", "Альберт Эйнштейн", "Стивен Хокинг", "Ричард Фейнман"]', 
'Альберт Эйнштейн', 
10, 
5),
('Кто изобрел лампочку?', 
'["Томас Эдисон", "Никола Тесла", "Александр Белл", "Генри Форд"]', 
'Томас Эдисон', 
10, 
5),
('Какой ученый открыл ДНК?', 
'["Джеймс Уотсон и Фрэнсис Крик", "Грегор Мендель", "Луи Пастер", "Чарльз Дарвин"]', 
'Джеймс Уотсон и Фрэнсис Крик', 
10, 
5),
('Кто изобрел первый компьютер?', 
'["Билл Гейтс", "Стив Джобс", "Чарльз Бэббидж", "Алан Тьюринг"]', 
'Чарльз Бэббидж', 
10, 
5),
('Какой ученый открыл пенициллин?', 
'["Луи Пастер", "Александр Флеминг", "Роберт Кох", "Джозеф Листер"]', 
'Александр Флеминг', 
10, 
5),
('Кто изобрел первый автомобиль?', 
'["Генри Форд", "Карл Бенц", "Рудольф Дизель", "Готлиб Даймлер"]', 
'Карл Бенц', 
10, 
5),
('Какой ученый открыл электрон?', 
'["Эрнест Резерфорд", "Джозеф Томсон", "Нильс Бор", "Макс Планк"]', 
'Джозеф Томсон', 
10, 
5); 