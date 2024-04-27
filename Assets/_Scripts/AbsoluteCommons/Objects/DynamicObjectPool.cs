using AbsoluteCommons.Attributes;
using AbsoluteCommons.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace AbsoluteCommons.Objects {
	[AddComponentMenu("Absolute Commons/Objects/Dynamic Object Pool")]
	public class DynamicObjectPool : NetworkBehaviour {
		[SerializeField, ReadOnly] private GameObject _prefab;
		[SerializeField] private int _initialCapacity = 10;
		[SerializeField] private bool _showPoolInHierarchy = false;

		[SerializeField, ReadOnly] private List<GameObject> _pool;
		private BitArray _dirty;
		private GameObject _container;
		private int _index;

		private static GameObject _visibleObjectContainer;

		private void Awake() {
			_pool = new List<GameObject>(_initialCapacity);
			_dirty = new BitArray(_initialCapacity, true);
			_container = new GameObject("Dynamic Pool") {
				hideFlags = _showPoolInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy
			};
			_container.AddComponent<NetworkObject>();

			// Make a global pool for objects in the world
			// This is just so they don't clutter the scene list
			if (Application.isEditor && !_visibleObjectContainer) {
				_visibleObjectContainer = new GameObject("Visible Dynamic Pool Objects");
				_visibleObjectContainer.AddComponent<NetworkObject>();
			}
		}

		public override void OnNetworkSpawn() {
			if (base.IsServer) {
				NetworkObject netContainer = _container.GetComponent<NetworkObject>();
				if (!netContainer.IsSpawned)
					netContainer.SmartSpawn(true);

				netContainer = _visibleObjectContainer.GetComponent<NetworkObject>();
				if (!netContainer.IsSpawned)
					netContainer.SmartSpawn(false);

				_container.transform.SetParent(transform, false);
			}

			base.OnNetworkSpawn();
		}

		public void SetPrefab(GameObject prefab) {
			// Dirty flags do not need to be set if the prefab is the same
			if (_prefab == prefab)
				return;

			_prefab = prefab;
			_dirty.SetAll(true);
		}

		public void SetPrefab<T>(T prefab) where T : Component => SetPrefab(prefab.gameObject);

		public GameObject Get() {
			if (!base.IsServer) {
				Debug.LogError("[DynamicObjectPool] Get() can only be called on the server");
				return null;
			}

			if (_prefab == null) {
				Debug.LogError("[DynamicObjectPool] No prefab set");
				return null;
			}

			for (int i = 0; i < _pool.Count; i++) {
				_index = (_index + 1) % _pool.Count;

				GameObject obj = _pool[_index];
				if (obj == null || !obj.activeInHierarchy)
					return PrepareObject(obj);
			}

			// Reached capacity, create a new object
			_index = _pool.Count;
			return PrepareObject(AddNewObject());
		}

		private GameObject PrepareObject(GameObject obj) {
			if (!obj || _dirty[_index]) {
				// A new object is needed since the prefab has changed or the object was destroyed
				if (obj)
					obj.GetComponent<NetworkObject>().SmartDespawn(true);

				obj = Create();

				_dirty[_index] = false;

				// Inform clients that the object has changed
				AddNewObjectClientRpc(obj.GetComponent<NetworkObject>().NetworkObjectId, _index);
			}

			obj.SetActive(true);
			obj.transform.SetParent(Application.isEditor ? _visibleObjectContainer.transform : null, true);

			NetworkObject netObj = obj.GetComponent<NetworkObject>();
			if (!netObj.IsSpawned)
				netObj.SmartSpawn(true);

			PrepareObjectClientRpc(_index);

			PooledObject.EnsureConnection(obj, this, _index);

			return obj;
		}

		[ClientRpc]
		private void PrepareObjectClientRpc(int index) {
			GameObject obj = _pool[index];

			obj.SetActive(true);
		}

		private GameObject AddNewObject() {
			GameObject newObj = Create();
			_pool.Add(newObj);
			_dirty.Length++;
			_dirty[_index] = false;
			AddNewObjectClientRpc(newObj.GetComponent<NetworkObject>().NetworkObjectId, _index);
			return newObj;
		}

		[ClientRpc]
		private void AddNewObjectClientRpc(ulong networkObjectID, int index) {
			GameObject obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectID].gameObject;

			if (index < _pool.Count)
				_pool[index] = obj;
			else
				_pool.Add(obj);

			if (_dirty.Length <= index)
				_dirty.Length = index + 1;

			_dirty[index] = false;
		}

		private GameObject Create() {
			GameObject obj = Instantiate(_prefab);

			if (!obj.TryGetComponent(out NetworkObject netObj)) {
				Debug.LogError("[DynamicObjectPool] Prefab does not have a NetworkObject component");
				return null;
			}

			netObj.SmartSpawn(true);

			return obj;
		}

		[ServerRpc]
		private void ResetObjectStateServerRpc(int index) {
			ResetObjectState(_pool[index]);

			ResetObjectStateClientRpc(index);
		}

		[ClientRpc]
		private void ResetObjectStateClientRpc(int index) {
			GameObject obj = _pool[index];

			obj.SetActive(false);
		}

		private void ResetObjectState(GameObject obj) {
			/*
			NetworkObject netObj = obj.GetComponent<NetworkObject>();
			if (netObj.IsSpawned)
				netObj.Despawn(false);
			*/

			if (!obj)
				return;

			obj.SetActive(false);
			obj.transform.SetParent(_container.GetComponent<NetworkObject>().IsSpawned ? _container.transform : null, true);
		}

		public T Get<T>() where T : Component => Get().GetComponent<T>();

		public void Return(GameObject obj) {
			obj.SetActive(false);

			if (!base.IsServer && base.IsOwner)
				ResetObjectStateServerRpc(_pool.IndexOf(obj));
		}

		public void Return<T>(T component) where T : Component => Return(component.gameObject);

		public override void OnNetworkDespawn() {
			// Force all objects to despawn
			if (base.IsServer) {
				foreach (GameObject obj in _pool) {
					ResetObjectState(obj);
					obj.GetComponent<NetworkObject>().SmartDespawn(true);
				}
			}
		}
	}
}
