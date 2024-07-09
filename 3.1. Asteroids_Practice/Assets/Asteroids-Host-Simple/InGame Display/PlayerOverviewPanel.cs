using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Asteroids.HostSimple
{
    public class PlayerOverviewPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerOverviewEntryPrefab = null;

        private Dictionary<PlayerRef, TextMeshProUGUI> _playerListEntries = new();

        private Dictionary<PlayerRef, string> _playerNickNames = new();
        private Dictionary<PlayerRef, int> _playerScores = new();
        private Dictionary<PlayerRef, int> _playerLives = new();

        public void AddEntry(PlayerRef playerRef, PlayerDataNetworked playerDataNetworked)
        {
            if (_playerListEntries.ContainsKey(playerRef))
                return;

            if (playerDataNetworked == null)
                return;

            var entry = Instantiate(_playerOverviewEntryPrefab, transform);
            entry.transform.localScale = Vector3.one;
            // entry.color = SpaceshipVisualController

            string nickName = string.Empty;
            int score = 0;
            int lives = 0;

            _playerNickNames.Add(playerRef, nickName);
            _playerScores.Add(playerRef, score);
            _playerLives.Add(playerRef, lives);

            _playerListEntries.Add(playerRef, entry);

            UpdateEntry(playerRef, entry);
        }

        public void RemoveEntry(PlayerRef playerRef)
        {
            if (_playerListEntries.TryGetValue(playerRef, out var entry) == false)
                return;

            if (entry != null)
            {
                Destroy(entry.gameObject);
            }

            _playerNickNames.Remove(playerRef);
            _playerScores.Remove(playerRef);
            _playerLives.Remove(playerRef);

            _playerListEntries.Remove(playerRef);
        }

        public void UpdateNickName(PlayerRef player, string nickName)
        {
            if (_playerListEntries.TryGetValue(player, out var entry) == false)
                return;

            _playerNickNames[player] = nickName;
            UpdateEntry(player, entry);
        }

        public void UpdateScore(PlayerRef player, int score)
        {
            if (_playerListEntries.TryGetValue(player, out var entry) == false)
                return;

            _playerScores[player] = score;
            UpdateEntry(player, entry);
        }

        public void UpdateLives(PlayerRef player, int lives)
        {
            if (_playerListEntries.TryGetValue(player, out var entry) == false)
                return;

            _playerLives[player] = lives;
            UpdateEntry(player, entry);
        }

        public void UpdateEntry(PlayerRef player, TextMeshProUGUI entry)
        {
            var nickName = _playerNickNames[player];
            var score = _playerScores[player];
            var lives = _playerLives[player];

            entry.text = $"{nickName}\nScore: {score}\nLives: {lives}";
        }
    }
}
