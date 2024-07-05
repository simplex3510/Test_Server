using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerPrefab;

    [Networked, Capacity(12)] private NetworkDictionary<PlayerRef, Player> Players => default;

    void IPlayerJoined.PlayerJoined(PlayerRef player)
    {
        if (HasStateAuthority)
        {
            NetworkObject playerObject = Runner.Spawn(playerPrefab,
                                                        Vector3.up,
                                                        Quaternion.identity,
                                                        player);

            Players.Add(player, playerObject.GetComponent<Player>());
        }
    }

    void IPlayerLeft.PlayerLeft(PlayerRef player)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (Players.TryGet(player, out Player playerBehaviour))
        {
            Players.Remove(player);
            Runner.Despawn(playerBehaviour.Object);
        }
    }
}
