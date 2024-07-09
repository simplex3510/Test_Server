using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.HostSimple
{
    public class AsteroidBehaviour : NetworkBehaviour
    {
        [HideInInspector][Networked] public NetworkBool IsBig { get; set; }
        [Networked] private NetworkBool _wastHit { get; set; }
        [Networked] private TickTimer _despawnTimer { get; set; }

        public bool IsAlive => !(_wastHit);
        [SerializeField] private int _points = 1;
        private NetworkRigidbody3D _networkRigidbody;

        public override void Spawned()
        {
            _networkRigidbody = GetComponent<NetworkRigidbody3D>();
            _networkRigidbody.InterpolationTarget.localScale = Vector3.one;
        }

        public void HitAsteroid(PlayerRef player)
        {
            if (Object == null)
                return;

            if (Object.HasStateAuthority == false)
                return;

            if (_wastHit)
                return;

            if (Runner.TryGetPlayerObject(player, out var playerNetworkObjct))
            {
                playerNetworkObjct.GetComponent<PlayerDataNetworked>().AddToScore(_points);
            }
        }
    }
}