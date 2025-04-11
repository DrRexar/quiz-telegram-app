#!/bin/bash

# Применяем миграции
dotnet ef database update

# Добавляем тестовый квиз
dotnet run --seed-db 