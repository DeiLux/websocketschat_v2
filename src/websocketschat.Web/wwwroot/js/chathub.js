
        //Регулярные выражения для валидации
        const regexPassword = RegExp('^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\s).*$');
        const regexUsername = RegExp('^[a-zA-Z][a-zA-Z0-9-_\.]{1,255}$');
        const hubConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chat", { accessTokenFactory: () => token })
            .build();


        let countChatMessages = 0;
        let token;      // токен
        let username;   // имя пользователя

        function messageCountHandler() {
            if (countChatMessages >= 11) {
                let count = countChatMessages - 11;
                $(`#message-${count}`).hide();
            }
        }

        function continueToAuth() {
            $('#chat').hide();
            $('#registration').hide();
            $('#auth').show();
        }
        function continueToRegistration() {
            $('#chat').hide();
            $('#registration').show();
            $('#auth').hide();
        }


        hubConnection.on('Notify', function (message) {
            $('#chat').show();
            $('#registration').hide();
            $('#auth').hide();

            let notifyElem = document.createElement("b");
            notifyElem.appendChild(document.createTextNode(message));

            let elem = document.createElement("p");
            elem.setAttribute("id", "message-" + countChatMessages++);
            elem.appendChild(notifyElem);
            var firstElem = document.getElementById("chat-window").firstChild;
            document.getElementById("chat-window").insertBefore(elem, firstElem);

            // Считаем кол-во сообщений по глобальному счетчику countChatMessages
            // И выводим только те сообщения, которые проходят условие
            messageCountHandler();
        });

        hubConnection.on("Receive", function (data) {

            // обновляем, если сообщение было приватное. (Private)
            // оставляем прежним, если сообщение было всем. (Broadcast)
            if (data.getter_username || !(/^\s*$/.test(data.getter_username))) {
                username = data.getter_username;
            }

            // создаем элемент <b> для имени пользователя
            let userNameElem = document.createElement("b");

            if (data.roleid == 1) {
                userNameElem.style.color = "red";
            } else if (data.roleid == 2) {
                userNameElem.style.color = "blue";
            } else {
                userNameElem.style.color = "green";
            }

            userNameElem.appendChild(document.createTextNode(data.sender_username + ": "));

            // создает элемент <p> для сообщения пользователя
            let elem = document.createElement("p");

            elem.setAttribute("id", "message-" + countChatMessages++);

            elem.appendChild(userNameElem);
            elem.appendChild(document.createTextNode(data.message));

            var firstElem = document.getElementById("chat-window").firstChild;
            document.getElementById("chat-window").insertBefore(elem, firstElem);

            // Считаем кол-во сообщений по глобальному счетчику countChatMessages
            // И выводим только те сообщения, которые проходят условие
            messageCountHandler();
        });

        // аутентификация
        document.getElementById("loginBtn").addEventListener("click", function (e) {
            let username = document.getElementById("userName").value;
            let password = document.getElementById("userPassword").value;
           
            if (!regexUsername.test(username)){
                alert("Invalid username, try other one.");
            } else if (!regexUsername.test(password)) {
                alert("Invalid password, try other one.");
            } else {
                var request = new XMLHttpRequest();
                // посылаем запрос на адрес "/token", в ответ получим токен и имя пользователя
                request.open("POST", "api/Accounts/token", true);
                request.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                request.addEventListener("load", function () {
                    if (request.status < 400) { // если запрос успешный

                        let data = JSON.parse(request.response);   // парсим ответ
                        token = data.access_token;
                        username = data.username;

                        hubConnection.start()       // начинаем соединение с хабом
                            .catch(err => {
                                console.error(err.toString());
                                //  document.getElementById("loginBtn").disabled = true;
                            });
                    }
                    else {
                        alert("Invalid username or password.");
                    }
                });

                // отправляем запрос на аутентификацию
                request.send("username=" + username + "&password=" + password);
            } 
        });

        document.getElementById("sendBtn").addEventListener("click", function (e) {
            let elem = document.getElementById("message");
            let message = elem.value;

            elem.value = "";

            hubConnection.invoke("Send", username, message);
        });

        // регистрация
        document.getElementById("registerBtn").addEventListener("click", function (e) {
            let username = document.getElementById("userNameRegistration").value;
            let password = document.getElementById("userPasswordRegistration").value;

            if (!regexUsername.test(username)){
                alert("Invalid username, try other one.");
            } else if (!regexUsername.test(password)) {
                alert("Invalid password, try other one.");
            } else {
                var request = new XMLHttpRequest();
                  // посылаем запрос на адрес "/token", в ответ получим токен и имя пользователя
                request.open("POST", "api/Accounts/register", true);
                request.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                request.addEventListener("load", function () {

                    if (request.status == 201) {
                        alert("Registration was successful.");
                        continueToAuth();
                    } else {
                        alert("Username or password is invalid");
                    }
                });

                if (document.getElementById("userPasswordRegistration").value == document.getElementById("userPasswordCopyRegistration").value) {
                    // отправляем запрос на регистрацию
                    request.send("username=" + username + "&password=" + password);
                } else {
                    alert("Passwords don't match.");
                }
            }
        });
    