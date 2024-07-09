using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Asteroids.HostSimple
{
    public class GameStateController : NetworkBehaviour
    {
        enum GameState
        {
            Starting,
            Running,
            Ending
        }

        [Networked] private TickTimer _timer {  get; set; }
        [Networked] private GameState _gameState { get; set; }
        [Networked] private NetworkBehaviourId _winner {  get; set; }

        [SerializeField] private TextMeshProUGUI _startEndDisplay = null;
        [SerializeField] private TextMeshProUGUI _ingameTimerDisplay = null;

        [SerializeField] private float _startDelay = 4.0f;
        [SerializeField] private float _endDelay = 4.0f;
        [SerializeField] private float _gameSessionLength = 180.0f;

        private List<NetworkBehaviourId> _playerDataNetworkedIds = new();

        public override void Spawned()
        {
            _startEndDisplay.gameObject.SetActive(true);
            _ingameTimerDisplay.gameObject.SetActive(false);

            if (_gameState != GameState.Starting)
            {
                foreach (var player in Runner.ActivePlayers)
                {
                    if (Runner.TryGetPlayerObject(player, out var playerObject) == false)
                        continue;

                    
                }
            }
        }
    }
}