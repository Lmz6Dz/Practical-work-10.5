using System.IO;
using System.Windows;

namespace Practical_work_10._5
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            File.Delete(Path.Combine(Directory.GetCurrentDirectory(), TelegramMessageClient.Bot.BotId.Value.ToString()));
        }
    }
}