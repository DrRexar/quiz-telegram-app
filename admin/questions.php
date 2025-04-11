<?php
require_once 'config.php';

if (!isset($_GET['quiz_id'])) {
    header('Location: index.php');
    exit();
}

$quiz_id = $_GET['quiz_id'];

// Получаем информацию о квизе
$stmt = $pdo->prepare("SELECT * FROM \"Quizzes\" WHERE \"Id\" = ?");
$stmt->execute([$quiz_id]);
$quiz = $stmt->fetch(PDO::FETCH_ASSOC);

if (!$quiz) {
    header('Location: index.php');
    exit();
}

// Получаем список вопросов
$stmt = $pdo->prepare("SELECT * FROM \"Questions\" WHERE \"QuizId\" = ? ORDER BY \"Id\"");
$stmt->execute([$quiz_id]);
$questions = $stmt->fetchAll(PDO::FETCH_ASSOC);

// Обработка удаления вопроса
if (isset($_POST['delete']) && isset($_POST['question_id'])) {
    try {
        $stmt = $pdo->prepare("DELETE FROM \"Questions\" WHERE \"Id\" = ? AND \"QuizId\" = ?");
        $stmt->execute([$_POST['question_id'], $quiz_id]);
        header("Location: questions.php?quiz_id=$quiz_id");
        exit();
    } catch (PDOException $e) {
        $error = 'Ошибка при удалении вопроса: ' . $e->getMessage();
    }
}
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Вопросы квиза</title>
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
            <h2>Вопросы квиза: <?php echo htmlspecialchars($quiz['Title']); ?></h2>
            <a href="question_edit.php?quiz_id=<?php echo $quiz_id; ?>" class="btn btn-primary">Добавить вопрос</a>
        </div>

        <?php if (isset($error)): ?>
            <div class="alert alert-danger"><?php echo htmlspecialchars($error); ?></div>
        <?php endif; ?>

        <div class="table-responsive">
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Вопрос</th>
                        <th>Варианты ответов</th>
                        <th>Правильный ответ</th>
                        <th>Действия</th>
                    </tr>
                </thead>
                <tbody>
                    <?php foreach ($questions as $question): ?>
                        <tr>
                            <td><?php echo $question['Id']; ?></td>
                            <td><?php echo htmlspecialchars($question['Text']); ?></td>
                            <td>
                                <?php
                                if (str_starts_with($question['Options'], '[')) {
                                    // Если это JSON массив
                                    $options = json_decode($question['Options'], true);
                                    if ($options === null) {
                                        echo 'Ошибка формата';
                                    } else {
                                        echo implode('<br>', array_map('htmlspecialchars', $options));
                                    }
                                } else {
                                    // Если это строка с разделителями
                                    $options = $question['Options'] ? explode(',', $question['Options']) : [];
                                    echo implode('<br>', array_map('htmlspecialchars', $options));
                                }
                                ?>
                            </td>
                            <td><?php echo htmlspecialchars($question['CorrectAnswer']); ?></td>
                            <td>
                                <a href="question_edit.php?quiz_id=<?php echo $quiz_id; ?>&id=<?php echo $question['Id']; ?>" class="btn btn-sm btn-primary">Редактировать</a>
                                <form method="POST" class="d-inline" onsubmit="return confirm('Вы уверены, что хотите удалить этот вопрос?');">
                                    <input type="hidden" name="question_id" value="<?php echo $question['Id']; ?>">
                                    <button type="submit" name="delete" class="btn btn-sm btn-danger">Удалить</button>
                                </form>
                            </td>
                        </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        </div>

        <div class="mt-4">
            <a href="index.php" class="btn btn-secondary">Назад к списку квизов</a>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html> 