using AbsoluteCommons.Utility;
using Unity.Netcode;

namespace TowerDefense.Networking {
	public class DeferredSpawning : NetworkBehaviour {
		public override void OnNetworkSpawn() {
			GetComponent<NetworkObject>().SmartSpawn();
			base.OnNetworkSpawn();
		}

		public override void OnNetworkDespawn() {
			GetComponent<NetworkObject>().SmartDespawn(true, includeSelf: false);
			base.OnNetworkDespawn();
		}
	}
}
