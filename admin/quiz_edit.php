<?php
require_once 'config.php';

$quiz = [
    'Id' => 0,
    'Title' => '',
    'Description' => ''
];

if (isset($_GET['id'])) {
    $stmt = $pdo->prepare("SELECT * FROM \"Quizzes\" WHERE \"Id\" = ?");
    $stmt->execute([$_GET['id']]);
    $quiz = $stmt->fetch(PDO::FETCH_ASSOC);
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $title = $_POST['title'] ?? '';
    $description = $_POST['description'] ?? '';

    if (empty($title)) {
        $error = 'Название квиза обязательно';
    } else {
        try {
            if ($quiz['Id'] > 0) {
                // Обновление существующего квиза
                $stmt = $pdo->prepare("UPDATE \"Quizzes\" SET \"Title\" = ?, \"Description\" = ? WHERE \"Id\" = ?");
                $stmt->execute([$title, $description, $quiz['Id']]);
            } else {
                // Создание нового квиза
                $stmt = $pdo->prepare("INSERT INTO \"Quizzes\" (\"Title\", \"Description\", \"CreatedAt\") VALUES (?, ?, CURRENT_TIMESTAMP)");
                $stmt->execute([$title, $description]);
            }
            header('Location: index.php');
            exit();
        } catch (PDOException $e) {
            $error = 'Ошибка при сохранении квиза: ' . $e->getMessage();
        }
    }
}
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title><?php echo $quiz['Id'] ? 'Редактирование' : 'Создание'; ?> квиза</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
</head>
<body>
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
        <div class="container">
            <a class="navbar-brand" href="index.php">Quiz App - Админ-панель</a>
            <div class="navbar-nav ms-auto">
                <a class="nav-link" href="logout.php">Выйти</a>
            </div>
        </div>
    </nav>

    <div class="container mt-4">
        <h2><?php echo $quiz['Id'] ? 'Редактирование' : 'Создание'; ?> квиза</h2>

        <?php if (isset($error)): ?>
            <div class="alert alert-danger"><?php echo htmlspecialchars($error); ?></div>
        <?php endif; ?>

        <form method="POST" class="mt-4">
            <div class="mb-3">
                <label for="title" class="form-label">Название</label>
                <input type="text" class="form-control" id="title" name="title" value="<?php echo htmlspecialchars($quiz['Title']); ?>" required>
            </div>

            <div class="mb-3">
                <label for="description" class="form-label">Описание</label>
                <textarea class="form-control" id="description" name="description" rows="3"><?php echo htmlspecialchars($quiz['Description']); ?></textarea>
            </div>

            <div class="mb-3">
                <button type="submit" class="btn btn-primary">Сохранить</button>
                <a href="index.php" class="btn btn-secondary">Отмена</a>
            </div>
        </form>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html> 