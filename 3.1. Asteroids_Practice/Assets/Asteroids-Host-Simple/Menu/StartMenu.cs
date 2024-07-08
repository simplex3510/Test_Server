using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Asteroids.HostSimple
{
    public class StartMenu : MonoBehaviour
    {
        [SerializeField] private NetworkRunner _networkRunnerPrefab = null;
        [SerializeField] private PlayerData _playerDataPrefab = null;

        [SerializeField] private TMP_InputField _nickName = null;
        [SerializeField] private TextMeshProUGUI _nickNamePlaceholder = null;

        [SerializeField] private TMP_InputField _roomName = null;
        [SerializeField] private string _gameSceneName = null;

        private NetworkRunner _runnerInstance = null;

        public void StartHost()
        {
            SetPlayerData();
            StartGame(GameMode.AutoHostOrClient, _roomName.text, _gameSceneName);
        }

        public void StartClient()
        {
            SetPlayerData();
            StartGame(GameMode.Client, _roomName.text, _gameSceneName);
        }

        private void SetPlayerData()
        {
            var playerData = FindObjectOfType<PlayerData>();
            if (playerData == null)
            {
                playerData = Instantiate(_playerDataPrefab);
            }

            if (string.IsNullOrWhiteSpace(_nickName.text))
            {
                playerData.SetNickName(_nickNamePlaceholder.text);
            }
            else
            {
                playerData.SetNickName(_nickName.text);
            }
        }

        private async void StartGame(GameMode gameMode, string roomName, string sceneName)
        {
            _runnerInstance = FindObjectOfType<NetworkRunner>();
            if (_runnerInstance == null)
            {
                _runnerInstance = Instantiate(_networkRunnerPrefab);
            }
            _runnerInstance.ProvideInput = true;

            var startGameArgs = new StartGameArgs()
            {
                GameMode = gameMode,
                SessionName = roomName,
                ObjectProvider = _runnerInstance.GetComponent<NetworkObjectPoolDefault>(),
            };

            await _runnerInstance.StartGame(startGameArgs);

            if (_runnerInstance.IsServer)
            {
                _runnerInstance.LoadScene(sceneName);
            }
        }
    }
}
