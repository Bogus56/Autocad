document.addEventListener("DOMContentLoaded", function () {
    let chatButton = document.getElementById("chatButton");
    if (chatButton) {
        chatButton.addEventListener("click", showChatPopup);
    }

    // Automatycznie pobierz użytkowników po otwarciu strony
    fetchUsers();
});

let currentReceiverId = null;

// 1️⃣ **Pokaż okienko czatu**
function showChatPopup() {
    let popup = document.getElementById("chatPopup");
    if (popup) {
        popup.style.display = "block";
    }
}

// 2️⃣ **Ukryj okienko czatu**
function closeChatPopup() {
    let popup = document.getElementById("chatPopup");
    if (popup) {
        popup.style.display = "none";
    }
}

// 3️⃣ **Pobierz użytkowników i dodaj do listy**
function fetchUsers() {
    fetch('/api/chat/GetUsers')
        .then(response => response.json())
        .then(data => {
            let userListContainer = document.getElementById('userList');
            if (!userListContainer) return;

            userListContainer.innerHTML = ''; // Wyczyść listę

            data.forEach(user => {
                let listItem = document.createElement('li');
                listItem.classList.add('list-group-item');
                listItem.innerHTML = `${user.imie} ${user.nazwisko}`;
                listItem.onclick = function () {
                    openChatPopup(user.id, `${user.imie} ${user.nazwisko}`);
                };
                userListContainer.appendChild(listItem);
            });
        })
        .catch(error => console.error('Błąd pobierania użytkowników:', error));
}

// 4️⃣ **Otwórz czat dla konkretnego użytkownika**
// Otwórz czat dla konkretnego użytkownika
function openChatPopup(userId, userName) {
    currentReceiverId = userId;
    document.getElementById("chatUserName").innerText = `Czat z ${userName}`;
    document.getElementById("chatPopup").style.display = "block";

    loadChatMessages(currentReceiverId); // ✅ PRZEKAZUJEMY receiverId!
}


// Pobierz wiadomości dla wybranego użytkownika
function loadChatMessages() {
    if (!currentReceiverId) return;

    fetch(`/api/chat/messages?receiverId=${currentReceiverId}`)
        .then(response => response.json())
        .then(messages => {
            console.log("Odebrane wiadomości:", messages);
            const chatBody = document.getElementById("chatMessages");
            chatBody.innerHTML = ""; // Wyczyść stare wiadomości

            messages.forEach(msg => {
                const msgElement = document.createElement("p");

                // ✅ Poprawne przypisanie "Ty" / "Odbiorca"
                msgElement.innerText = `${msg.isSender ? "Ty" : "Odbiorca"}: ${msg.message}`;
                msgElement.classList.add(msg.isSender ? "sent-message" : "received-message"); // Stylowanie

                chatBody.appendChild(msgElement);
            });
        })
        .catch(error => console.error("Błąd pobierania wiadomości:", error));
}



// Wysyłanie wiadomości
function sendChatMessage() {
    let messageInput = document.getElementById("chatInput");
    let message = messageInput.value.trim();

    if (!message || !currentReceiverId) return;

    fetch("/api/chat/send", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ receiverId: currentReceiverId, message: message })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                messageInput.value = "";
                loadChatMessages(); // ✅ Odśwież wiadomości natychmiast po wysłaniu
            }
        })
        .catch(error => console.error("Błąd wysyłania wiadomości:", error));
}


// 7️⃣ **Automatyczne odświeżanie wiadomości co 3 sekundy**
setInterval(() => {
    if (currentReceiverId) {
        loadChatMessages();
    }
}, 3000);


