-- Получение информации о таблицах и их колонках
SELECT 
    t.table_name,
    c.column_name,
    c.data_type,
    c.column_default,
    c.is_nullable,
    c.character_maximum_length,
    tc.constraint_type,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM 
    information_schema.tables t
    JOIN information_schema.columns c ON t.table_name = c.table_name
    LEFT JOIN information_schema.key_column_usage kcu ON c.table_name = kcu.table_name AND c.column_name = kcu.column_name
    LEFT JOIN information_schema.table_constraints tc ON kcu.constraint_name = tc.constraint_name
    LEFT JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name
WHERE 
    t.table_schema = 'public'
    AND t.table_type = 'BASE TABLE'
ORDER BY 
    t.table_name,
    c.ordinal_position;

-- Создание SQL-скрипта для воссоздания структуры базы данных
SELECT '-- Создание таблицы ' || table_name || E'\n' ||
       'CREATE TABLE ' || table_name || ' (' || E'\n' ||
       string_agg('    ' || column_name || ' ' || 
                 data_type || 
                 CASE 
                     WHEN character_maximum_length IS NOT NULL 
                     THEN '(' || character_maximum_length || ')'
                     ELSE ''
                 END ||
                 CASE 
                     WHEN column_default IS NOT NULL 
                     THEN ' DEFAULT ' || column_default
                     ELSE ''
                 END ||
                 CASE 
                     WHEN is_nullable = 'NO' 
                     THEN ' NOT NULL'
                     ELSE ''
                 END,
                 E',\n') ||
       E'\n);' as create_script
FROM information_schema.columns
WHERE table_schema = 'public'
GROUP BY table_name
ORDER BY table_name; 