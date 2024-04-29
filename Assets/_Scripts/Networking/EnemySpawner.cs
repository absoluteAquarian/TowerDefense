using AbsoluteCommons.Attributes;
using AbsoluteCommons.Utility;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.Networking {
	public class EnemySpawner : NetworkBehaviour {
		public GameObject enemyPrefab;

		[SerializeField, ReadOnly] private GameObject spawnedEnemy;

		private Button button;

		private void Start() {
			// Link the button in the UI to the RequestEnemy method
			button = GameObject.Find("UI Canvas").GetChild("DebugSpawnEnemy").GetComponent<Button>();
			button.onClick.AddListener(RequestEnemy);
		}

		public override void OnDestroy() {
			if (button)
				button.onClick.RemoveListener(RequestEnemy);
		}

		public void RequestEnemy() => RequestEnemySpawnServerRpc();

		[ServerRpc(RequireOwnership = false)]
		private void RequestEnemySpawnServerRpc() {
			if (spawnedEnemy && spawnedEnemy.activeSelf && spawnedEnemy.GetComponent<NetworkObject>().IsSpawned)
				return;

			spawnedEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
			spawnedEnemy.GetComponent<NetworkObject>().Spawn(true);

			RequestEnemySpawnClientRpc(new NetworkObjectReference(spawnedEnemy));
		}

		[ClientRpc]
		private void RequestEnemySpawnClientRpc(NetworkObjectReference reference) {
			spawnedEnemy = reference;
		}

		private void Update() {
			if (!IsServer)
				return;

			// If the enemy has despawned or was returned to an object pool (if applicable), reset the reference
			if (spawnedEnemy && (!spawnedEnemy.activeSelf || !spawnedEnemy.GetComponent<NetworkObject>().IsSpawned))
				spawnedEnemy = null;
		}

		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
			if (serializer.IsWriter) {
				var writer = serializer.GetFastBufferWriter();

				writer.WriteValueSafe(new NetworkObjectReference(spawnedEnemy));
			} else {
				var reader = serializer.GetFastBufferReader();

				reader.ReadValueSafe(out NetworkObjectReference reference);
				spawnedEnemy = reference;
			}

			base.OnSynchronize(ref serializer);
		}
	}
}
