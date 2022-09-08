using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Practical_work_10._5
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TelegramMessageClient _client;

        public MainWindow()
        {
            InitializeComponent();

            _client = new TelegramMessageClient(this);

            ViewList.ItemsSource = _client.BotMessageLog;
        }

        // Выход
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private MessageLog _f;

        // Отправить
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _f = ViewList.SelectedItem as MessageLog;

            if (_f != null)
            {
                _client.SendMessage(TextBox.Text, _f.Id.ToString(), _f.MessageId);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Некому отправлять!", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            TextBox.Clear();
        }


        // Сохранить историю
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Json file (*.json)|*.json",
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (_client.BotMessageLog.Count != 0)
            {
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.Copy(Convert.ToString(_client.Bid), saveFileDialog.FileName);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Нечего сохранять!", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Справка - О программе
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Skillbox Student", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Файл - Список присланных файлов
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            _f = ViewList.SelectedItem as MessageLog;

            if (_f != null)
            {
                var p = Path.Combine(Directory.GetCurrentDirectory(), $"{_f.Id}");

                if (Directory.Exists($"{p}"))
                {
                    Process.Start("explorer.exe", $"{p}");
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Ничего не отправлено", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Выберите пользователя из таблицы", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}