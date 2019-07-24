using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using HoloToolkit.Unity.SpatialMapping;
using System.Collections.Generic;

//[NetworkSettings(channel = 6, sendInterval = (1.0f / 29.0f))]
public class CustomNetworkManager : MonoBehaviour
{
    public int port;
    public NetworkIdentity prefab;
    public GameObject clientPrefab;
    public GameObject SceneRoot;
    public DrawLine lineHandler;
    public GameObject SpatialMeshVR;
    public Material lineMaterial;
    public Material surfaceMaterial;

    public bool isAtStartup = true;
    public bool isTheClient = true;
    public string serverAddress;
    public static PhotonView photonView;

    NetworkClient myClient;

    public class CustomMessage
    {
        public static short lineMessage = MsgType.Highest + 1;
    }
    public class AssetMessage
    {
        public static short assetMessage = MsgType.Highest + 2;
    }
    public class SpatialMeshMsg
    {
        public static short meshMsg = MsgType.Highest + 3;
    }

    public class PointsMessage : MessageBase
    {
        public Vector3[] vertices;
        public Vector2[] uvs;
        public int[] triangles;
    }
    public class ARLineMessage : MessageBase
    {
        public Vector3[] pointPositions;
    }
    public class AssetsMessage : MessageBase
    {
        public NetworkHash128 assetId;
    }
    public void sendSpatialMesh(Mesh mesh)
    {
        //PointsMessage message = new PointsMessage();
        //message.vertices = vertices;
        //message.uvs = uvs;
        //message.triangles = triangles;
        //myClient.Send(SpatialMeshMsg.meshMsg, message);

        var data = MeshSerializer.WriteMesh(mesh, true);

        photonView.RPC("TransferMesh", PhotonTargets.All, SpatialMeshMsg.meshMsg, data);
    }
    public void onSpatialMeshMsg(NetworkMessage netMsg)
    {
        Debug.Log("Received spatial mesh message");
        PointsMessage message = netMsg.ReadMessage<PointsMessage>();

        // line object
        GameObject surfaceObject = new GameObject("Surface Object");
        surfaceObject.transform.SetParent(SpatialMeshVR.transform);

        Mesh mesh = new Mesh();
        surfaceObject.AddComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = message.vertices;
        mesh.uv = message.uvs;
        mesh.triangles = message.triangles;
        surfaceObject.AddComponent<MeshRenderer>().material = surfaceMaterial;

    }
    public void sendAssetId(NetworkHash128 assetId)
    {
        AssetsMessage msg = new AssetsMessage();
        msg.assetId = assetId;
        NetworkServer.SendToAll(AssetMessage.assetMessage, msg);
    }
    public void onAssetMsg(NetworkMessage netMsg)
    {
        Debug.Log("Received assetId");
        AssetsMessage msg = netMsg.ReadMessage<AssetsMessage>();
        foreach (NetworkHash128 hash in ClientScene.prefabs.Keys)
        {
            if (hash.Equals(prefab.assetId))
            {
                Debug.Log("Found match");
                return;
            }
        }
        Debug.Log("Registering spawn handler for prefab");
        ClientScene.RegisterSpawnHandler(msg.assetId, SpawnSphere, UnspawnSphere);
        Debug.Log("Readying client");
        ClientScene.Ready(netMsg.conn);

    }
    public void sendMessage(Vector3[] vertices, Vector2[] uvs, int[] triangles)
    {
        PointsMessage message = new PointsMessage();
        message.vertices = vertices;
        message.uvs = uvs;
        message.triangles = triangles;
        NetworkServer.SendToAll(CustomMessage.lineMessage, message);
    }
    public void sendARLine(Vector3[] pointPositions)
    {
        ARLineMessage message = new ARLineMessage();
        message.pointPositions = pointPositions;
        //myClient.Send(CustomMessage.lineMessage, message);

        photonView.RPC("TransferARLine", PhotonTargets.All, CustomMessage.lineMessage, pointPositions);
    }
    public void onServerReceiveMessage(NetworkMessage msg)
    {
        // TODO: Add linerenderer drawing on server (VR) side
        ARLineMessage message = msg.ReadMessage<ARLineMessage>();
        lineHandler.SpawnLineGenerator(message.pointPositions);
    }
    public void onClientReceiveMessage(NetworkMessage msg)
    {
        //Debug.Log("Received message");
        PointsMessage message = msg.ReadMessage<PointsMessage>();

        // line object
        GameObject line = new GameObject();
        Mesh mesh = new Mesh();
        line.AddComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = message.vertices;
        mesh.uv = message.uvs;
        mesh.triangles = message.triangles;
        line.AddComponent<MeshRenderer>().material = lineMaterial;

        //Vector3[] points = message.points;
        //Debug.Log(points.Length + " points received");
        //foreach (Vector3 point in points)
        //{
        //    Debug.Log(point);
        //}
    }
    private GameObject SpawnSphere(Vector3 position, NetworkHash128 assetId)
    {
        GameObject clientObject = Instantiate(clientPrefab, position, Quaternion.identity);
        //clientObject.transform.SetParent(SceneRoot.transform);
        clientObject.AddComponent<NetworkIdentity>();
        clientObject.AddComponent<NetworkTransform>();
        return clientObject;
    }
    private void UnspawnSphere(GameObject sphere)
    {
        Destroy(sphere);
    }
    void Start()
    {
        photonView = PhotonView.Get(this);

        ConnectionConfig myConfig = new ConnectionConfig();
        Debug.Log("Channels: " + myConfig.ChannelCount);
        foreach (ChannelQOS channel in myConfig.Channels)
        {
            Debug.Log(channel);
        }
        // setup code
        if (isTheClient)
        {
            Debug.Log("STARTING CLIENT CustomNetworkManager");
            SetupClient();
        }
        else
        {
            Debug.Log("STARTING SERVER");
            SetupServer();
        }
    }

