using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class SimpleTcpNetworkManager : MonoBehaviour
{
    [Header("Connection Settings")]
    [SerializeField] private string ipAddress = "127.0.0.1";
    [SerializeField] private int port = 7777;

    private TcpListener server;
    private TcpClient client;

    private StreamReader reader;
    private StreamWriter writer;

    private Thread serverThread;
    private Thread receiveThread;

    private bool isRunning;
    private bool isConnected;

    public bool IsConnected => isConnected;

    private readonly object sendLock = new object();
    private readonly ConcurrentQueue<string> receivedMessages = new ConcurrentQueue<string>();

    public event Action<string> OnMessageReceived;
    public bool IsHost { get; private set; }

    private void Update()
    {
        while (receivedMessages.TryDequeue(out string message))
        {
            Debug.Log($"수신: {message}");
            OnMessageReceived?.Invoke(message);
        }
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void StartHost()
    {
        if (isRunning)
        {
            Debug.LogWarning("이미 네트워크가 실행 중입니다.");
            return;
        }

        IsHost = true;
        isRunning = true;

        serverThread = new Thread(ServerListenLoop);
        serverThread.IsBackground = true;
        serverThread.Start();

        Debug.Log($"Host 시작 / Port:{port}");
    }

    private void ServerListenLoop()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            receivedMessages.Enqueue("클라이언트 접속 대기 중...");

            client = server.AcceptTcpClient();

            receivedMessages.Enqueue("클라이언트 접속 완료");

            SetupStreams();
            StartReceiveThread();
        }
        catch (Exception e)
        {
            receivedMessages.Enqueue($"서버 오류: {e.Message}");
            Disconnect();
        }
    }

    public void StartClient()
    {
        if (isRunning)
        {
            Debug.LogWarning("이미 네트워크가 실행 중입니다.");
            return;
        }

        try
        {
            IsHost = false;
            isRunning = true;

            client = new TcpClient();
            client.Connect(ipAddress, port);

            Debug.Log($"서버 접속 성공 / IP:{ipAddress}, Port:{port}");

            SetupStreams();
            StartReceiveThread();
        }
        catch (Exception e)
        {
            Debug.LogError($"클라이언트 접속 실패: {e.Message}");
            isRunning = false;
        }
    }
    private void SetupStreams()
    {
        NetworkStream stream = client.GetStream();

        reader = new StreamReader(stream);
        writer = new StreamWriter(stream);
        writer.AutoFlush = true;

        isConnected = true;
    }

    private void StartReceiveThread()
    {
        receiveThread = new Thread(ReceiveLoop);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveLoop()
    {
        try
        {
            while (isRunning && isConnected)
            {
                string message = reader.ReadLine();

                if (message == null)
                {
                    receivedMessages.Enqueue("상대 연결이 끊겼습니다.");
                    Disconnect();
                    break;
                }

                receivedMessages.Enqueue(message);
            }
        }
        catch (Exception e)
        {
            receivedMessages.Enqueue($"수신 루프 종료: {e.Message}");
            Disconnect();
        }
    }

    public void SendMessageToPeer(string message)
    {
        if (!isConnected || writer == null)
        {
            Debug.LogWarning("연결되지 않아 메시지를 보낼 수 없습니다.");
            return;
        }

        try
        {
            lock (sendLock)
            {
                writer.WriteLine(message);
            }

            Debug.Log($"송신: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"송신 실패: {e.Message}");
            Disconnect();
        }
    }

    public void SendPacket(PacketType type, object data)
    {
        string jsonData = JsonUtility.ToJson(data);
        NetworkPacket packet = new NetworkPacket(type, jsonData);
        string packetJson = JsonUtility.ToJson(packet);

        SendMessageToPeer(packetJson);
    }

    public void Disconnect()
    {
        isRunning = false;
        isConnected = false;

        try
        {
            reader?.Close();
            writer?.Close();
            client?.Close();
            server?.Stop();
        }
        catch
        {
        }

        reader = null;
        writer = null;
        client = null;
        server = null;

        Debug.Log("네트워크 종료");
    }
}