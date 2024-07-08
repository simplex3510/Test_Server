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
            // �� ���� ���Ǵ� ������ ���� ��� ������ �������� ���� - �޸� ����
            var nicknameInputField = GetComponentInChildren<TextMeshProUGUI>();
            nicknameInputField.text = PlayerData.GetRandomNickName();
        }
    }
}
