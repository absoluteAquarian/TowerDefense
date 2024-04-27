using TowerDefense.Player;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.UI {
	[AddComponentMenu("Debug/Weapon-Info")]
	public class WeaponDebugInfo : MonoBehaviour {
		[SerializeField] private Text _info;

		private string _origText;

		private void Start() {
			_origText = _info.text;
		}

		private void Update() {
			GameObject player = GameObject.FindWithTag("Player");

			if (player)
				player.GetComponent<PlayerWeaponInfo>().UpdateDebugGUI(_info, _origText);
		}
	}
}
