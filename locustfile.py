from locust import HttpUser, TaskSet, task, between

- name: Sprawdzenie instalacji Locust
  run: |
    python3 --version
    pip show locust



class UserBehavior(TaskSet):

    @task(2)  # Rejestracja i quizy mają większą wagę
    def register_and_quiz(self):
        self.client.post("/api/register", json={"username": "testuser", "password": "123456"})
        self.client.post("/api/login", json={"username": "testuser", "password": "123456"})
        self.client.get("/api/quiz/start")
        self.client.post("/api/quiz/submit", json={"answers": [1, 2, 3, 4]})
        self.client.get("/api/certificate/download")

    @task(1)  # Komentarze i wiadomości mają mniejszą wagę
    def post_comments_and_messages(self):
        self.client.post("/api/comments", json={"post_id": 1, "content": "Świetny quiz!"})
        self.client.post("/api/messages", json={"receiver_id": 2, "message": "Cześć, jak ci poszło?"})

class WebsiteUser(HttpUser):
    host = "http://localhost:7137"  # Adres API do testowania
    tasks = [UserBehavior]
    wait_time = between(1, 5)
