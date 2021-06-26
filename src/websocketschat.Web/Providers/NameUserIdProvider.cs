using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace websocketschat.Web.Providers
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Поменять на Guid чтобы починить приватные сообщения.
            // Поскольку IdentifierId сменное поле, после изменения никнейма мы не можем отправим ему сообщение!
            return connection.User?.FindFirstValue("Guid");
        }
    }
}
