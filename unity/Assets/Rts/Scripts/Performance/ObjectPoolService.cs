using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Performance
{
    public sealed class ObjectPoolService : MonoBehaviour
    {
        readonly Dictionary<string, Stack<GameObject>> inactiveByKey = new Dictionary<string, Stack<GameObject>>();
        readonly Dictionary<GameObject, string> keyByObject = new Dictionary<GameObject, string>();

        Transform poolRoot;

        public int CreatedCount { get; private set; }
        public int ReusedCount { get; private set; }
        public int ReleasedCount { get; private set; }
        public int ActiveLeaseCount { get; private set; }
        public int InactiveCount { get; private set; }

        public GameObject Acquire(string key, Func<GameObject> factory, Transform parent)
        {
            if (string.IsNullOrEmpty(key))
                key = "default";
            if (factory == null)
                throw new ArgumentNullException("factory");

            EnsurePoolRoot();
            Stack<GameObject> stack;
            GameObject obj = null;
            if (inactiveByKey.TryGetValue(key, out stack))
            {
                while (stack.Count > 0 && obj == null)
                    obj = stack.Pop();
            }

            if (obj == null)
            {
                obj = factory();
                CreatedCount++;
            }
            else
            {
                ReusedCount++;
                InactiveCount = Math.Max(0, InactiveCount - 1);
            }

            keyByObject[obj] = key;
            obj.transform.SetParent(parent, false);
            obj.SetActive(true);
            ActiveLeaseCount++;
            return obj;
        }

        public void Release(GameObject obj)
        {
            if (obj == null)
                return;

            string key;
            if (!keyByObject.TryGetValue(obj, out key))
                key = obj.name;

            EnsurePoolRoot();
            obj.SetActive(false);
            obj.transform.SetParent(poolRoot, false);

            Stack<GameObject> stack;
            if (!inactiveByKey.TryGetValue(key, out stack))
            {
                stack = new Stack<GameObject>();
                inactiveByKey[key] = stack;
            }

            stack.Push(obj);
            ReleasedCount++;
            InactiveCount++;
            ActiveLeaseCount = Math.Max(0, ActiveLeaseCount - 1);
        }

        public void ClearInactive()
        {
            foreach (var pair in inactiveByKey)
            {
                var stack = pair.Value;
                while (stack.Count > 0)
                {
                    var obj = stack.Pop();
                    if (obj != null)
                        DestroyObject(obj);
                }
            }

            inactiveByKey.Clear();
            InactiveCount = 0;
        }

        void EnsurePoolRoot()
        {
            if (poolRoot != null)
                return;

            var root = new GameObject("Object Pool");
            root.transform.SetParent(transform, false);
            poolRoot = root.transform;
        }

        static void DestroyObject(UnityEngine.Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }
}
