// Assets/Scripts/Core/NetworkManager.cs

using UnityEngine;
using UnityEngine.Networking; // Для UnityWebRequest
using System.Collections;    // Для Coroutines
using System;                // Для Action
using Newtonsoft.Json;       // Для обработки JSON

public class NetworkManager : MonoBehaviour
{
    // !!! ВАЖНО: Замените на адрес вашего бэкенда !!!
    // Если запускаете локально, это может быть http://localhost:5000 или https://localhost:7000
    // Убедитесь, что ваш ASP.NET Core бэкенд запущен.
    private const string BASE_URL = "http://localhost:5000/api";

    public static NetworkManager Instance { get; private set; }
    private string jwtToken = ""; // JWT-токен для авторизации


    // --- Добавьте это публичное свойство ---
    public string AuthToken
    {
        get { return jwtToken; }
    }
    // ------
    void Awake()
    {
        // Реализация Singleton: гарантирует, что есть только один NetworkManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Сохраняет объект при переключении сцен
        }
        else
        {
            Destroy(gameObject); // Уничтожает дубликат
        }
        LoadAuthToken(); // Пытаемся загрузить сохраненный токен
    }

    private void LoadAuthToken()
    {
        jwtToken = PlayerPrefs.GetString("AuthToken", "");
        if (!string.IsNullOrEmpty(jwtToken))
        {
            Debug.Log("Загружен сохраненный JWT токен.");
        }
    }

    public void SetAuthToken(string token)
    {
        jwtToken = token;
        PlayerPrefs.SetString("AuthToken", token); // Сохраняем токен локально
        PlayerPrefs.Save(); // Сохраняем PlayerPrefs на диск
        Debug.Log("JWT токен успешно сохранен.");
    }

    public void ClearAuthToken()
    {
        jwtToken = "";
        PlayerPrefs.DeleteKey("AuthToken");
        PlayerPrefs.Save();
        Debug.Log("JWT токен удален.");
    }

    // --- Методы для отправки HTTP-запросов ---
    public void PostRequest(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onFailure)
    {
        StartCoroutine(SendRequest(endpoint, UnityWebRequest.kHttpVerbPOST, jsonBody, onSuccess, onFailure));
    }

    // Метод для GET запросов (понадобится для получения данных, например, профиля)
    public void GetRequest(string endpoint, Action<string> onSuccess, Action<string> onFailure)
    {
        StartCoroutine(SendRequest(endpoint, UnityWebRequest.kHttpVerbGET, null, onSuccess, onFailure));
    }


    private IEnumerator SendRequest(string endpoint, string method, string jsonBody, Action<string> onSuccess, Action<string> onFailure)
    {
        // Создаем полный URL
        using var webRequest = new UnityWebRequest(BASE_URL + endpoint, method);

        // Если это POST-запрос с JSON телом
        if (jsonBody != null && (method == UnityWebRequest.kHttpVerbPOST || method == UnityWebRequest.kHttpVerbPUT))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.SetRequestHeader("Content-Type", "application/json");
        }

        // Добавляем JWT токен в заголовок Authorization для всех запросов, кроме регистрации/входа
        if (!string.IsNullOrEmpty(jwtToken) && !endpoint.Contains("auth/register") && !endpoint.Contains("auth/login"))
        {
            webRequest.SetRequestHeader("Authorization", "Bearer " + jwtToken);
        }

        // Для получения ответа от сервера
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        // Отправляем запрос и ждем ответа
        yield return webRequest.SendWebRequest();

        // Обработка результатов запроса
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(webRequest.downloadHandler.text);
        }
        else
        {
            // Логирование и вызов колбэка для ошибки
            string errorResponse = webRequest.downloadHandler.text;
            onFailure?.Invoke(errorResponse);
            Debug.LogError($"Network Error [{webRequest.method} {webRequest.url}]: Code {webRequest.responseCode} - {errorResponse}");
        }
    }
}