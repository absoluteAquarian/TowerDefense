using AbsoluteCommons.Utility;
using TowerDefense.Networking;
using Unity.Netcode;
using UnityEngine;

namespace TowerDefense.Weapons {
	[CreateAssetMenu(fileName = nameof(WeaponDatabase), menuName = "Tower Defense/Weapon Database", order = 1)]
	public class WeaponDatabase : ScriptableObject {
		public GameObject[] weapons;

		public Weapon GetWeaponInfo(WeaponType weapon) {
			GameObject prefab = weapons[(int)weapon];

			if (prefab == null) {
				if (weapon != WeaponType.None)
					Debug.LogError($"WeaponDatabase: No weapon found for {weapon}");
				return null;
			}

			return prefab.GetComponent<Weapon>();
		}

		private static void CheckForNetworkSpawn(GameObject obj) {
			if (obj.TryGetComponent(out NetworkObject netObj))
				netObj.SmartSpawn(true);
		}

		public GameObject InstantiateWeapon(WeaponType weapon, bool clientside = false) {
			GameObject prefab = weapons[(int)weapon];

			if (prefab == null) {
				Debug.LogError($"WeaponDatabase: No weapon found for {weapon}");
				return null;
			}

			GameObject obj = Instantiate(prefab);

			/*
			if (clientside)
				obj.AddComponent<VisibleOnlyToLocalClient>();
			*/

			CheckForNetworkSpawn(obj);
			return obj;
		}

		public GameObject InstantiateWeapon(WeaponType weapon, Transform parent, bool clientside = false) {
			GameObject prefab = weapons[(int)weapon];

			if (prefab == null) {
				Debug.LogError($"WeaponDatabase: No weapon found for {weapon}");
				return null;
			}

			GameObject obj = Instantiate(prefab, parent);

			/*
			if (clientside)
				obj.AddComponent<VisibleOnlyToLocalClient>();
			*/

			CheckForNetworkSpawn(obj);
			return obj;
		}

		public GameObject InstantiateWeapon(WeaponType weapon, Vector3 position, Quaternion rotation, bool clientside = false) {
			GameObject prefab = weapons[(int)weapon];

			if (prefab == null) {
				Debug.LogError($"WeaponDatabase: No weapon found for {weapon}");
				return null;
			}

			GameObject obj = Instantiate(prefab, position, rotation);

			/*
			if (clientside)
				obj.AddComponent<VisibleOnlyToLocalClient>();
			*/

			CheckForNetworkSpawn(obj);
			return obj;
		}

		public GameObject InstantiateWeapon(WeaponType weapon, Vector3 position, Quaternion rotation, Transform parent, bool clientside = false) {
			GameObject prefab = weapons[(int)weapon];

			if (prefab == null) {
				Debug.LogError($"WeaponDatabase: No weapon found for {weapon}");
				return null;
			}

			GameObject obj = Instantiate(prefab, position, rotation, parent);
			
			/*
			if (clientside)
				obj.AddComponent<VisibleOnlyToLocalClient>();
			*/

			CheckForNetworkSpawn(obj);
			return obj;
		}

		public GameObject InstantiateWeapon(WeaponType weapon, Transform parent, bool worldPositionStays, bool clientside = false) {
			GameObject prefab = weapons[(int)weapon];

			if (prefab == null) {
				Debug.LogError($"WeaponDatabase: No weapon found for {weapon}");
				return null;
			}

			GameObject obj = Instantiate(prefab, parent, worldPositionStays);
			
			/*
			if (clientside)
				obj.AddComponent<VisibleOnlyToLocalClient>();
			*/

			CheckForNetworkSpawn(obj);
			return obj;
		}
	}
}
