using System;
using System.Collections.Generic;
using System.Text;

namespace websocketschat.Bot.Parsing.ResponseOtDeda
{
    public class AvailableTransport
    {
        public string transport { get; set; }
        public List<string> transferFormats { get; set; }
    }

    public class Root
    {
        public int negotiateVersion { get; set; }
        public string connectionId { get; set; }
        public string connectionToken { get; set; }
        public List<AvailableTransport> availableTransports { get; set; }
    }
}
