using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChatController : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private SimpleTcpNetworkManager networkManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI chatLogText;
    [SerializeField] private TMP_InputField chatInputField;

    [Header("Settings")]
    [SerializeField] private int maxMessageCount = 30;

    private string chatLog = "";
    private int messageCount = 0;

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

    private void Update()
    {
        if (chatInputField == null)
            return;

        if (!chatInputField.isFocused)
            return;

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            SendChatMessage();
        }
    }

    private void SendChatMessage()
    {
        if (chatInputField == null)
            return;

        string message = chatInputField.text.Trim();

        if (string.IsNullOrEmpty(message))
            return;

        if (networkManager == null || !networkManager.IsConnected)
        {
            AddSystemMessage("연결된 상대가 없습니다.");
            ClearInput();
            return;
        }

        string senderName = networkManager.IsHost ? "Host" : "Client";

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

        chatLog += $"{sender}: {message}\n";

        TrimChatLogIfNeeded();

        if (chatLogText != null)
            chatLogText.text = chatLog;
    }

    private void AddSystemMessage(string message)
    {
        messageCount++;

        chatLog += $"[시스템] {message}\n";

        TrimChatLogIfNeeded();

        if (chatLogText != null)
            chatLogText.text = chatLog;
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
    }
}