using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Asteroids.HostSimple
{
    public class NickNameGeneration : MonoBehaviour
    {
        private void Awake()
        {
            // 한 번만 사용되는 변수라서 따로 멤버 변수로 저장하지 않음 - 메모리 낭비
            var nicknameInputField = GetComponentInChildren<TextMeshProUGUI>();
            nicknameInputField.text = PlayerData.GetRandomNickName();
        }
    }
}
