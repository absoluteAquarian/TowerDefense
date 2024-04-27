using TowerDefense.CameraComponents;
using Unity.Netcode;
using UnityEngine;

namespace TowerDefense.Networking {
	public class PlayerNetcode : NetworkBehaviour {
		private CameraFollow _camera;

		private NetworkVariable<PlayerCameraState> _cameraState;

		public float RotationHorizontal => _cameraState.Value.RotationHorizontal;

		public float RotationVertical => _cameraState.Value.RotationVertical;

		public Vector3 FirstPersonCameraTarget => _cameraState.Value.FirstPersonTarget;

		public Quaternion FirstPersonLookRotation => Quaternion.Euler(RotationVertical, RotationHorizontal, 0);

		public Vector3 ThirdPersonLookTarget => _cameraState.Value.ThirdPersonLookTarget;

		private void Awake() {
			_camera = Camera.main.GetComponent<CameraFollow>();
			_cameraState = new NetworkVariable<PlayerCameraState>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
		}

		private void Update() {
			if (IsOwner) {
				// Update and transmit the network state
				TransmitState();
			}
		}

		private void TransmitState() {
			PlayerCameraState state = new PlayerCameraState(_camera);

			if (base.IsServer)
				TransmitStateServerRpc(state);
		}

		[ServerRpc]
		private void TransmitStateServerRpc(PlayerCameraState state) {
			_cameraState.Value = state;
		}
	}

	public struct PlayerCameraState : INetworkSerializable {
		private float eulerX, eulerY;

		private Vector3 fpTarget;
		private Vector3 tpLookTarget;

		public float RotationHorizontal {
			readonly get => eulerY;
			set => eulerY = value;
		}

		public float RotationVertical {
			readonly get => eulerX;
			set => eulerX = value;
		}

		public Vector3 FirstPersonTarget {
			readonly get => fpTarget;
			set => fpTarget = value;
		}

		public Vector3 ThirdPersonLookTarget {
			readonly get => tpLookTarget;
			set => tpLookTarget = value;
		}

		public PlayerCameraState(CameraFollow camera) {
			Vector3 euler = camera.GetFirstPersonLookRotation().eulerAngles;
			eulerX = euler.x;
			eulerY = euler.y;
			fpTarget = camera.GetFirstPersonTarget();

			if (camera.CheckCameraRaycast(out var hit, 100f))
				tpLookTarget = hit.point;
			else
				tpLookTarget = camera.transform.position + camera.transform.forward * 100f;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
			if (serializer.IsWriter) {
				var writer = serializer.GetFastBufferWriter();

				writer.WriteValueSafe(eulerX);
				writer.WriteValueSafe(eulerY);
				writer.WriteValueSafe(fpTarget);
				writer.WriteValueSafe(tpLookTarget);
			} else {
				var reader = serializer.GetFastBufferReader();

				reader.ReadValueSafe(out eulerX);
				reader.ReadValueSafe(out eulerY);
				reader.ReadValueSafe(out fpTarget);
				reader.ReadValueSafe(out tpLookTarget);
			}
		}
	}
}
