using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using TaskManagerApp.Models;

namespace TaskManagerApp;

public partial class MainWindow : Window
{
    // Колекція завдань, що підтримує автоматичне оновлення прив'язаного списку
    private readonly ObservableCollection<TaskItem> _tasks = new();

    public MainWindow()
    {
        InitializeComponent();

        // Прив'язка списку до джерела даних
        TasksListBox.ItemsSource = _tasks;

        // Підписка на зміни в колекції (додавання / видалення)
        _tasks.CollectionChanged += OnTasksCollectionChanged;

        // Початкові демонстраційні дані
        AddTaskInternal("Підготувати практичну роботу з Avalonia UI", false);
        AddTaskInternal("Вивчити роботу з XAML та обробниками подій", true);
        AddTaskInternal("Оформити звіт та завантажити на GitHub", false);

        UpdateCounters();
    }

    // =========================================================================
    // ОБРОБНИКИ ПОДІЙ КОРИСТУВАЧА
    // =========================================================================

    // 1. Обробник кнопки «Додати»
    private void OnAddTaskClicked(object? sender, RoutedEventArgs e)
    {
        AddNewTask();
    }

    // 2. Додавання завдання при натисканні клавіші Enter у полі введення
    private void OnTaskInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddNewTask();
        }
    }

    // 3. Обробник видалення вибраного завдання у списку
    private void OnDeleteSelectedClicked(object? sender, RoutedEventArgs e)
    {
        if (TasksListBox.SelectedItem is TaskItem selectedTask)
        {
            _tasks.Remove(selectedTask);
            ValidationMessageTextBlock.IsVisible = false;
        }
        else
        {
            ValidationMessageTextBlock.Text = "⚠️ Будь ласка, виберіть завдання зі списку для видалення.";
            ValidationMessageTextBlock.IsVisible = true;
        }
    }

    // 4. Обробник видалення конкретного завдання за кнопкою на елементі
    private void OnDeleteSingleTaskClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: TaskItem task })
        {
            _tasks.Remove(task);
            ValidationMessageTextBlock.IsVisible = false;
        }
    }

    // 5. Обробник кнопки очищення всіх виконаних завдань
    private void OnClearCompletedClicked(object? sender, RoutedEventArgs e)
    {
        var completedTasks = _tasks.Where(t => t.IsCompleted).ToList();
        foreach (var task in completedTasks)
        {
            _tasks.Remove(task);
        }
        ValidationMessageTextBlock.IsVisible = false;
    }

    // =========================================================================
    // ДОПОМІЖНА ЛОГІКА ТА ВАЛІДАЦІЯ
    // =========================================================================

    private void AddNewTask()
    {
        string text = TaskInputTextBox.Text?.Trim() ?? string.Empty;

        // Вимога 8: Перевірка порожнього введення
        if (string.IsNullOrWhiteSpace(text))
        {
            ValidationMessageTextBlock.Text = "⚠️ Завдання не може бути порожнім! Введіть назву.";
            ValidationMessageTextBlock.IsVisible = true;
            TaskInputTextBox.Focus();
            return;
        }

        // Приховуємо повідомлення про помилку, якщо введення коректне
        ValidationMessageTextBlock.IsVisible = false;

        // Додаємо нове завдання до колекції
        AddTaskInternal(text, false);

        // Очищаємо поле введення та повертаємо фокус
        TaskInputTextBox.Text = string.Empty;
        TaskInputTextBox.Focus();
    }

    private void AddTaskInternal(string title, bool isCompleted)
    {
        var task = new TaskItem(title, isCompleted);
        _tasks.Add(task);
    }

    // Відстеження змін у колекції для підключення обробників властивостей елементів
    private void OnTasksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (TaskItem task in e.NewItems)
            {
                task.PropertyChanged += OnTaskPropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (TaskItem task in e.OldItems)
            {
                task.PropertyChanged -= OnTaskPropertyChanged;
            }
        }

        UpdateCounters();
    }

    // Автоматичне оновлення лічильника при зміні стану CheckBox (IsCompleted)
    private void OnTaskPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TaskItem.IsCompleted))
        {
            UpdateCounters();
        }
    }

    // Додаткове завдання: Лічильник невиконаних завдань
    private void UpdateCounters()
    {
        int total = _tasks.Count;
        int uncompleted = _tasks.Count(t => !t.IsCompleted);
        int completed = total - uncompleted;

        CounterTextBlock.Text = $"⏳ Невиконаних завдань: {uncompleted}";
        TotalStatsTextBlock.Text = $"Всього: {total} | Виконано: {completed}";
    }
}
