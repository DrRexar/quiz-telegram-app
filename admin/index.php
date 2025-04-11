<?php
require_once 'config.php';

// Получаем список квизов
$quizzes = $pdo->query("SELECT * FROM \"Quizzes\" ORDER BY \"Id\" DESC")->fetchAll(PDO::FETCH_ASSOC);
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Админ-панель Quiz App</title>
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
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2>Список квизов</h2>
            <a href="quiz_edit.php" class="btn btn-primary">Создать новый квиз</a>
        </div>

        <div class="table-responsive">
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Название</th>
                        <th>Описание</th>
                        <th>Количество вопросов</th>
                        <th>Действия</th>
                    </tr>
                </thead>
                <tbody>
                    <?php foreach ($quizzes as $quiz): ?>
                        <tr>
                            <td><?php echo htmlspecialchars($quiz['Id']); ?></td>
                            <td><?php echo htmlspecialchars($quiz['Title']); ?></td>
                            <td><?php echo htmlspecialchars($quiz['Description'] ?? ''); ?></td>
                            <td>
                                <?php
                                $stmt = $pdo->prepare("SELECT COUNT(*) FROM \"Questions\" WHERE \"QuizId\" = ?");
                                $stmt->execute([$quiz['Id']]);
                                echo $stmt->fetchColumn();
                                ?>
                            </td>
                            <td>
                                <a href="quiz_edit.php?id=<?php echo $quiz['Id']; ?>" class="btn btn-sm btn-primary">Редактировать</a>
                                <a href="questions.php?quiz_id=<?php echo $quiz['Id']; ?>" class="btn btn-sm btn-info">Вопросы</a>
                                <a href="quiz_delete.php?id=<?php echo $quiz['Id']; ?>" class="btn btn-sm btn-danger" onclick="return confirm('Вы уверены?')">Удалить</a>
                            </td>
                        </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html> 