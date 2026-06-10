using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private SimpleTcpNetworkManager networkManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI chatLogText;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private ScrollRect chatScrollRect;

    [Header("Settings")]
    [SerializeField] private int maxMessageCount = 30;

    private string chatLog = "";
    private int messageCount = 0;

    private void Awake()
    {
        if (chatInputField != null)
            chatInputField.onSubmit.AddListener(OnSubmitChat);
    }

    private void OnEnable()
    {
        if (networkManager != null)
            networkManager.OnMessageReceived += HandleNetworkMessage;
    }

    private void OnDisable()
    {
        if (networkManager != null)
            networkManager.OnMessageReceived -= HandleNetworkMessage;
    }

    private void OnDestroy()
    {
        if (chatInputField != null)
            chatInputField.onSubmit.RemoveListener(OnSubmitChat);
    }

    private void OnSubmitChat(string inputText)
    {
        SendChatMessage(inputText);
    }

    private void SendChatMessage(string inputText)
    {
        Debug.Log("채팅 전송 시도");

        if (chatInputField == null)
        {
            Debug.LogWarning("ChatInputField가 연결되지 않았습니다.");
            return;
        }

        string message = inputText.Trim();

        Debug.Log($"입력 메시지: {message}");

        if (string.IsNullOrEmpty(message))
        {
            ClearInput();
            return;
        }

        if (networkManager == null)
        {
            AddSystemMessage("NetworkManager가 연결되지 않았습니다.");
            ClearInput();
            return;
        }

        if (!networkManager.IsConnected)
        {
            AddSystemMessage("연결된 상대가 없습니다.");
            ClearInput();
            return;
        }

        string senderName = networkManager.IsHost ? "상대" : "상대";

        ChatPacketData chatData = new ChatPacketData(senderName, message);
        networkManager.SendPacket(PacketType.Chat, chatData);

        AddChatMessage("나", message);

        ClearInput();
    }

    private void HandleNetworkMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (!message.StartsWith("{"))
            return;

        NetworkPacket packet;

        try
        {
            packet = JsonUtility.FromJson<NetworkPacket>(message);
        }
        catch
        {
            return;
        }

        if (packet == null)
            return;

        if (packet.type != PacketType.Chat)
            return;

        ChatPacketData chatData =
            JsonUtility.FromJson<ChatPacketData>(packet.jsonData);

        AddChatMessage(chatData.senderName, chatData.message);
    }

    private void AddChatMessage(string sender, string message)
    {
        messageCount++;

        chatLog += $"{sender} : {message}\n";

        TrimChatLogIfNeeded();

        if (chatLogText != null)
        {
            chatLogText.text = chatLog;
            Debug.Log($"채팅 로그 갱신: {chatLog}");
        }
        else
        {
            Debug.LogWarning("ChatLogText가 연결되지 않았습니다.");
        }

        ScrollToBottom();
    }

    private void AddSystemMessage(string message)
    {
        messageCount++;

        chatLog += $"[시스템] {message}\n";

        TrimChatLogIfNeeded();

        if (chatLogText != null)
            chatLogText.text = chatLog;

        ScrollToBottom();
    }

    private void TrimChatLogIfNeeded()
    {
        if (messageCount <= maxMessageCount)
            return;

        string[] lines = chatLog.Split('\n');

        if (lines.Length <= maxMessageCount)
            return;

        int startIndex = Mathf.Max(0, lines.Length - maxMessageCount - 1);

        chatLog = "";

        for (int i = startIndex; i < lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(lines[i]))
                chatLog += lines[i] + "\n";
        }

        messageCount = Mathf.Min(messageCount, maxMessageCount);
    }

    private void ClearInput()
    {
        if (chatInputField == null)
            return;

        chatInputField.text = "";
        chatInputField.ActivateInputField();
        chatInputField.Select();
    }

    private void ScrollToBottom()
    {
        if (chatScrollRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }
}