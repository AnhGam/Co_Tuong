using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chinese_Chess.Models
{
    namespace Chinese_Chess.Models
    {
        public enum MessageType
        {
            System, 
            Player, 
            Bot,    
            Info    // (Optional)
        }

        public class ChatMessage
        {
            public DateTime Time { get; set; }
            public string Sender { get; set; }
            public string Text { get; set; }
            public MessageType Type { get; set; }

            public string TimeDisplay => Time.ToString("HH:mm");
        }
    }
}
