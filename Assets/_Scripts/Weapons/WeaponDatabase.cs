using AbsoluteCommons.Objects;
using System.Collections.Generic;
using TowerDefense.Weapons.Projectiles;
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

		public GameObject InstantiateWeapon(WeaponType weapon) {
			GameObject prefab = weapons[(int)weapon];

			if (prefab == null) {
				Debug.LogError($"WeaponDatabase: No weapon found for {weapon}");
				return null;
			}

			return Instantiate(prefab);
		}

		public GameObject InstantiateWeapon(WeaponType weapon, Transform parent) {
			GameObject prefab = weapons[(int)weapon];

			if (prefab == null) {
				Debug.LogError($"WeaponDatabase: No weapon found for {weapon}");
				return null;
			}

			return Instantiate(prefab, parent);
		}

		public GameObject InstantiateWeapon(WeaponType weapon, Vector3 position, Quaternion rotation) {
			GameObject prefab = weapons[(int)weapon];

			if (prefab == null) {
				Debug.LogError($"WeaponDatabase: No weapon found for {weapon}");
				return null;
			}

			return Instantiate(prefab, position, rotation);
		}

		public GameObject InstantiateWeapon(WeaponType weapon, Vector3 position, Quaternion rotation, Transform parent) {
			GameObject prefab = weapons[(int)weapon];

			if (prefab == null) {
				Debug.LogError($"WeaponDatabase: No weapon found for {weapon}");
				return null;
			}

			return Instantiate(prefab, position, rotation, parent);
		}

		public GameObject InstantiateWeapon(WeaponType weapon, Transform parent, bool worldPositionStays) {
			GameObject prefab = weapons[(int)weapon];

			if (prefab == null) {
				Debug.LogError($"WeaponDatabase: No weapon found for {weapon}");
				return null;
			}

			return Instantiate(prefab, parent, worldPositionStays);
		}
	}
}
