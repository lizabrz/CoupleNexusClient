// Assets/Scripts/Modules/Auth/AuthManager.cs

using UnityEngine;
using UnityEngine.UI; // Для кнопок и Toggle
using TMPro; // Для TextMeshPro InputFields и Text
using System.Collections.Generic; // Для Dictionary при десериализации
using Newtonsoft.Json; // Для работы с JSON

public class AuthManager : MonoBehaviour
{
    // --- UI Элементы для Регистрации ---
    [Header("Registration UI")]
    public GameObject registrationPanel; // Панель, содержащая элементы регистрации
    public TMP_InputField regEmailInput;
    public TMP_InputField regPasswordInput;
    public TMP_InputField regConfirmPasswordInput;
    public Button registerButton;

    // --- UI Элементы для Входа ---
    [Header("Login UI")]
    public GameObject loginPanel; // Панель, содержащая элементы входа
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Button loginButton;

    // --- Общие UI Элементы ---
    [Header("Shared UI")]
    public TMP_Text statusMessageText; // Для вывода ошибок или статуса
    public Button toggleToLoginButton; // Кнопка "Уже есть аккаунт? Войти"
    public Button toggleToRegisterButton; // Кнопка "Еще нет аккаунта? Зарегистрироваться"

    void Start()
    {
        // Привязываем методы к кнопкам
        registerButton.onClick.AddListener(HandleRegistration);
        loginButton.onClick.AddListener(HandleLogin);
        toggleToLoginButton.onClick.AddListener(() => ToggleAuthMode(false));
        toggleToRegisterButton.onClick.AddListener(() => ToggleAuthMode(true));

        // Устанавливаем начальное состояние (например, показываем вход)
        ToggleAuthMode(false);
        statusMessageText.text = ""; // Очищаем статус при старте
    }

    public void ToggleAuthMode(bool isRegisterMode)
    {
        registrationPanel.SetActive(isRegisterMode);
        loginPanel.SetActive(!isRegisterMode);
        statusMessageText.text = ""; // Сбрасываем статус при переключении
    }

    public void HandleRegistration()
    {
        // --- Клиентская Валидация ---
        if (string.IsNullOrEmpty(regEmailInput.text) || string.IsNullOrEmpty(regPasswordInput.text) || string.IsNullOrEmpty(regConfirmPasswordInput.text))
        {
            statusMessageText.text = "Все поля обязательны для заполнения!";
            return;
        }
        if (regPasswordInput.text != regConfirmPasswordInput.text)
        {
            statusMessageText.text = "Пароли не совпадают!";
            return;
        }
        if (regPasswordInput.text.Length < 6) // Пример простой валидации
        {
            statusMessageText.text = "Пароль должен быть не менее 6 символов.";
            return;
        }
        // Добавить валидацию email (регулярное выражение)
        // ...

        statusMessageText.text = "Регистрация...";

        // Создаем объект для отправки на сервер
        var authData = new
        {
            Email = regEmailInput.text,
            Password = regPasswordInput.text
        };
        string jsonPayload = JsonConvert.SerializeObject(authData);

        // Отправляем запрос через NetworkManager
        NetworkManager.Instance.PostRequest(
            "/auth/register", // Эндпоинт на сервере
            jsonPayload,
            OnAuthSuccess,
            OnAuthFailure
        );
    }

    public void HandleLogin()
    {
        // --- Клиентская Валидация ---
        if (string.IsNullOrEmpty(loginEmailInput.text) || string.IsNullOrEmpty(loginPasswordInput.text))
        {
            statusMessageText.text = "Введите Email и пароль!";
            return;
        }

        statusMessageText.text = "Вход...";

        var authData = new
        {
            Email = loginEmailInput.text,
            Password = loginPasswordInput.text
        };
        string jsonPayload = JsonConvert.SerializeObject(authData);

        // Отправляем запрос через NetworkManager
        NetworkManager.Instance.PostRequest(
            "/auth/login",
            jsonPayload,
            OnAuthSuccess,
            OnAuthFailure
        );
    }

    private void OnAuthSuccess(string jsonResponse)
    {
        // Десериализуем ответ от сервера (например, {"token": "...", "nickname": "..."})
        var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
        string token = response["token"];
        string nickname = response.ContainsKey("nickname") ? response["nickname"] : "Пользователь"; // Никнейм может прийти позже

        NetworkManager.Instance.SetAuthToken(token); // Сохраняем токен

        statusMessageText.text = $"Успех! Добро пожаловать, {nickname}!";
        Debug.Log("Авторизация успешна. Токен получен.");

        // !!! Здесь логика перехода на следующий экран онбординга !!!
        // В зависимости от того, зарегистрирован ли пользователь и связана ли пара
        // Например:
        // SceneLoader.Instance.LoadScene("QuestionnaireScene"); // На Первичную Анкету
    }

    private void OnAuthFailure(string errorResponseJson)
    {
        // Десериализуем JSON-ответ с ошибкой от сервера, чтобы получить понятное сообщение
        // Пример: {"message": "Пользователь с таким Email уже существует."}
        string errorMessage = "Неизвестная ошибка авторизации.";
        try
        {
            var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorResponseJson);
            if (errorDict != null && errorDict.ContainsKey("message"))
            {
                errorMessage = errorDict["message"];
            }
        }
        catch (JsonException)
        {
            errorMessage = "Ошибка сервера: " + errorResponseJson;
        }

        statusMessageText.text = errorMessage;
        Debug.LogError($"Ошибка авторизации: {errorMessage}");
    }
}