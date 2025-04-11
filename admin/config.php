<?php
session_start();

// Параметры подключения к базе данных
define('DB_HOST', 'localhost');
define('DB_NAME', 'quizapp');
define('DB_USER', 'postgres');
define('DB_PASS', '12345');

try {
    $pdo = new PDO(
        "pgsql:host=" . DB_HOST . ";dbname=" . DB_NAME,
        DB_USER,
        DB_PASS,
        [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
    );
} catch (PDOException $e) {
    die("Ошибка подключения к базе данных: " . $e->getMessage());
}

// Проверка авторизации
if (!isset($_SESSION['admin']) && basename($_SERVER['PHP_SELF']) !== 'login.php') {
    header('Location: login.php');
    exit();
}
?> 