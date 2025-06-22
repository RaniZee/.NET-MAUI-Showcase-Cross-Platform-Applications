using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace TaskTrackerMAUI.Models
{
    public enum Priority { Low, Medium, High, Critical }
    public enum TaskStatus { New, InProgress, OnReview, Completed }

    [Table("TaskItems")]
    public class TaskItem : INotifyPropertyChanged
    {
        private int _id;
        [PrimaryKey, AutoIncrement]
        public int Id { get => _id; set => SetProperty(ref _id, value); }

        private string _title;
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        private string _description;
        public string Description { get => _description; set => SetProperty(ref _description, value); }

        private DateTime? _dueDate;
        public DateTime? DueDate { get => _dueDate; set { SetProperty(ref _dueDate, value); OnPropertyChanged(nameof(IsOverdue)); } }

        private DateTime? _reminderDateTime;
        public DateTime? ReminderDateTime { get => _reminderDateTime; set => SetProperty(ref _reminderDateTime, value); }

        private Priority _priority;
        public Priority Priority { get => _priority; set => SetProperty(ref _priority, value); }

        private int _categoryId;
        [Indexed]
        public int CategoryId { get => _categoryId; set => SetProperty(ref _categoryId, value); }

        private string _categoryName;
        [Ignore]
        public string CategoryName { get => _categoryName; set => SetProperty(ref _categoryName, value); }

        private TaskStatus _status;
        public TaskStatus Status { get => _status; set { SetProperty(ref _status, value); OnPropertyChanged(nameof(IsOverdue)); } }

        private DateTime _createdDate;
        public DateTime CreatedDate { get => _createdDate; set => SetProperty(ref _createdDate, value); }

        private DateTime _modifiedDate;
        public DateTime ModifiedDate { get => _modifiedDate; set => SetProperty(ref _modifiedDate, value); }

        [Ignore]
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && Status != TaskStatus.Completed;

        public TaskItem()
        {
            CreatedDate = DateTime.Now; ModifiedDate = DateTime.Now;
            Status = TaskStatus.New; Title = string.Empty; Description = string.Empty;
            ReminderDateTime = null; CategoryName = "Без категории";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;
            backingStore = value;
            OnPropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}