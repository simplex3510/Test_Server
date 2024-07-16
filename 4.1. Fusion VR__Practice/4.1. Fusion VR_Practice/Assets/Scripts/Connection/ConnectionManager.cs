using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [System.Flags]
    public enum ConnectionCriterias
    {
        RoomName = 1,
        SessionProperties = 2,
    }

    public struct StringSessionProperty
    {
        public string propertyName;
        public string value;
    }

    [Header("Room Configuration")]
    public GameMode gameMode = GameMode.Shared;
    public string roomName = "SampleFusion";
    public bool connectOnStart = true;
    [Tooltip("Set it to 0 to use the DefaultPlayers value, from the Global NetworkProjectConfig (simulation section)")]
    public int playerCount = 0;

    [Header("Room Selection Criteria")]
    public ConnectionCriterias connectionCriterias = ConnectionCriterias.RoomName;
    [Tooltip("If connectionCriterias include SessionProperties, additionalSessionProperties (editable in the inspector) will be added to sessionProperties")]
    public List<StringSessionProperty> additionalSessionProperties = new();
    public Dictionary<string, SessionProperty> sessionProperties;

    [Header("Fusion Settings")]
    [Tooltip("Fusion Runner/ Automatically created if not set")]
    public NetworkRunner runner;
    public INetworkSceneManager sceneManager;

    [Header("Local user Spawner")]
    public NetworkObject userPrefab;

    [Header("Event")]
    public UnityEvent onWillConnect = new();

    [Header("Info")]
    public List<StringSessionProperty> actualSessionProperties = new();

    // Dictionary of spawned user prefabs, to store them on the server for host topology, and destroy them on disconnection
    // (for shared topology, use Network Objects's "Destroy When State Authority Leaves" option)
    private Dictionary<PlayerRef, NetworkObject> _spawnedUsers = new();

    bool ShouldConnectWithRoomName => (connectionCriterias & ConnectionCriterias.RoomName) != 0;
    bool ShouldConnectWithSessionProperties => (connectionCriterias & ConnectionCriterias.SessionProperties) != 0;

    private Dictionary<string, SessionProperty> AllConnectionSessionProperties
    {
        get
        {
            var propDict = new Dictionary<string, SessionProperty>();
            actualSessionProperties = new List<StringSessionProperty>();
            if (sessionProperties != null)
            {
                foreach (var prop in sessionProperties)
                {
                    propDict.Add(prop.Key, prop.Value);
                    actualSessionProperties.Add(new StringSessionProperty { propertyName = prop.Key, value = prop.Value});
                }
            }

            if (additionalSessionProperties != null)
            {
                foreach (var additionalProperty in additionalSessionProperties)
                {
                    propDict[additionalProperty.propertyName] = additionalProperty.value;
                    actualSessionProperties.Add(additionalProperty);
                }
            }

            return propDict;
        }
    }

    private void Awake()
    {
        if (runner == null)
        {
            runner = GetComponent<NetworkRunner>();
        }

        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }
        runner.ProvideInput = true;
    }

    private async void Start()
    {
        if (connectOnStart)
        {
            await Connect();
        }    
    }

    public virtual NetworkSceneInfo CurrentSceneInfo()
    {
        var activeScene = SceneManager.GetActiveScene();
        SceneRef sceneRef = default;

        if (activeScene.buildIndex < 0 || SceneManager.sceneCountInBuildSettings <= activeScene.buildIndex)
        {
            Debug.LogError("Current scene is not part of the build settings");
        }
        else
        {
            sceneRef = SceneRef.FromIndex(activeScene.buildIndex);
        }

        var sceneInfo = new NetworkSceneInfo();
        if (sceneRef.IsValid)
        {
            sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Single);
        }
        return sceneInfo;
    }


    public async Task Connect()
    {
        if (sceneManager == null)
        {
            gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        if (onWillConnect != null)
        {
            onWillConnect.Invoke();
        }

        var args = new StartGameArgs()
        {
            GameMode = gameMode,
            Scene = CurrentSceneInfo(),
            SceneManager = sceneManager,
        };

        if (ShouldConnectWithRoomName)
        {
            args.SessionName = roomName;
        }

        if (ShouldConnectWithSessionProperties)
        {
            args.SessionProperties = AllConnectionSessionProperties;
        }

        if (0 < playerCount)
        {
            args.PlayerCount = playerCount;
        }

        await runner.StartGame(args);

        string prop = "";
        if (runner.SessionInfo.Properties != null && 0 < runner.SessionInfo.Properties.Count)
        {
            prop = "SessionProperties: ";
            foreach (var p in runner.SessionInfo.Properties)
            {
                prop += $"{p.Key} = {p.Value.PropertyType} ";
            }
        }

        Debug.Log($"Session info: Room name {runner.SessionInfo.Name}. Region: {runner.SessionInfo.Region}. {prop}");
        if ((connectionCriterias & ConnectionCriterias.RoomName) == 0)
        {
            roomName = runner.SessionInfo.Name;
        }
    }

    #region Player Spawn
    public void OnPlayerJoinedSharedMode(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer && userPrefab != null)
        {
            NetworkObject networkPlayerObject = runner.Spawn(userPrefab, position: transform.position, rotation: transform.rotation, player, (runner, obj) => { });
        }
    }

    public void OnPlayerJoinedHostMode(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && userPrefab != null)
        {
            Debug.Log($"OnPlayerJoined. PlayerId: {player.PlayerId}");
            NetworkObject networkPlayerObject = runner.Spawn(userPrefab, position: transform.position, rotation: transform.rotation, inputAuthority: player, (runner, obj) => { });
            _spawnedUsers.Add(player, networkPlayerObject);
        }
    }

    public void OnPlayerLeftHostMode(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedUsers.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedUsers.Remove(player);
        }
    }
    #endregion

    #region INetworkRunnerCallbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.Topology == Topologies.ClientServer)
        {
            OnPlayerJoinedHostMode(runner, player);
        }
        else
        {
            OnPlayerJoinedSharedMode(runner, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.Topology == Topologies.ClientServer)
        {
            OnPlayerLeftHostMode(runner, player);
        }
    }
    #endregion

    #region INetworkRunnerCallbacks (debug log only)
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("OnConnectedToServer");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Shutdown: {shutdownReason}");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"OnDisconnectedFromServer: {reason}");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log($"OnConnectFailed: {reason}");
    }
    #endregion

    #region INetworkRunnerCallbacks (unused)
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }
    #endregion
}
