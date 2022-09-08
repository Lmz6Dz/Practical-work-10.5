using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace Practical_work_10._5
{
    internal class TelegramMessageClient
    {
        private readonly MainWindow w;

        private static readonly string Token = System.IO.File.ReadAllText(@"token");
        public static readonly TelegramBotClient Bot = new(Token);
        public static CancellationTokenSource Cts = new();
        public long Bid = (long)Bot.BotId;

        public ObservableCollection<MessageLog> BotMessageLog { get; set; }

        public TelegramMessageClient(MainWindow w)
        {
            this.BotMessageLog = new ObservableCollection<MessageLog>();
            this.w = w;

            var receiverOptions = new Telegram.Bot.Polling.ReceiverOptions
            {
                AllowedUpdates = { }
            };

            Bot.StartReceiving(
                HandleUpdatesAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: Cts.Token);
        }

        public Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            w.Dispatcher.Invoke(() =>
            {
                ((MainWindow)System.Windows.Application.Current.MainWindow).TextBlockError.Text = errorMessage;
            });
            
            return Task.CompletedTask;
        }

        public async Task HandleUpdatesAsync(ITelegramBotClient bot, Update update,
            CancellationToken cancellationToken)
        {
            if (update.Message != null)
            {
                switch (update.Message.Type)
                {
                    case MessageType.Document:
                        {
                            var newName = await DownLoad(update.Message.Document.FileId,
                                update.Message.Document.FileName, update.Message.Chat.Id);
                            await bot.SendTextMessageAsync(update.Message.Chat.Id,
                                $"файл {update.Message.Document.FileName} загружен под именем {newName}",
                                cancellationToken: Cts.Token);
                            break;
                        }
                    case MessageType.Audio:
                        {
                            var newName = await DownLoad(update.Message.Audio.FileId, update.Message.Audio.FileName,
                                update.Message.Chat.Id);
                            await bot.SendTextMessageAsync(update.Message.Chat.Id,
                                $"файл {update.Message.Audio.FileName} загружен под именем {newName}", cancellationToken: Cts.Token);
                            break;
                        }
                    case MessageType.Voice:
                        {
                            var newName = await DownLoad(update.Message.Voice.FileId,
                                $"{update.Message.Voice.FileId}.ogg", update.Message.Chat.Id);
                            await bot.SendTextMessageAsync(update.Message.Chat.Id,
                                $" Загружено голосовое сообщение {update.Message.Voice.FileSize} байт под именем {newName}", cancellationToken: Cts.Token);
                            break;
                        }
                    case MessageType.Photo:
                        {
                            var newName = await DownLoad(update.Message.Photo.Last().FileId,
                                $"{update.Message.Photo.Last().FileId}.jpg", update.Message.Chat.Id);
                            await bot.SendTextMessageAsync(update.Message.Chat.Id,
                                $"Загружено изображение {update.Message.Photo.Last().FileSize} байт под именем {newName}", cancellationToken: Cts.Token);
                            break;
                        }
                    case MessageType.Text:
                        {
                            await HandleMessage(bot, update.Message, update.Message.Chat.Id);
                            break;
                        }
                    default: return;
                }
            }
        }

        private static async Task<string> DownLoad(string fileId, string path, long directory)
        {
            if (!Directory.Exists($"{directory}")) Directory.CreateDirectory($"{directory}");

            var fileName = path;

            //сохранение копии
            if (System.IO.File.Exists($"{directory}/{fileName}"))
            {
                var n = 1;
                while (System.IO.File.Exists($"{directory}/{fileName}"))
                {
                    fileName = Path.Combine(
                        Path.GetDirectoryName($"{directory}"),
                        Path.GetFileNameWithoutExtension(path) + " (" + n.ToString() + ")" + Path.GetExtension(path));
                    n++;
                }
            }

            FileStream fs = new($"{directory}/" + fileName, FileMode.Create);

            await Bot.DownloadFileAsync(Bot.GetFileAsync(fileId).Result.FilePath, fs);

            fs.Close();

            fs.Dispose();

            return fileName;
        }

        public async Task HandleMessage(ITelegramBotClient bot, Message message, long directory)
        {
            switch (message.Text)
            {
                case "/start":
                    {
                        await bot.SendTextMessageAsync(message.Chat.Id, "Здравия, вас приветствует бот для закачки файлов. " +
                                                                        "Отправьте документы, фото, аудио, голосовое сообщение для сохранения на диск. " +
                                                                        "Для просмотра и скачивания введите команду /download");
                        break;
                    }
                case "/download":
                    {
                        if (Directory.Exists($"{directory}"))
                        {
                            await bot.SendTextMessageAsync(message.Chat.Id, "Список ранее загруженных файлов: ");

                            var file = Directory.GetFiles($"{directory}").Select(fn => Path.GetFileName(fn)).ToArray();

                            foreach (var s in file)
                            {
                                var pathfull = Path.Combine(Directory.GetCurrentDirectory(), $"{directory}", s);

                                using var stream = System.IO.File.Open(pathfull, FileMode.Open);

                                await bot.SendDocumentAsync(message.Chat.Id, new InputOnlineFile(stream, s));
                            }
                        }
                        else
                            await bot.SendTextMessageAsync(message.Chat.Id, "Файлы не были отправлены, нечего загружать");
                        break;
                    }
                default:
                    {
                        w.Dispatcher.Invoke(() =>
                        {
                            BotMessageLog.Add(
                                new MessageLog(
                                    DateTime.Now.ToLongTimeString(), message.Text, message.Chat.FirstName, message.Chat.Id, message.MessageId));
                        });

                        var l = BotMessageLog.Last().Msg.Length;

                        var json = JsonConvert.SerializeObject(BotMessageLog.Last());

                        System.IO.File.AppendAllText(Convert.ToString(Bid), json);

                        //ширина столбца с сообщениями
                        w.Dispatcher.Invoke(() =>
                        {
                            if (l > 13)
                            {
                                ((MainWindow)System.Windows.Application.Current.MainWindow).Msg.Width = l * 7.7;
                            }
                        });

                        break;
                    }
            }
        }

        public void SendMessage(string text, string id, int mid)
        {
            Bot.SendTextMessageAsync(Convert.ToInt64(id), text, replyToMessageId: mid);
        }
    }
}