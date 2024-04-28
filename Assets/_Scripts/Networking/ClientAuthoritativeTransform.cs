using Unity.Netcode.Components;
using UnityEngine;

namespace TowerDefense.Networking {
	[AddComponentMenu("Tower Defense/Networking/Client Authoritative Transform")]
	public class ClientAuthoritativeTransform : NetworkTransform {
		protected override bool OnIsServerAuthoritative() => false;
	}
}
