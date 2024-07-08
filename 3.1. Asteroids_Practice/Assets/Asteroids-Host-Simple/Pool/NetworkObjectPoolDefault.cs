using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Asteroids.HostSimple
{
    public class NetworkObjectPoolDefault : NetworkObjectProviderDefault
    {
        // �ν����Ϳ��� 3���� �Ҵ��ص� ������ ����Ʈ - Ǯ���� ��ü�� ����
        // ����Ʈ�� ��������� ��� ��ü�� Ǯ��
        [SerializeField] private List<NetworkObject> _poolableObjects;

        // Pool ��ųʸ�
        private Dictionary<NetworkObjectTypeId, Stack<NetworkObject>> _free = new();

        protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        {
            // ������(��Ʈ��ũ ������Ʈ)�� Pool�� �ִ��� ������ �˻�
            if (ShouldPool(prefab))
            {
                // Pool�� ���ٸ�

                // �ش� �������� Pool���� ��������, Pool���� ���ٸ� ����
                var instance = GetObjectFromPool(prefab);

                instance.transform.position = Vector3.zero;

                return instance;
            }

            // ������ ����Ʈ���� �ش� �������� �����ϸ� �׳� ����
            return Instantiate(prefab);
        }

        protected override void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
        {
            // ���޹��� �������� Ǯ���� ����̶��
            if (_free.TryGetValue(prefabId, out var stack))
            {
                instance.gameObject.SetActive(false);
                stack.Push(instance);
            }
            // Ǯ���� ����� �ƴ϶��
            else
            {
                Destroy(instance.gameObject);
            }
        }

        // Pool���� ��Ʈ��ũ ������Ʈ�� �������� �޼ҵ�
        private NetworkObject GetObjectFromPool(NetworkObject prefab)
        {
            NetworkObject instance = null;

            // Pool���� NetworkTypeId�� ���� Stack�� �������� �õ�
            if (_free.TryGetValue(prefab.NetworkTypeId, out var stack))
            {
                // ������ ��Ҹ� pop�ϰ� instance�� ����, Ż��
                while (0 < stack.Count && instance == null)
                {
                    instance = stack.Pop();
                }
            }

            // 1. ���ÿ� �ƹ� �͵� ���� �� || 2. ������ �������� �� ���� ��
            if (instance == null)
            {
                // ��Ʈ��ũ ������Ʈ�� �����Ƿ� ����
                instance = GetNewInstance(prefab);
            }

            instance.gameObject.SetActive(true);
            
            return instance;
        }

        // ���޹��� ������(��Ʈ��ũ ������Ʈ)�� ����
        private NetworkObject GetNewInstance(NetworkObject prefab)
        {
            // ���ο� ��Ʈ��ũ ������Ʈ�� ����
            NetworkObject instance = Instantiate(prefab);

            // Pool���� ������ �����ͺ�, �ٵ� ���� ��ü�� ���ٸ�(2��° ��� �˻�)
            if (_free.TryGetValue(prefab.NetworkTypeId, out var stack) == false)
            {
                stack = new Stack<NetworkObject>();         // ���Ӱ� ������ ����
                _free.Add(prefab.NetworkTypeId, stack);     // Pool�� �ش� NetworkTypeId�� �����Ͽ� �߰�
            }

            // �̷��� ���Ӱ� ������ �ν��Ͻ��� ��ȯ
            return instance;
        }

        // Pool�� ������ �ϴ��� �˻�
        private bool ShouldPool(NetworkObject networkOject)
        {
            // poolableObjects ����Ʈ�� ����ִٸ�
            if (_poolableObjects.Count == 0)
            {
                // Pool�� ������ ��
                return true;
            }

            // poolableObjects ����Ʈ�� ������� �ʴٸ� ����
            return IsPoolableObject(networkOject);
        }

        // �ش� ��Ʈ��ũ ������Ʈ�� poolable ������Ʈ���� �˻�
        // ��, poolableObjects ����Ʈ�� ������� �˻�
        private bool IsPoolableObject(NetworkObject networkOject)
        {
            foreach (var poolableObject in _poolableObjects)
            {
                if (networkOject == poolableObject)
                {
                    // ����Ʈ�� ��Ҷ�� - pooling �ؾ� �Ǵ� ������Ʈ
                    return true;
                }
            }

            // ����Ʈ�� ��Ұ� �ƴ϶�� - pooling ���� �ʾƵ� �Ǵ� ������Ʈ
            return false;
        }
    }
}