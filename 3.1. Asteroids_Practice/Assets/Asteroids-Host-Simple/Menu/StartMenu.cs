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

        }

        private void SetPlayerData()
        {
            var playerData = FindObjectOfType<PlayerData>();
        }
    }
}
