using FinnFragen.Web.Data;
using FinnFragen.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FinnFragen.Web.Services
{
    public class MessagePushService
    {
        private readonly IHubContext<MessageHub, IMessageClient> hubContext;

        public MessagePushService(IHubContext<MessageHub, IMessageClient> hubContext)
        {
            this.hubContext = hubContext;
        }

        public async Task NewMessage(Message message)
        {
            var sendMessage = new IMessageClient.Message()
            {
                Id = message.Id,
                Author = message.MessageAuthor == Message.Author.Asker ? IMessageClient.MessageAuthor.Asker : IMessageClient.MessageAuthor.Answerer,
                Html = message.MessageHtml,
                Text = message.MessageText,
                Seen = message.Seen,
                Sent = message.Date
            };

            hubContext.Clients.Group(message.Question.Id.ToString()).UpdateMessage(sendMessage)
        }
    }
}
