// Assets/Scripts/Modules/Auth/WelcomeManager.cs

using UnityEngine;
using UnityEngine.UI; // Если используете стандартные кнопки Unity
using TMPro; // Если используете TextMeshPro Buttons

public class WelcomeManager : MonoBehaviour
{
    public Button loginButton;
    public Button registerButton;
    public TMP_Text appTitle; // Для отображения названия

    void Start()
    {
        // Проверка на наличие токена для автоматического перехода
        if (!string.IsNullOrEmpty(NetworkManager.Instance.AuthToken))
        {
            Debug.Log("Автоматический вход: токен найден.");
            // Здесь должна быть логика проверки токена на сервере, 
            // но пока просто перейдем на главный экран
            SceneLoader.Instance.LoadScene("MainAppScene"); // Предполагаем, что такая сцена будет
            return;
        }

        // Привязываем методы к кнопкам
        loginButton.onClick.AddListener(() => SceneLoader.Instance.LoadScene("AuthScene"));
        registerButton.onClick.AddListener(() => SceneLoader.Instance.LoadScene("AuthScene")); // Пока обе ведут на одну сцену

        appTitle.text = "Couple's Nexus"; // Установите заголовок
    }
}