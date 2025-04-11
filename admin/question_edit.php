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

$question = [
    'Id' => 0,
    'Text' => '',
    'Options' => '[]',
    'CorrectAnswer' => '',
    'Points' => 1
];

if (isset($_GET['id'])) {
    $stmt = $pdo->prepare("SELECT * FROM \"Questions\" WHERE \"Id\" = ? AND \"QuizId\" = ?");
    $stmt->execute([$_GET['id'], $quiz_id]);
    $question = $stmt->fetch(PDO::FETCH_ASSOC);
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $text = $_POST['text'] ?? '';
    $options = array_map('trim', array_filter(explode("\n", $_POST['options'] ?? '')));
    $correct_answer = trim($_POST['correct_answer'] ?? '');
    $points = intval($_POST['points'] ?? 1);

    if (empty($text) || empty($options) || empty($correct_answer)) {
        $error = 'Все поля должны быть заполнены';
    } else {
        // Проверяем, есть ли правильный ответ среди вариантов (игнорируя регистр)
        $correct_answer_exists = false;
        foreach ($options as $option) {
            if (strtolower(trim($option)) === strtolower($correct_answer)) {
                $correct_answer_exists = true;
                break;
            }
        }

        if (!$correct_answer_exists) {
            $error = 'Правильный ответ должен быть одним из вариантов ответа';
        } else {
            try {
                $options_json = json_encode($options);
                if ($question['Id'] > 0) {
                    // Обновление существующего вопроса
                    $stmt = $pdo->prepare("UPDATE \"Questions\" SET \"Text\" = ?, \"Options\" = ?, \"CorrectAnswer\" = ?, \"Points\" = ? WHERE \"Id\" = ? AND \"QuizId\" = ?");
                    $stmt->execute([$text, $options_json, $correct_answer, $points, $question['Id'], $quiz_id]);
                } else {
                    // Создание нового вопроса
                    $stmt = $pdo->prepare("INSERT INTO \"Questions\" (\"Text\", \"Options\", \"CorrectAnswer\", \"QuizId\", \"Points\") VALUES (?, ?, ?, ?, ?)");
                    $stmt->execute([$text, $options_json, $correct_answer, $quiz_id, $points]);
                }
                header("Location: questions.php?quiz_id=$quiz_id");
                exit();
            } catch (PDOException $e) {
                $error = 'Ошибка при сохранении вопроса: ' . $e->getMessage();
            }
        }
    }
}
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title><?php echo $question['Id'] ? 'Редактирование' : 'Создание'; ?> вопроса</title>
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
        <h2><?php echo $question['Id'] ? 'Редактирование' : 'Создание'; ?> вопроса</h2>
        <p class="text-muted">Квиз: <?php echo htmlspecialchars($quiz['Title']); ?></p>

        <?php if (isset($error)): ?>
            <div class="alert alert-danger"><?php echo htmlspecialchars($error); ?></div>
        <?php endif; ?>

        <form method="POST" class="mt-4">
            <div class="mb-3">
                <label for="text" class="form-label">Вопрос</label>
                <textarea class="form-control" id="text" name="text" rows="3" required><?php echo htmlspecialchars($question['Text']); ?></textarea>
            </div>

            <div class="mb-3">
                <label for="options" class="form-label">Варианты ответов (по одному в строке)</label>
                <textarea class="form-control" id="options" name="options" rows="5" required><?php echo implode("\n", json_decode($question['Options'], true)); ?></textarea>
                <div class="form-text">Каждый вариант ответа должен быть на новой строке</div>
            </div>

            <div class="mb-3">
                <label for="correct_answer" class="form-label">Правильный ответ</label>
                <input type="text" class="form-control" id="correct_answer" name="correct_answer" value="<?php echo htmlspecialchars($question['CorrectAnswer']); ?>" required>
                <div class="form-text">Правильный ответ должен точно совпадать с одним из вариантов ответа</div>
            </div>

            <div class="mb-3">
                <label for="points" class="form-label">Баллы за правильный ответ</label>
                <input type="number" class="form-control" id="points" name="points" value="<?php echo htmlspecialchars($question['Points']); ?>" min="1" required>
                <div class="form-text">Сколько баллов получит пользователь за правильный ответ</div>
            </div>

            <div class="mb-3">
                <button type="submit" class="btn btn-primary">Сохранить</button>
                <a href="questions.php?quiz_id=<?php echo $quiz_id; ?>" class="btn btn-secondary">Отмена</a>
            </div>
        </form>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html> 