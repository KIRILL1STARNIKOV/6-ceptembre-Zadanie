using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace _6ceptebre
{

    public partial class MainWindow : Window
    {
        //объедки
        private DispatcherTimer _timer;
        private TimeSpan _timeLeft;

        private ObservableCollection<Alarm> _alarms = new ObservableCollection<Alarm>();
        private DispatcherTimer _alarmTimer;

        private int _selectedNoteIndex = -1; // Индекс выбранной заметки
        private ObservableCollection<string> _notes = new ObservableCollection<string>();
        public MainWindow()
        {
            InitializeComponent();
            // Таймер
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            // Будильник
            AlarmsListView.ItemsSource = _alarms;

            _alarmTimer = new DispatcherTimer();
            _alarmTimer.Interval = TimeSpan.FromSeconds(1);
            _alarmTimer.Tick += AlarmTimer_Tick;
            _alarmTimer.Start();

            // Заметки
            NotesListView.ItemsSource = _notes;
        }
        //Таймер
        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            int hours = 0, minutes = 0, seconds = 0;


            if (!int.TryParse(HoursTextBox.Text, out hours))
            {
                hours = 0;
            }
            if (!int.TryParse(MinutesTextBox.Text, out minutes))
            {
                minutes = 0;
            }
            if (!int.TryParse(SecondsTextBox.Text, out seconds))
            {
                seconds = 0;
            }

            // проверочка на правильность
            if (minutes >= 0 && minutes < 60 && seconds >= 0 && seconds < 60)
            {
                _timeLeft = new TimeSpan(hours, minutes, seconds);
                TimerDisplayLabel.Text = _timeLeft.ToString(@"hh\:mm\:ss");
                _timer.Start();
            }
            else
            {
                MessageBox.Show("Неверные значения. Пожалуйста, введите корректные минуты и секунды (0-59).");
            }
        }
        //отсчет времени
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_timeLeft.TotalSeconds > 0)
            {
                _timeLeft = _timeLeft.Subtract(TimeSpan.FromSeconds(1));
                TimerDisplayLabel.Text = _timeLeft.ToString(@"hh\:mm\:ss");
            }
            else
            {
                _timer.Stop();
                TimerDisplayLabel.Text = "00:00:00";
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show("Время вышло!");
            }
        }

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        private void ResetTimer_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _timeLeft = TimeSpan.Zero;
            TimerDisplayLabel.Text = "00:00:00";
        }

        // Будильник
        private void AddAlarm_Click(object sender, RoutedEventArgs e)
        {
            DateTime date = AlarmDatePicker.SelectedDate ?? DateTime.Now;
            int hours = int.Parse(AlarmHoursTextBox.Text);
            int minutes = int.Parse(AlarmMinutesTextBox.Text);

            DateTime alarmTime = new DateTime(date.Year, date.Month, date.Day, hours, minutes, 0);

            _alarms.Add(new Alarm
            {
                AlarmTime = alarmTime,
                IsActive = true
            });
        }

        private void AlarmTimer_Tick(object sender, EventArgs e)
        {
            foreach (var alarm in _alarms.Where(a => a.IsActive))
            {
                if (DateTime.Now >= alarm.AlarmTime)
                {
                    alarm.IsActive = false;
                    System.Media.SystemSounds.Beep.Play();
                    MessageBox.Show($"Будильник на {alarm.DisplayTime} сработал!");
                }
            }
        }

        private void RemoveAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (AlarmsListView.SelectedItem is Alarm selectedAlarm)
            {
                _alarms.Remove(selectedAlarm);
            }
        }

        // Заметки
        private void SaveOrEditNote_Click(object sender, RoutedEventArgs e)
        {
            string noteText = NoteTextBox.Text;

            // Если нет введенного текста - ничего не делаем
            if (string.IsNullOrWhiteSpace(noteText))
            {
                MessageBox.Show("Заметка не может быть пустой.");
                return;
            }

            // Если выбрана заметка - обновляем ее
            if (_selectedNoteIndex >= 0 && _selectedNoteIndex < _notes.Count)
            {
                _notes[_selectedNoteIndex] = noteText; // Обновляем заметку
                NotesListView.Items.Refresh(); // Обновляем отображение списка
            }
            else
            {
                // Если заметка не выбрана, добавляем новую
                _notes.Add(noteText);
            }

            // Очищаем поле после сохранения или добавления
            NoteTextBox.Clear();
            _selectedNoteIndex = -1;
        }

        // Удаление заметки
        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (NotesListView.SelectedItem is string selectedNote)
            {
                _notes.Remove(selectedNote);
                NoteTextBox.Clear(); // Очищаем поле после удаления
                _selectedNoteIndex = -1;
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заметку для удаления.");
            }
        }

        // Выбор заметки для редактирования
        private void NotesListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (NotesListView.SelectedItem is string selectedNote)
            {
                NoteTextBox.Text = selectedNote; // Показываем выбранную заметку для редактирования
                _selectedNoteIndex = NotesListView.SelectedIndex; // Запоминаем индекс выбранной заметки
            }
        }
    }
    //временно хранит данные
    public class Alarm
    {
        public DateTime AlarmTime { get; set; }
        public bool IsActive { get; set; }
        public string DisplayTime => AlarmTime.ToString("g");
    }
}
