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

                    TrackNewPlayer(playerObject.GetComponent<PlayerDataNetworked>().Id);
                }
            }

            Runner.SetIsSimulated(Object, true);

            // Host
            if (Object.HasStateAuthority == false)
                return;

            _gameState = GameState.Starting;
            _timer = TickTimer.CreateFromSeconds(Runner,  _startDelay);
            
        }

        public override void FixedUpdateNetwork()
        {
            switch (_gameState)
            {
                case GameState.Starting:
                    break;
                case GameState.Running:
                    break;
                case GameState.Ending:
                    break;
                default:
                    break;
            }
        }

        private void UpdateStartingDisplay()
        {
            // Host & Client
            _startEndDisplay.text = $"Game Starts In {Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0)}";

            // Host
            if (Object.HasStateAuthority == false)
                return;

            if (_timer.ExpiredOrNotRunning(Runner) == false)
                return;

            // FindObjectOfType<>()
            // FindObjectOfType<>()

            _gameState = GameState.Running;
            _timer = TickTimer.CreateFromSeconds(Runner, _gameSessionLength);
        }

        private void UpdateRunningDisplay()
        {
            // Host & Client
            _startEndDisplay.gameObject.SetActive(false);
            _ingameTimerDisplay.gameObject.SetActive(true);
            _ingameTimerDisplay.text = $"{Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0).ToString("000")} seconds left";
        }

        private void UpdateEndingDisplay()
        {
            // Host & Client
            if (Runner.TryFindBehaviour(_winner, out PlayerDataNetworked playerData) == false)
                return;

            _startEndDisplay.gameObject.SetActive(true);
            _ingameTimerDisplay.gameObject.SetActive(false);
            _startEndDisplay.text = $"{playerData.NickName} won with {playerData.Score} points. Desconneting in {Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0)}";
            // _startEndDisplay.color = SpaceshipVisualController

            // Host
            if (_timer.ExpiredOrNotRunning(Runner) == false)
                return;

            Runner.Shutdown();
        }

        public void TrackNewPlayer(NetworkBehaviourId playerDataNetworkedId)
        {
            _playerDataNetworkedIds.Add(playerDataNetworkedId);
        }
    }
}