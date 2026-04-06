using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ZooShop.ViewModels;

public class SupportViewModel : ViewModelBase
{
    private string _statusMessage = "";
    private string _senderName = "";
    private string _senderEmail = "";
    private string _subject = "";
    private string _messageText = "";

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public string SenderName
    {
        get => _senderName;
        set => SetField(ref _senderName, value);
    }

    public string SenderEmail
    {
        get => _senderEmail;
        set => SetField(ref _senderEmail, value);
    }

    public string Subject
    {
        get => _subject;
        set => SetField(ref _subject, value);
    }

    public string MessageText
    {
        get => _messageText;
        set => SetField(ref _messageText, value);
    }

    public ICommand SubmitCommand { get; }

    public string[] FAQ { get; } = {
        "❓ Как добавить новый товар?\n→ Перейдите в 'Каталог' и используйте фильтры для поиска. Для добавления нового товара обратитесь к администратору.\n",
        "❓ Как оформить продажу?\n→ Добавьте товары в чек из каталога (кнопка 'В чек'), перейдите в 'Продажи', выберите клиента и склад, нажмите 'Оформить продажу'.\n",
        "❓ Как посмотреть остатки?\n→ Раздел 'Склад' показывает текущие остатки по каждому складу. Используйте форму для приёмки/списания/перемещения.\n",
        "❓ Как добавить клиента и питомца?\n→ Раздел 'Клиенты и питомцы'. Заполните форму клиента, сохраните. Затем добавьте питомца, указав аллергии и породу.\n",
        "❓ Как назначить задачу?\n→ Раздел 'Задачи и расписание'. Заполните форму: название, описание, исполнитель, приоритет. Нажмите 'Создать задачу'.\n",
        "❓ Как выгрузить отчёт?\n→ Раздел 'Отчёты'. Выберите период, сгенерируйте нужный отчёт, нажмите 'Экспорт CSV'.\n",
        "❓ Какие виды животных поддерживаются?\n→ Собаки, Кошки, Птицы, Рыбки, Грызуны. Можно добавлять новые виды через базу данных."
    };

    public SupportViewModel()
    {
        SubmitCommand = new RelayCommand(_ => SubmitFeedback());
    }

    private void SubmitFeedback()
    {
        if (string.IsNullOrWhiteSpace(SenderName) || string.IsNullOrWhiteSpace(MessageText))
        {
            StatusMessage = "Заполните имя и сообщение";
            return;
        }

        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "support.log");
            var entry = $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] От: {SenderName} | Email: {SenderEmail} | Тема: {Subject}\n{MessageText}\n{'─',50}\n";
            File.AppendAllText(logPath, entry);

            StatusMessage = $"✅ Сообщение отправлено! Лог: support.log";
            SenderName = ""; SenderEmail = ""; Subject = ""; MessageText = "";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }
}
