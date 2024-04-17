using AbsoluteCommons.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbsoluteCommons.Objects {
	[AddComponentMenu("Absolute Commons/Objects/Dynamic Object Pool")]
	public class DynamicObjectPool : MonoBehaviour {
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
			_container.transform.SetParent(transform, false);

			// Make a global pool for objects in the world
			// This is just so they don't clutter the scene list
			if (Application.isEditor && _visibleObjectContainer == null) {
				_visibleObjectContainer = new GameObject("Visible Dynamic Pool Objects");
				_visibleObjectContainer.transform.SetParent(null, false);
				_visibleObjectContainer.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			}
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
			if (_prefab == null) {
				Debug.LogError("DynamicObjectPool: No prefab set");
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
			if (obj == null || _dirty[_index]) {
				// A new object is needed since the prefab has changed or the object was destroyed
				if (obj != null)
					Destroy(obj);

				obj = Create();

				_dirty[_index] = false;
			} else {
				// Just ensure the connection is still there
				PooledObject.EnsureConnection(obj, this, _index);
			}

			obj.SetActive(true);
			obj.transform.SetParent(Application.isEditor ? _visibleObjectContainer.transform : null, true);

			return obj;
		}

		private GameObject AddNewObject() {
			GameObject newObj = Create();
			_pool.Add(newObj);
			_dirty.Length++;
			_dirty[_index] = false;
			return newObj;
		}

		private GameObject Create() {
			GameObject obj = Instantiate(_prefab);
			PooledObject.EnsureConnection(obj, this, _pool.Count);
			ResetObjectState(obj);
			return obj;
		}

		private void ResetObjectState(GameObject obj) {
			obj.SetActive(false);
			obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			obj.transform.SetParent(_container.transform, false);
		}

		public T Get<T>() where T : Component => Get().GetComponent<T>();

		public void Return(GameObject obj) => ResetObjectState(obj);

		public void Return<T>(T component) where T : Component => Return(component.gameObject);
	}
}
