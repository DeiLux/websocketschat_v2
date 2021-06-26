using System;
using System.Collections.Generic;
using System.Text;

namespace websocketschat.Bot.Parsing.ResponseMessage
{
    public class Root
    {
        public string message { get; set; }
        public string sender_username { get; set; }
        public string getter_username { get; set; }
        public int roleid { get; set; }
    }
}