    // Create a server and listen on a port
    public void SetupServer()
    {
        NetworkServer.Listen(port);
        isAtStartup = false;
        NetworkServer.RegisterHandler(MsgType.Connect, onServerReceiveConnect);
        NetworkServer.RegisterHandler(MsgType.Ready, OnClientReady);
        // Handle incoming line data
        NetworkServer.RegisterHandler(CustomMessage.lineMessage, onServerReceiveMessage);
        NetworkServer.RegisterHandler(SpatialMeshMsg.meshMsg, onSpatialMeshMsg);
        //NetworkServer.RegisterHandler(MsgType.AddPlayer, OnAddPlayerMessage);
    }
    //void OnAddPlayerMessage(NetworkMessage netMsg)
    //{
    //    GameObject thePlayer = (GameObject)Instantiate(prefab.gameObject, Vector3.zero, Quaternion.identity);

    //    // This spawns the new player on all clients
    //    NetworkServer.AddPlayerForConnection(netMsg.conn, thePlayer, 0);
    //}
    // Create a client and connect to the server port
    public void SetupClient()
    {
        // TODO: TEMP
        //ClientScene.RegisterPrefab(prefab.gameObject, SpawnSphere, UnspawnSphere);
        //myClient = new NetworkClient();
        //myClient.RegisterHandler(MsgType.Connect, OnConnected);
        //myClient.RegisterHandler(AssetMessage.assetMessage, onAssetMsg);
        // Handle incoming line data
        //myClient.RegisterHandler(CustomMessage.lineMessage, onClientReceiveMessage);
        //Debug.Log("Attemping to connect");
        //myClient.Connect(serverAddress, port);
        //Debug.Log("Status: " + myClient.isConnected);
        isAtStartup = false;
    }

    [PunRPC]
    public void ClientConnected(NetworkMessage msg)
    {
        Debug.Log("Client connected: " + MsgType.Connect);
        OnConnected(msg);
    }

    [PunRPC]
    public void TransferAssetMessage(NetworkMessage msg)
    {
        Debug.Log("Asset Message: " + AssetMessage.assetMessage);
        onAssetMsg(msg);
    }

    [PunRPC]
    public void TransferLineMessage(NetworkMessage msg)
    {
        Debug.Log("Line Message: " + CustomMessage.lineMessage);
        onClientReceiveMessage(msg);
    }

    public void OnClientReady(NetworkMessage netMsg)
    {
        Debug.Log("Client ready");
        NetworkServer.SetClientReady(netMsg.conn);
        NetworkServer.Spawn(Instantiate(prefab.gameObject, new Vector3(0, 0, 0), Quaternion.identity));
    }
    public void onServerReceiveConnect(NetworkMessage netMsg)
    {
        Debug.Log("Received a client connection");
        sendAssetId(prefab.assetId);
    }
    // client function
    public void OnConnected(NetworkMessage netMsg)
    {
        // TODO: TEMP
        //ClientScene.AddPlayer(myClient.connection, 0);
        Debug.Log("Connected to server");
        //ClientScene.Ready(netMsg.conn);

    }
}
