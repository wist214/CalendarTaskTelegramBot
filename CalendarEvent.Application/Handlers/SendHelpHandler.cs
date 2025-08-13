using CalendarEvent.Application.Commands;
using CalendarEvent.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CalendarEvent.Application.Handlers
{
    public class SendHelpHandler : IRequestHandler<SendHelpCommand>
    {
        private readonly IMessageSender _messageSender;
        private readonly ILogger<SendHelpHandler> _logger;

        public SendHelpHandler(IMessageSender messageSender, ILogger<SendHelpHandler> logger)
        {
            _messageSender = messageSender;
            _logger = logger;
        }

        public async Task Handle(SendHelpCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Sending help message to user {UserId}", request.UserId);

            const string helpMessage = @"🤖 **Календарный бот - Справка**

**Команды:**
• `/login` - Войти через Google Calendar
• `/logout` - Выйти из аккаунта
• `/help` - Показать эту справку

**Создание задач и событий:**

📝 **Текстовые сообщения:**
• `Купить продукты завтра в 18:00`
• `Встреча с командой в понедельник в 10 утра`
• `Задача написать отчет до пятницы`

🎤 **Голосовые сообщения:**
• Просто запишите голосовое сообщение на русском языке
• Максимальная длительность: 60 секунд
• Говорите четко для лучшего распознавания

**Примеры команд:**
• `Задача: Подготовить презентацию до 25.12.2024 в 15:00`
• `Встреча: Совещание с 26.12.2024 09:00 до 26.12.2024 10:00`
• `Позвонить клиенту сегодня`

**Поддерживаемые форматы дат:**
• `сегодня`, `завтра`, `послезавтра`
• `понедельник`, `вторник`, и т.д.
• `25.12.2024`, `25/12/2024`
• `в 15:00`, `в 15:30`

Для начала работы выполните `/login` для подключения к Google Calendar.";

            await _messageSender.SendMessageAsync(request.ChatId, helpMessage, null, cancellationToken);
        }
    }
}
