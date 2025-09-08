using Netick;
using Netick.Unity;
using UnityEngine;
namespace Mirror.Examples.Pong
{
    [AddComponentMenu("")]
    public class NetworkManagerPong : NetworkBehaviour
    {
        public Transform leftRacketSpawn;
        public Transform rightRacketSpawn;
        public NetworkObject ball;
        public NetworkObject playerPrefab; // 玩家预制体（球拍）
        
        [SerializeReference] public NetworkManager networkManager;
        // public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        // {
        //     // add player at correct spawn position
        //     Transform start = numPlayers == 0 ? leftRacketSpawn : rightRacketSpawn;
        //     GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        //     NetworkServer.AddPlayerForConnection(conn, player);
        //
        //     // spawn ball if two players
        //     if (numPlayers == 2)
        //     {
        //         ball = Instantiate( networkManager.SpawnPoints.Find(prefab => prefab.name == "Ball"));
        //         NetworkServer.Spawn(ball);
        //         Sandbox.NetworkInstantiate(ball);
        //     }
        // }
        //
        // public override void OnServerDisconnect(NetworkConnectionToClient conn)
        // {
        //     // destroy ball
        //     if (ball != null)
        //         NetworkServer.Destroy(ball);
        //
        //     // call base functionality (actually destroys the player)
        //     base.OnServerDisconnect(conn);
        // }

        public override void NetworkStart()
        {
            Sandbox.Events.OnPlayerJoined += EventsOnOnPlayerJoined;
            Sandbox.Events.OnPlayerLeft += EventsOnOnPlayerLeft;
        }
        

        public override void NetworkDestroy()
        {
            Sandbox.Events.OnPlayerJoined -= EventsOnOnPlayerJoined;
            Sandbox.Events.OnPlayerLeft -= EventsOnOnPlayerLeft;
        }
        private void EventsOnOnPlayerJoined(NetworkSandbox sandbox, NetworkPlayerId  playerId)
        {
            // 根据当前玩家数量决定生成位置
            Transform spawnPoint = sandbox.Players.Count == 1 ? leftRacketSpawn : rightRacketSpawn;
            
            NetworkObject racket = sandbox.NetworkInstantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation,sandbox.GetPlayerById(playerId));
              
           Debug.Log(sandbox.Players.Count.ToString());
            if (sandbox.Players.Count == 2)
            {
                if (IsServer)
                {
                    sandbox.NetworkInstantiate(ball);
                }
            }
            sandbox.SetPlayerObject(playerId, racket);
        }
        private void EventsOnOnPlayerLeft(NetworkSandbox sandbox, NetworkPlayerId playerId)
        {
            
            // destroy ball
            if (ball != null)
                sandbox.Destroy(ball);
            
            sandbox.Destroy(sandbox.GetPlayerObject( playerId));
        }

    }
}
