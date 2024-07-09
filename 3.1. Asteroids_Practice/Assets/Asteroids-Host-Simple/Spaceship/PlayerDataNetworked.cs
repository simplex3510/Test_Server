using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.HostSimple
{
    public class PlayerDataNetworked : NetworkBehaviour
    {
        private const int STARTING_LIVES = 3;

        [HideInInspector] [Networked] public NetworkString<_16> NickName { get; private set; }
        [HideInInspector] [Networked] public int Score { get; private set; }
        [HideInInspector] [Networked] public int Lives { get; private set; }

        private PlayerOverviewPanel _overviewPanel = null;
        private ChangeDetector _changeDetector;

        public override void Spawned()
        {
            // Client
            if (Object.HasInputAuthority)
            {
                var nickName = FindObjectOfType<PlayerData>().GetNickName();
                RpcSetNickName(nickName);
            }

            // Host
            if (Object.HasStateAuthority)
            {
                Lives = STARTING_LIVES;
                Score = 0;
            }

            // Host & Client
            _overviewPanel = FindObjectOfType<PlayerOverviewPanel>();
            _overviewPanel.AddEntry(Object.InputAuthority, this);

            _overviewPanel.UpdateNickName(Object.InputAuthority, NickName.ToString());
            _overviewPanel.UpdateScore(Object.InputAuthority, Score);
            _overviewPanel.UpdateLives(Object.InputAuthority, Lives);

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this, out var preBuffer, out var curBuffer))
            {
                switch (change)
                {
                    case nameof(NickName):
                        _overviewPanel.UpdateNickName(Object.InputAuthority, NickName.ToString());
                        break;
                    case nameof(Score):
                        _overviewPanel.UpdateScore(Object.InputAuthority, Score);
                            break;
                    case nameof(Lives):
                        _overviewPanel.UpdateLives(Object.InputAuthority, Lives);
                        break;
                }
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _overviewPanel.RemoveEntry(Object.InputAuthority);
        }

        public void AddToScore(int points)
        {
            Score += points;
        }

        public void SubtractLife()
        {
            Lives--;
        }

        #region RPC Method
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        private void RpcSetNickName(string nickName)
        {
            if (string.IsNullOrEmpty(nickName))
                return;

            NickName = nickName;
        }
        #endregion
    }
}