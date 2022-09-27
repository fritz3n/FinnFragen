using FinnFragen.Web.Data;
using FinnFragen.Web.Services;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FinnFragen.Web.Hubs
{
    public class MessageHub : Hub<IMessageClient>
    {
        private readonly QuestionHandler questionHandler;

        public MessageHub(QuestionHandler questionHandler)
        {
            this.questionHandler = questionHandler;
        }

        public async Task<bool> SendMessage(string id, string message, string rcToken)
        {
            var question = await questionHandler.QuestionFromId(id);
            if (question is null)
                return false;

            await questionHandler.SendMessageMarkdown(question, message, Context.User.Identity.IsAuthenticated ? Message.Author.Answerer : Message.Author.Asker);

            return true;
        }
    }
}
