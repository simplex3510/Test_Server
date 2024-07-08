using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Asteroids.HostSimple
{
    public class NetworkObjectPoolDefault : NetworkObjectProviderDefault
    {
        // 인스펙터에서 3개를 할당해둔 상태인 리스트 - 풀링할 객체만 선별
        // 리스트가 비어있으면 모든 객체를 풀링
        [SerializeField] private List<NetworkObject> _poolableObjects;

        // Pool 딕셔너리
        private Dictionary<NetworkObjectTypeId, Stack<NetworkObject>> _free = new();

        protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        {
            // 프리팹(네트워크 오브젝트)의 Pool이 있는지 없는지 검사
            if (ShouldPool(prefab))
            {
                // Pool이 없다면

                // 해당 프리팹을 Pool에서 가져오고, Pool에서 없다면 생성
                var instance = GetObjectFromPool(prefab);

                instance.transform.position = Vector3.zero;

                return instance;
            }

            // 기존의 리스트에서 해당 프리팹이 존재하면 그냥 생성
            return Instantiate(prefab);
        }

        protected override void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
        {
            // 전달받은 프리팹이 풀링의 대상이라면
            if (_free.TryGetValue(prefabId, out var stack))
            {
                instance.gameObject.SetActive(false);
                stack.Push(instance);
            }
            // 풀링의 대상이 아니라면
            else
            {
                Destroy(instance.gameObject);
            }
        }

        // Pool에서 네트워크 오브젝트를 가져오는 메소드
        private NetworkObject GetObjectFromPool(NetworkObject prefab)
        {
            NetworkObject instance = null;

            // Pool에서 NetworkTypeId를 통해 Stack을 가져오는 시도
            if (_free.TryGetValue(prefab.NetworkTypeId, out var stack))
            {
                // 스택의 요소를 pop하고 instance에 저장, 탈출
                while (0 < stack.Count && instance == null)
                {
                    instance = stack.Pop();
                }
            }

            // 1. 스택에 아무 것도 없을 때 || 2. 스택을 가져오지 못 했을 때
            if (instance == null)
            {
                // 네트워크 오브젝트가 없으므로 생성
                instance = GetNewInstance(prefab);
            }

            instance.gameObject.SetActive(true);
            
            return instance;
        }

        // 전달받은 프리팹(네트워크 오브젝트)를 생성
        private NetworkObject GetNewInstance(NetworkObject prefab)
        {
            // 새로운 네트워크 오브젝트를 생성
            NetworkObject instance = Instantiate(prefab);

            // Pool에서 스택을 가져와봄, 근데 스택 자체가 없다면(2번째 경우 검사)
            if (_free.TryGetValue(prefab.NetworkTypeId, out var stack) == false)
            {
                stack = new Stack<NetworkObject>();         // 새롭게 스택을 만들어서
                _free.Add(prefab.NetworkTypeId, stack);     // Pool에 해당 NetworkTypeId와 맵핑하여 추가
            }

            // 이렇게 새롭게 생성된 인스턴스를 반환
            return instance;
        }

        // Pool을 만들어야 하는지 검사
        private bool ShouldPool(NetworkObject networkOject)
        {
            // poolableObjects 리스트가 비어있다면
            if (_poolableObjects.Count == 0)
            {
                // Pool을 만들어야 함
                return true;
            }

            // poolableObjects 리스트가 비어있지 않다면 실행
            return IsPoolableObject(networkOject);
        }

        // 해당 네트워크 오브젝트가 poolable 오브젝트인지 검사
        // 즉, poolableObjects 리스트의 요소인지 검사
        private bool IsPoolableObject(NetworkObject networkOject)
        {
            foreach (var poolableObject in _poolableObjects)
            {
                if (networkOject == poolableObject)
                {
                    // 리스트의 요소라면 - pooling 해야 되는 오브젝트
                    return true;
                }
            }

            // 리스트의 요소가 아니라면 - pooling 하지 않아도 되는 오브젝트
            return false;
        }
    }
}