using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    private const int MAX_CONNECTIONS = 100;
    private const string SERVER_IP = "3.129.92.115";
    private const int SERVER_PORT = 8999;
    private const int WEB_PORT = 8998;
    private const int BUFFER_SIZE = 1024;

    //channels
    private int reliableChannelID;
    private int unreliableChannelID;

    private int hostId;
    private int connectionId;

    private byte error;
    private bool isStarted;
    public bool isConnected { get; private set; }

    public delegate void Callback();
    public delegate void BoolCallback(bool success);
    public delegate void RoomStatCallback(NetworkPlayer player, bool entered);

    Callback connectedCallback;
    BoolCallback joinRoomCallback;
    public RoomStatCallback RoomStatusChangeCallback;

    public Room currentRoom { get; private set; }
    public bool isRoomHost { get; private set; }
    public NetworkPlayer localPlayer { get; private set; }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        readData();
    }

    public void Init(Callback cb)
    {
        GlobalConfig config = new GlobalConfig();
        NetworkTransport.Init(config);

        ConnectionConfig cc = new ConnectionConfig();
        reliableChannelID = cc.AddChannel(QosType.Reliable);
        unreliableChannelID = cc.AddChannel(QosType.Unreliable);
        HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);

        hostId = NetworkTransport.AddHost(topo, 0);

        Debug.Log(hostId);

        if (Application.platform == RuntimePlatform.WebGLPlayer && !Application.isEditor)
            connectionId = NetworkTransport.Connect(hostId, SERVER_IP, WEB_PORT, 0, out error);
        else
            connectionId = NetworkTransport.Connect(hostId, SERVER_IP, SERVER_PORT, 0, out error);

        connectedCallback = cb;
        isStarted = true;

        Debug.Log(string.Format("Attempting to connect on {0}...", SERVER_IP));
    }

    public void CreateRoom(string name, BoolCallback cb)
    {
        joinRoomCallback = cb;
        sendServer(new CreateRoomPacket(name));
    }

    public void JoinRoom(string roomName, string playerName, BoolCallback cb)
    {
        joinRoomCallback = cb;
        sendServer(new JoinRoomPacket(roomName, playerName));
    }

    public void LeaveRoom()
    {
        sendServer(new LeaveRoomPacket());
    }

    private void readData()
    {
        if (!isStarted)
            return;

        byte[] buffer = new byte[BUFFER_SIZE];
        int outHostId, outConnectionId, outChannelId;
        int recievedSize;

        NetworkEventType e = NetworkTransport.Receive(out outHostId, out outConnectionId, out outChannelId, buffer, BUFFER_SIZE, out recievedSize, out error);

        if (e == NetworkEventType.Nothing) return;

        switch (e)
        {
            case NetworkEventType.ConnectEvent:
                isConnected = true;
                connectedCallback();
                break;
            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(buffer);
                Packet msg = (Packet)formatter.Deserialize(ms);
                OnData(outConnectionId, outChannelId, outHostId, msg);
                break;
            case NetworkEventType.BroadcastEvent:
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnected");
                isConnected = false;
                break;
            default:
                Debug.Log("Network event type not recognized");
                break;
        }
    }

    private void OnData(int cnnId, int channelId, int recHostId, Packet packet)
    {
        switch (packet.OP)
        {
            case PacketOP.None:
                break;
            case PacketOP.JoinRoomConfPacket:
                JoinRoomConfPacket confPacket = (JoinRoomConfPacket)packet;
                if (confPacket.successful)
                {
                    currentRoom = new Room(confPacket.name, confPacket.host);
                    localPlayer = confPacket.clientPlayer;
                    isRoomHost = confPacket.host == localPlayer;
                    currentRoom.players = confPacket.playersInRoom;
                }
                joinRoomCallback(confPacket.successful);
                break;
            case PacketOP.RoomStatusChange:
                RoomStatusChangePacket statusPacket = (RoomStatusChangePacket)packet;
                NetworkPlayer p = statusPacket.player;
                if (statusPacket.entered)
                    currentRoom.players.Add(p);
                RoomStatusChangeCallback(p, statusPacket.entered);
                break;
        }
    }

    private void sendServer(Packet packet)
    {
        byte[] buffer = new byte[BUFFER_SIZE];

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, packet);

        NetworkTransport.Send(hostId, connectionId, reliableChannelID, buffer, BUFFER_SIZE, out error);
    }

    public void shutdown()
    {
        isConnected = false;
        NetworkTransport.Shutdown();
    }
}

