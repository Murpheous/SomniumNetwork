using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SyncedControls.Example
{
    /// <summary>
    /// This class receive network events from photon and can register network objects in the scene.
    /// There can only be one instance of this class in the scene.
    /// </summary>
    public class SceneNetworking : MonoBehaviour, INetworkRunnerCallbacks
    {
        // Private Fields
        private float _t0 = 0; // Time since start of the scene
        private NetworkObject[] _sceneNetworkObjects = new NetworkObject[0];

        // # Properties
        // Public static Properties
        public static SceneNetworking Instance { get; private set; } // Singleton instance
        public static event Action<ReliableKey, byte[]> OnReliableMessageReceived; // Special message received from other player
        public static bool IsLocalPlayerJoined { get; private set; } = false; // Check if local player has joined
        public NetworkRunner NetworkRunnerRef { get; private set; }
        public SceneRef NetworkSceneRef { get; private set; }

        // Private Properties
        private static event Action _onLocalPlayerJoined; // Event when local player has joined

        private void Awake()
        {
            Instance = this;
            _t0 = Time.time;
            DebugUI.Log(nameof(SceneNetworking), "World Start, t=0s");

            GetAllNetworkObjects();

            InvokeRepeating(nameof(WaitForNetworkRunner), 0.0f, 0.05f);
        }

        /// Creators: You can use this to register a callback when the network is ready to be used.
        /// Will call the callback immediately if the networking is already ready.
        public static void RegisterOnNetworkReady(Action onNetworkReady)
        {
            if (IsLocalPlayerJoined)
                onNetworkReady?.Invoke();
            else
                _onLocalPlayerJoined += onNetworkReady;
        }

        /// Creators: You can use this to unregister a callback
        public static void UnRegisterOnNetworkReady(Action onNetworkReady)
        {
            if(onNetworkReady != null)
                _onLocalPlayerJoined -= onNetworkReady;
        }

        /// Creators: Do not use directly, use NetVar instead.
        public void SendReliableDataToAllPlayers(int type, int id, int paramA, int paramB, byte[] data)
        {
            if (NetworkRunnerRef == null)
            {
                DebugUI.LogError(nameof(SceneNetworking), "NetworkRunner Reference is null.");
                return;
            }

            var key = ReliableKey.FromInts(type, id, paramA, paramB);
            foreach (PlayerRef player in NetworkRunnerRef.ActivePlayers)
            {
                // DebugUI.Log("Send Data to " + player.PlayerId);
                NetworkRunnerRef.SendReliableDataToPlayer(player, key, data);
            }
        }

        private void WaitForNetworkRunner()
        {
            if (NetworkRunner.Instances.Count > 0)
            {
                CancelInvoke(nameof(WaitForNetworkRunner));
                NetworkRunnerRef = NetworkRunner.Instances[0];
                DebugUI.Log(nameof(SceneNetworking), "NetworkRunner ready, t=" + GetTime());
                Setup(NetworkRunnerRef);
            }
        }

        private void Setup(NetworkRunner runner)
        {
            runner.AddCallbacks(this);
        }

        private void InitScenePath()
        {
            NetworkSceneRef = SceneRef.FromPath(gameObject.scene.path);
            if(!NetworkSceneRef.IsValid)
                DebugUI.LogError(nameof(SceneNetworking), "Scene reference is not valid ! " + gameObject.scene.name + ", " + gameObject.scene.path);
        }

        private void GetAllNetworkObjects()
        {
            _sceneNetworkObjects = FindObjectsByType<NetworkObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Array.Sort(_sceneNetworkObjects, (a, b) => a.SortKey.CompareTo(b.SortKey));
            DebugUI.Log(nameof(SceneNetworking), "Found " + _sceneNetworkObjects.Length + " NetworkObjects in the scene.");
        }

        private void RegisterAllNetworkObjects()
        {
            // Register the NetworkObjects
            int r = Instance.NetworkRunnerRef.RegisterSceneObjects(NetworkSceneRef, _sceneNetworkObjects);
            DebugUI.Log(nameof(SceneNetworking), "Network objects registered: " + r);
        }

        // Network Events
        /// Called by photon, do not use.
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (player == NetworkRunnerRef.LocalPlayer)
            {
                DebugUI.Log(nameof(SceneNetworking), "Local Player Joined, t=" + GetTime());
                DebugUI.Log(nameof(SceneNetworking), $"Is Master Client={NetworkRunnerRef.IsSharedModeMasterClient}");
                InitScenePath();
                IsLocalPlayerJoined = true;
                _onLocalPlayerJoined?.Invoke();
                RegisterAllNetworkObjects();
            }
            else
            {
                DebugUI.Log(nameof(SceneNetworking), "Remote Player Joined, id=" + player.PlayerId + ", t=" + GetTime());
            }
        }

        /// Called by photon, do not use.
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            DebugUI.Log(nameof(SceneNetworking), "Player Left, " + GetTime());
        }

        /// Called by photon, do not use.
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            //DebugUI.Log(nameof(SceneNetworking), "OnReliableDataReceived received, t=" + GetTime());
            OnReliableMessageReceived?.Invoke(key, data.ToArray());
        }

        /// Called by photon, do not use.
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            //DebugUI.Log(nameof(SceneNetwork), "OnReliableDataProgress, " + GetTime());
        }

        /// Called by photon, do not use.
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            //DebugUI.Log(nameof(SceneNetwork), "Object EnterAOI, " + GetTime());
        }

        /// Called by photon, do not use.
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            //DebugUI.Log(nameof(SceneNetwork), "Object ExitAOI, " + GetTime());
        }

        /// Called by photon, do not use.
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            DebugUI.Log(nameof(SceneNetworking), "UserSimulationMessage received, t=" + GetTime());
        }

        /// Called by photon, do not use.
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            // Warning, called a lot !
        }

        /// Called by photon, do not use.
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            // DebugUI.Log(nameof(SceneNetwork), "OnInputMissing, " + GetTime());
        }

        /// Called by photon, do not use.
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            DebugUI.Log(nameof(SceneNetworking), "Shutdown, t=" + GetTime());
        }

        /// Called by photon, do not use.
        public void OnConnectedToServer(NetworkRunner runner)
        {
            DebugUI.Log(nameof(SceneNetworking), "Connected To Server, t=" + GetTime());
        }

        /// Called by photon, do not use.
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            DebugUI.Log(nameof(SceneNetworking), "Disconnected From Server, t=" + GetTime());
        }

        /// Called by photon, do not use.
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            DebugUI.Log(nameof(SceneNetworking), "Connect Request, t=" + GetTime());
        }

        /// Called by photon, do not use.
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            DebugUI.Log(nameof(SceneNetworking), "Connect Failed, t=" + GetTime());
        }

        /// Called by photon, do not use.
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            DebugUI.Log(nameof(SceneNetworking), "Session List Updated, t=" + GetTime());
        }

        /// Called by photon, do not use.
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            DebugUI.Log(nameof(SceneNetworking), "Custom Authentication Response, t=" + GetTime());
        }

        /// Called by photon, do not use.
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            DebugUI.Log(nameof(SceneNetworking), "Host Migration, t=" + GetTime());
        }

        /// Called by photon, do not use.
        public void OnSceneLoadDone(NetworkRunner runner)
        {
            DebugUI.Log(nameof(SceneNetworking), "Scene LoadDone, t=" + GetTime());
        }

        /// Called by photon, do not use.
        public void OnSceneLoadStart(NetworkRunner runner)
        {
            DebugUI.Log(nameof(SceneNetworking), "Scene Load Start, t=" + GetTime());
        }

        private string GetTime()
        {
            return (Time.time - _t0).ToString("F2")+"s";
        }
    }
}