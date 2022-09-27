using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinnFragen.Web.Hubs
{
    public interface IMessageClient
    {
        Task UpdateMessages(IEnumerable<Message> message);
        Task UpdateMessage(Message message);

        public enum MessageAuthor
        {
            Asker,
            Answerer
        }

        public class Message
        {
            public int Id { get; set; }
            public MessageAuthor Author { get; set; }
            public string Text { get; set; }
            public string Html { get; set; }
            public bool Seen { get; set; }
            public DateTime Sent { get; set; }

            public static Message
        }
    }
}
