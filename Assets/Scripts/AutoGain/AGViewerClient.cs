using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AGViewerClient : MonoBehaviour
{
    public enum ConnectionState
    {
        Disconnected, // No active WebSocket connection to the viewer.
        Connecting, // A WebSocket connection attempt is currently in progress.
        Connected, // The WebSocket connection is open and ready to send data.
        Disconnecting // The client is closing an existing WebSocket connection.
    }

    [Header("Viewer Server")]
    [SerializeField] string host = "127.0.0.1";
    [SerializeField] int portNumber = 8765;
    [SerializeField] string endpointPath = "/unity";
    [SerializeField] int connectTimeoutSeconds = 5;

    [Header("Scene Integration")]
    [Tooltip("3D Auto Gain 씬 이름을 넣습니다. 해당 씬에서 벗어나면 자동으로 인스턴스가 종료됩니다.")]
    [SerializeField] string autoGainSceneName = "3D Auto Gain";
    [SerializeField] Button connectButton;
    [SerializeField] TMP_Text connectionStateText;

    [Header("Status")]
    [SerializeField] ConnectionState connectionState = ConnectionState.Disconnected;
    [SerializeField] string lastError = "";

    private static AGViewerClient instance;
    private ClientWebSocket webSocket;
    private CancellationTokenSource lifetimeCts;
    private bool connectionAttemptFailed;
    private bool isQuitting;

    public ConnectionState State => connectionState;
    public bool IsConnected => webSocket != null && webSocket.State == WebSocketState.Open;
    public string LastError => lastError;

    private Uri ViewerUri
    {
        get
        {
            string path = string.IsNullOrWhiteSpace(endpointPath) ? "" : endpointPath;
            if (!string.IsNullOrEmpty(path) && !path.StartsWith("/"))
                path = "/" + path;

            return new Uri($"ws://{host}:{portNumber}{path}");
        }
    }

    // ================================================================
    // Unity Events
    // ================================================================

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        ValidateUiReferences();
        connectButton?.onClick.AddListener(ConnectToViewer);
        SetConnectButtonVisible(true);
        RefreshStatusText();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void Update()
    {
        RefreshStatusText();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != autoGainSceneName)
            return;

        AGManager manager = FindObjectOfType<AGManager>();
        if (manager != null)
            manager.viewerClient = this;
        else
            Debug.LogWarning("[AGViewerClient] AGManager was not found in the AutoGain scene.");

        SetConnectButtonVisible(false);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (!isQuitting && scene.name == autoGainSceneName)
            Destroy(gameObject);
    }

    // ================================================================
    // Public Methods
    // ================================================================

    public async void ConnectToViewer()
    {
        if (connectionState == ConnectionState.Connected || connectionState == ConnectionState.Connecting)
            return;

        await ConnectAsync();
    }

    public async void DisconnectFromViewer()
    {
        await DisconnectAsync();
    }

    public async Task<bool> ConnectAsync()
    {
        CleanupSocket();

        connectionState = ConnectionState.Connecting;
        connectionAttemptFailed = false;
        lastError = "";
        lifetimeCts = new CancellationTokenSource();
        RefreshStatusText();

        using (CancellationTokenSource timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(connectTimeoutSeconds)))
        using (CancellationTokenSource connectCts = CancellationTokenSource.CreateLinkedTokenSource(
            lifetimeCts.Token,
            timeoutCts.Token
        ))
        {
            webSocket = new ClientWebSocket();

            try
            {
                await webSocket.ConnectAsync(ViewerUri, connectCts.Token);
                connectionState = ConnectionState.Connected;
                connectionAttemptFailed = false;
                _ = ReceiveLoopAsync(lifetimeCts.Token);
                RefreshStatusText();
                Debug.Log($"[AGViewerClient] Connected to {ViewerUri}");
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                connectionState = ConnectionState.Disconnected;
                connectionAttemptFailed = true;
                RefreshStatusText();
                Debug.LogWarning($"[AGViewerClient] Connection failed: {lastError}");
                CleanupSocket();
                return false;
            }
        }
    }

    public async Task DisconnectAsync()
    {
        if (connectionState == ConnectionState.Disconnected)
            return;

        connectionState = ConnectionState.Disconnecting;
        lifetimeCts?.Cancel();

        try
        {
            if (webSocket != null &&
                (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived))
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Unity client disconnecting", CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            lastError = ex.Message;
        }
        finally
        {
            CleanupSocket();
            connectionState = ConnectionState.Disconnected;
            connectionAttemptFailed = false;
            RefreshStatusText();
            Debug.Log("[AGViewerClient] Disconnected");
        }
    }

    public async Task<bool> SendTextAsync(string message)
    {
        if (!IsConnected)
            return false;

        byte[] bytes = Encoding.UTF8.GetBytes(message);
        ArraySegment<byte> buffer = new ArraySegment<byte>(bytes);

        try
        {
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, lifetimeCts.Token);
            return true;
        }
        catch (Exception ex)
        {
            lastError = ex.Message;
            MarkDisconnected();
            return false;
        }
    }

    // ================================================================
    // Private Methods
    // ================================================================

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        byte[] buffer = new byte[1024];

        try
        {
            while (!token.IsCancellationRequested && webSocket != null && webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    MarkDisconnected();
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during local disconnect.
        }
        catch (Exception ex)
        {
            lastError = ex.Message;
            MarkDisconnected();
        }
    }

    private void MarkDisconnected()
    {
        if (connectionState == ConnectionState.Disconnecting)
            return;

        CleanupSocket();
        connectionState = ConnectionState.Disconnected;
        connectionAttemptFailed = false;
        Debug.LogWarning("[AGViewerClient] Viewer connection lost");
    }

    private void ValidateUiReferences()
    {
        if (connectButton == null)
            Debug.LogWarning("[AGViewerClient] Connect button is not assigned.");

        if (connectionStateText == null)
            Debug.LogWarning("[AGViewerClient] Connection state text is not assigned.");
    }

    private void RefreshStatusText()
    {
        if (connectionStateText == null)
            return;

        if (connectionState == ConnectionState.Connected)
        {
            connectionStateText.text = "Viewer Connected";
            connectionStateText.color = Color.green;
        }
        else if (connectionAttemptFailed)
        {
            connectionStateText.text = "Viewer Not Found";
            connectionStateText.color = new Color(1f, 0.5f, 0f);
        }
        else
        {
            connectionStateText.text = "Viewer Disconnected";
            connectionStateText.color = Color.red;
        }
    }

    private void SetConnectButtonVisible(bool visible)
    {
        if (connectButton != null)
            connectButton.gameObject.SetActive(visible);
    }

    private void CleanupSocket()
    {
        lifetimeCts?.Cancel();
        lifetimeCts?.Dispose();
        lifetimeCts = null;

        webSocket?.Dispose();
        webSocket = null;
    }

    private async void OnDestroy()
    {
        if (instance == this)
            instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        connectButton?.onClick.RemoveListener(ConnectToViewer);

        await DisconnectAsync();
    }

    private async void OnApplicationQuit()
    {
        isQuitting = true;
        await DisconnectAsync();
    }
}
