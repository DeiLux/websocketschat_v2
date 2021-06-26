using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using websocketschat.Core.Models;
using websocketschat.Core.Services.Interfaces;
using websocketschat.Web.Helpers;

namespace websocketschat.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IUserService _userService;
        private readonly UsersContext _usersContext;
        public ChatHub(ILogger<ChatHub> logger, IUserService userService, UsersContext usersContext)
        {
            _logger = logger;
            _userService = userService;
            _usersContext = usersContext;
        }
        public async Task Send(string username, string text)
        {
            Guid userId = Guid.Parse(Context.User.FindFirstValue("Guid"));
            User connectedUser = await _userService.GetUserByIdAsync(userId);

            if(connectedUser.IsDeleted == true)
            {
                await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                {
                    message = "--> You're banned in this chat.",
                    sender_username = "Server",
                    getter_username = connectedUser.Username,
                    roleid = 3
                });
                return;
            }

            string responseMessage = text;

            if (responseMessage == string.Empty || responseMessage == null)
            {
                await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                {
                    message = "--> Message can't be null or empty.",
                    sender_username = "Server",
                    getter_username = connectedUser.Username,
                    roleid = 3
                });
                return;
            }

            // starts with '/'  -  if text is command.
            // else -  if text is message.
            if (text.StartsWith("/"))
            {
                text = text.Remove(0, 1);

                // Chat commands.
                // /commands
                if (text.ToLower().Contains("commands"))
                {
                    responseMessage = "/send_to=username&message=text - sends private message (text param) to user (username param) if exists.\n" +
                                      "/change_name=newName - change nickname if new nickname is free to pick.\n" +
                                      "/make_admin=username - Give admin rights to user.\n" +
                                      "/remove_admin=username - Give admin rights back.\n" +
                                      "/ban_user=username - Ban user by username.\n" +
                                      "/unban_user=username - Unban user by username.\n" +
                                      "/commands - shows stored commands.";

                    await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                    {
                        message = "-->" + responseMessage,
                        sender_username = "Server",
                        getter_username = connectedUser.Username,
                        roleid = 3
                    });
                    return;
                }

                // Change current user nickname.
                // /change_name=nickname
                else if (text.ToLower().Contains("change_name="))
                {
                    Regex regexUsername = new Regex(@"^[a-zA-Z][a-zA-Z0-9-_\.]{1,255}$");
                    string newNickname = text.Substring(12);
                    if (regexUsername.IsMatch(newNickname))
                    {
                        if (newNickname == string.Empty || newNickname == null)
                        {
                            responseMessage = $"You tried to change your username to \'{newNickname}\' with failure.";

                            await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                            {
                                message = "-->" + responseMessage,
                                sender_username = "Server",
                                getter_username = connectedUser.Username,
                                roleid = 3
                            });
                            return;
                        }
                        else
                        {
                            User userWithSameUsername = await _userService.GetUserByUsernameAsync(newNickname);

                            if (userWithSameUsername == null)
                            {
                                User user = await _userService.GetUserByUsernameAsync(username);

                                user.Username = newNickname;

                                User updatedUser = await _userService.UpdateUserAsync(user);

                                responseMessage = $"User \'{username}\' changed nickname to \'{updatedUser.Username}\'.";

                                await Clients.All.SendAsync("Receive", new
                                {
                                    message = responseMessage,
                                    sender_username = updatedUser.Username,
                                    getter_username = "",
                                    roleid = updatedUser.RoleId
                                });
                                return;
                            }
                            else
                            {
                                responseMessage = $"You tried to change nickname to \'{newNickname}\' but user with this nickname is belong to other user.";
                                await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                                {
                                    message = "-->" + responseMessage,
                                    sender_username = "Server",
                                    getter_username = connectedUser.Username,
                                    roleid = 3
                                });
                                return;
                            }
                        }
                    }
                    else
                    {
                        responseMessage = $"You tried to change nickname to \'{newNickname}\' but you didn't pass validation.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });
                        return;
                    }
                }

                // Private message.
                // /send_to=username&message=text
                else if (text.ToLower().StartsWith("send_to="))
                {
                    string to = text.Split(new char[] { '&' })[0].Substring(8);
                    string message = text.Split(new char[] { '&' })[1].Substring(8);

                    User userMessageGetter = await _userService.GetUserByUsernameAsync(to);

                    if (userMessageGetter == null)
                    {
                        responseMessage = $"User with username \'{to}\' does not exist.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });
                        return;
                    }

                    // if sender is a getter too.
                    if (userMessageGetter != connectedUser)
                    {

                        if (message == string.Empty || message == null)
                        {
                            await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                            {
                                message = "--> Message can't be null or empty.",
                                sender_username = "Server",
                                getter_username = connectedUser.Username,
                                roleid = 3
                            });
                            return;
                        }

                        // write to sender what his message was sent to recevier.
                        responseMessage = $"Message \'{message}\' was sent to {userMessageGetter.Username} successfully.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });

                        // print message on receiver screen.
                        await Clients.User(userMessageGetter.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + message,
                            sender_username = connectedUser.Username,
                            getter_username = userMessageGetter.Username,
                            roleid = connectedUser.RoleId
                        });
                        return;
                    }
                    else
                    {
                        responseMessage = $"You tried to send message to yourself.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });
                        return;
                    }
                }

                // Give admin rights to user.
                // /make_admin=username
                else if (text.ToLower().StartsWith("make_admin="))
                {
                    if (connectedUser.RoleId != 1)
                    {
                        responseMessage = $"You role is \'User\', this command is not allowed to you.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });
                        return;
                    }

                    // 11 symbols
                    string newAdminNickname = text.Substring(11);
                    
                    if(newAdminNickname != string.Empty && newAdminNickname != null)
                    {
                        User newAdmin = await _userService.GetUserByUsernameAsync(newAdminNickname);

                        if(newAdmin != null)
                        {
                            newAdmin.RoleId = 1;
                            User updatedUser = await _userService.UpdateUserAsync(newAdmin);

                            responseMessage = $"User \'{updatedUser.Username}\' is granted admin rights.";

                            // write to sender what his message was sent to recevier.
                            await Clients.All.SendAsync("Notify", responseMessage);
                            return;
                        }
                        else
                        {
                            responseMessage = $"User with username \'{newAdminNickname}\' does not exist.";
                            await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                            {
                                message = "-->" + responseMessage,
                                sender_username = "Server",
                                getter_username = connectedUser.Username,
                                roleid = 3
                            });
                            return;
                        }
                    }
                    else
                    {
                        responseMessage = "Username can't be null or empty.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });
                        return;
                    }
                }

                // Give admin rights back.
                // /remove_admin=username
                else if (text.ToLower().StartsWith("remove_admin="))
                {
                    // 13 symbols
                    if (connectedUser.RoleId != 1)
                    {
                        responseMessage = $"You role is \'User\', this command is not allowed to you.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });
                        return;
                    }

                    string candidateToLoseAdminRoleNickname = text.Substring(13);

                    if (candidateToLoseAdminRoleNickname != string.Empty && candidateToLoseAdminRoleNickname != null)
                    {
                        User newAdmin = await _userService.GetUserByUsernameAsync(candidateToLoseAdminRoleNickname);

                        if(newAdmin.Username == "root")
                        {
                            responseMessage = $"Nobody can't remove admin rights from \'{newAdmin.Username}\'.";
                            await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                            {
                                message = "-->" + responseMessage,
                                sender_username = "Server",
                                getter_username = connectedUser.Username,
                                roleid = 3
                            });

                            return;
                        }

                        if (newAdmin != null)
                        {
                            newAdmin.RoleId = 2;
                            User updatedUser = await _userService.UpdateUserAsync(newAdmin);

                            responseMessage = $"User \'{updatedUser.Username}\' lost admin rights.";

                            // write to sender what his message was sent to recevier.
                            await Clients.All.SendAsync("Notify", responseMessage);
                            return;
                        }
                        else
                        {
                            responseMessage = $"User with username \'{candidateToLoseAdminRoleNickname}\' does not exist.";
                            await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                            {
                                message = "-->" + responseMessage,
                                sender_username = "Server",
                                getter_username = connectedUser.Username,
                                roleid = 3
                            });
                            return;
                        }
                    }
                    else
                    {
                        responseMessage = "Username can't be null or empty.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });
                        return;
                    }
                }

                // Ban user by username.
                // /ban_user=username
                else if (text.ToLower().StartsWith("ban_user="))
                {
                    //9
                    if (connectedUser.RoleId != 1)
                    {
                        responseMessage = $"You role is \'User\', this command is not allowed to you.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });
                        return;
                    }

                    string nicknameBan = text.Substring(9);

                    if (nicknameBan != string.Empty && nicknameBan != null)
                    {
                        User BannedUser = await _userService.GetUserByUsernameAsync(nicknameBan);

                        if (BannedUser.Username == "root")
                        {
                            responseMessage = $"Nobody can't ban \'{BannedUser.Username}\'.";
                            await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                            {
                                message = "-->" + responseMessage,
                                sender_username = "Server",
                                getter_username = connectedUser.Username,
                                roleid = 3
                            });

                            return;
                        }

                        if (BannedUser != null)
                        {
                            BannedUser.IsDeleted = true;
                            User updatedUser = await _userService.UpdateUserAsync(BannedUser);

                            responseMessage = $"User \'{updatedUser.Username}\' banned.";

                            // write to sender what his message was sent to recevier.
                            await Clients.All.SendAsync("Notify", responseMessage);
                            return;
                        }
                        else
                        {
                            responseMessage = $"User with username \'{nicknameBan}\' does not exist.";
                            await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                            {
                                message = "-->" + responseMessage,
                                sender_username = "Server",
                                getter_username = connectedUser.Username,
                                roleid = 3
                            });
                            return;
                        }
                    }
                    else
                    {
                        responseMessage = "Username can't be null or empty.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });
                        return;
                    }
                }

                // Unban user by username.
                // /unban_user=username
                else if (text.ToLower().StartsWith("unban_user="))
                {
                    //11
                    if (connectedUser.RoleId != 1)
                    {
                        responseMessage = $"You role is \'User\', this command is not allowed to you.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });
                        return;
                    }

                    string nicknameUnban = text.Substring(11);

                    if (nicknameUnban != string.Empty && nicknameUnban != null)
                    {
                        User UnBannedUser = await _userService.GetUserByUsernameAsync(nicknameUnban);

                        if (UnBannedUser.Username == "root")
                        {
                            responseMessage = $"\'root\' can't be banned.";
                            await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                            {
                                message = "-->" + responseMessage,
                                sender_username = "Server",
                                getter_username = connectedUser.Username,
                                roleid = 3
                            });

                            return;
                        }

                        if (UnBannedUser != null)
                        {
                            UnBannedUser.IsDeleted = false;
                            User updatedUser = await _userService.UpdateUserAsync(UnBannedUser);

                            responseMessage = $"User \'{updatedUser.Username}\' unbanned.";

                            // write to sender what his message was sent to recevier.
                            await Clients.All.SendAsync("Notify", responseMessage);
                            return;
                        }
                        else
                        {
                            responseMessage = $"User with username \'{nicknameUnban}\' does not exist.";
                            await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                            {
                                message = "-->" + responseMessage,
                                sender_username = "Server",
                                getter_username = connectedUser.Username,
                                roleid = 3
                            });
                            return;
                        }
                    }
                    else
                    {
                        responseMessage = "Username can't be null or empty.";
                        await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                        {
                            message = "-->" + responseMessage,
                            sender_username = "Server",
                            getter_username = connectedUser.Username,
                            roleid = 3
                        });
                        return;
                    }
                }

                // Command was not found.
                // /dsfdsf
                else
                {
                    responseMessage = "Command " + "\'/" + text + "\' is unsupported.";
                    await Clients.User(connectedUser.Id.ToString()).SendAsync("Receive", new
                    {
                        message = "-->" + responseMessage,
                        sender_username = "Server",
                        getter_username = connectedUser.Username,
                        roleid = 3
                    });
                    return;
                }
            }
            // Simple message.
            else
            {
                await Clients.All.SendAsync("Receive", new
                {
                    message = text,
                    sender_username = connectedUser.Username,
                    getter_username = "",
                    roleid = connectedUser.RoleId
                });
            }
        }

        #region Connect/Disconnect stuff
        public override async Task OnConnectedAsync()
        {
            Guid userId = Guid.Parse(Context.User.FindFirstValue("Guid"));
            User connectedUser = await _userService.GetUserByIdAsync(userId);

            _usersContext.UsersOnline++;

            await Clients.All.SendAsync("Notify", $"{connectedUser.Username} joined the chat. Users connected: {_usersContext.UsersOnline}");
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Guid userId = Guid.Parse(Context.User.FindFirstValue("Guid"));
            User connectedUser = await _userService.GetUserByIdAsync(userId);

            _usersContext.UsersOnline--;

            await Clients.All.SendAsync("Notify", $"{connectedUser.Username} left the chat. Users connected: {_usersContext.UsersOnline}");
            await base.OnDisconnectedAsync(exception);
        }
        #endregion
    }
}
