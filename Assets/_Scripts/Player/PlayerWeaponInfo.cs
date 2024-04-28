using AbsoluteCommons.Attributes;
using AbsoluteCommons.Components;
using AbsoluteCommons.Objects;
using AbsoluteCommons.Utility;
using TowerDefense.CameraComponents;
using TowerDefense.Networking;
using TowerDefense.Weapons;
using TowerDefense.Weapons.Projectiles;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.Player {
	[AddComponentMenu("Player/Player Weapon Info")]
	[RequireComponent(typeof(TimersTracker), typeof(DynamicObjectPool), typeof(PlayerNetcode))]
	public class PlayerWeaponInfo : NetworkBehaviour {
		public WeaponDatabase database;

		private WeaponType _previousWeapon;

		[Header("Weapon Properties")]
		[SerializeField] private WeaponType _currentWeapon;
		public WeaponType CurrentWeapon => _currentWeapon;

		[SerializeField, ReadOnly] private WeaponType _displayedWeapon;
		public WeaponType DisplayedWeapon {
			get => _displayedWeapon;
			private set => _displayedWeapon = value;
		}

		[SerializeField, ReadOnly] private bool _hasShootCooldown;
		[SerializeField, ReadOnly] private bool _triggerFinger;

		[SerializeField, ReadOnly] private TimersTracker _timers;
		[SerializeField, ReadOnly] private DynamicObjectPool _projectileCreator;

		[Header("Animation Properties")]
		[SerializeField, ReadOnly] private bool _playWeaponAnimation;
		[SerializeField, ReadOnly] private DeployState _deployState;
		public DeployState DeployState => _deployState;
		[SerializeField, ReadOnly] private float _transitionTime;
		public float AnimationTransitionTime {
			get => _transitionTime;
			private set => _transitionTime = value;
		}

		[SerializeField, ReadOnly] private float _deployShowTime = 0f;
		[SerializeField, ReadOnly] private float _holsterHideTime = 1f;

		private Animator _firstPersonAnimator;
		private NetworkAnimator _networkFirstPersonAnimator;
		private Animator _thirdPersonAnimator;
		private NetworkAnimator _networkThirdPersonAnimator;

		private CameraFollow _camera;

		private PlayerNetcode _netcode;

		[Header("IK Properties")]
		[SerializeField, ReadOnly] private GameObject _weaponObject;
		[SerializeField, ReadOnly] private GameObject _firstPersonWeaponObject;
	//	[SerializeField, ReadOnly] private GameObject _leftHandIKTarget;
	//	[SerializeField, ReadOnly] private GameObject _firstPersonLeftHandIKTarget;
	//	[SerializeField, ReadOnly] private GameObject _rightHandIKTarget;
	//	[SerializeField, ReadOnly] private GameObject _firstPersonRightHandIKTarget;

		public Weapon GetWeaponObject() {
			GameObject obj = _camera.target.IsObjectOrParentOfObject(gameObject) && _camera.FirstPersonRenderingMode ? _firstPersonWeaponObject : _weaponObject;
			return obj ? obj.GetComponent<Weapon>() : null;
		}

		private static int resetTimersAction = -1;

		private void Awake() {
			// NOTE: the child paths may need to be changed if this script is used in a different project
			_firstPersonAnimator = gameObject.GetChildComponent<Animator>("Animator/Y Bot Arms");
			_networkFirstPersonAnimator = _firstPersonAnimator != null ? _firstPersonAnimator.GetComponent<NetworkAnimator>() : null;
			_thirdPersonAnimator = gameObject.GetChildComponent<Animator>("Animator/Y Bot");
			_networkThirdPersonAnimator = _thirdPersonAnimator != null ? _thirdPersonAnimator.GetComponent<NetworkAnimator>() : null;

			_camera = Camera.main.GetComponent<CameraFollow>();

			_timers = GetComponent<TimersTracker>();
			_projectileCreator = GetComponent<DynamicObjectPool>();

			if (resetTimersAction == -1)
				resetTimersAction = TimersTracker.RegisterCompletionAction(ResetShootTriggers);

			_netcode = GetComponent<PlayerNetcode>();
		}

		/*
		public override void OnNetworkSpawn() {
			// First person model isn't needed for other clients
			// TODO: allow clients to be in first person mode for another player?
			if (!base.IsOwner) {
				Destroy(gameObject.GetChild("Animator/Y Bot Arms"));

				// Remove all RenderInFirstPerson components
				// The component has logic to disable its behavior if the player isn't the local player, so this is just a precaution
				foreach (RenderInFirstPerson render in gameObject.GetComponentsInChildren<RenderInFirstPerson>(true))
					Destroy(render);

				_firstPersonAnimator = null;
				_networkFirstPersonAnimator = null;
			}

			base.OnNetworkSpawn();
		}
		*/

		private void Start() {
			// SetWeapon is responsible for initializing the animators and certain animation-related properties
			SetWeapon(_currentWeapon);
			_previousWeapon = _currentWeapon;
		}

		private void Update() {
			// Trigger weapon change animation if the weapon has changed
			if (_previousWeapon != _currentWeapon) {
				SetWeapon(_currentWeapon);
				_previousWeapon = _currentWeapon;
			}

			if (!_playWeaponAnimation) {
				// Ensure that the player can't get stuck in a Deploying or Holstering state
				if (_deployState == DeployState.Deploying) {
					_deployState = DeployState.Holstered;  // Force the weapon to deploy
					InitWeaponObject();
				} else if (_deployState == DeployState.Holstering)
					DestroyWeaponObject();
			}

			// Ensure that the weapon is visible when deployed and not visible when holstered
			if (_deployState == DeployState.Deployed) {
				if (!_weaponObject)
					InitWeaponObject();
			} else if (_deployState == DeployState.Holstered) {
				if (_weaponObject)
					DestroyWeaponObject();
			}

			if (base.IsOwner) {
				if (ClientInput.IsTriggered("Deploy Weapon"))
					DeployWeapon();
				else if (ClientInput.IsTriggered("Holster Weapon"))
					HolsterWeapon();
			
				if (CanShootWeapon())
					ShootWeapon();
				else
					_triggerFinger = false;
			}

			// Update the animator
			if (_firstPersonAnimator)
				_firstPersonAnimator.SetInteger("weaponState", (int)_deployState);

			if (_thirdPersonAnimator)
				_thirdPersonAnimator.SetInteger("weaponState", (int)_deployState);
		}

		internal void UpdateDebugGUI(Text text, string origText) {
			text.text = origText
				.Replace("<TYPE>", _currentWeapon.ToString())
				.Replace("<STATE>", _deployState.ToString())
				.Replace("<STATE_TIME>", _transitionTime.ToString("0.000"));
		}

		public void SetWeapon(WeaponType weapon) {
			_currentWeapon = weapon;

			Weapon info = database.GetWeaponInfo(_currentWeapon);
			if (info) {
				_deployShowTime = Mathf.Clamp(info.deployShowTime, 0f, 1f);
				_holsterHideTime = Mathf.Clamp(info.holsterHideTime, 0f, 1f);
			}

			/*
			// Only deploy if the game is running
			if (Application.isPlaying) {
				_deployState = DeployState.Holstered;  // Force the weapon to deploy
				DeployWeapon();
			}
			*/

			if (base.IsOwner) {
				if (_firstPersonAnimator)
					_firstPersonAnimator.SetInteger("weaponID", (int)_currentWeapon);

				if (_thirdPersonAnimator)
					_thirdPersonAnimator.SetInteger("weaponID", (int)_currentWeapon);

				ChangeWeaponTypeServerRpc(new WeaponTypeChangeMessage(weapon));
			}
		}

		[ServerRpc]
		private void ChangeWeaponTypeServerRpc(WeaponTypeChangeMessage msg) {
			ChangeWeaponTypeClientRpc(msg);
		}

		[ClientRpc]
		private void ChangeWeaponTypeClientRpc(WeaponTypeChangeMessage msg) {
			if (base.IsOwner)
				return;

			SetWeapon(msg.weapon);
		}

		public void DeployWeapon(bool immediate = false) {
			if (_deployState != DeployState.Holstered && _currentWeapon != WeaponType.None)
				return;

			if (_currentWeapon == WeaponType.None) {
				HolsterWeapon();
				return;
			}

			if (immediate) {
				_deployState = DeployState.Deployed;
				_transitionTime = 1.0f;
				_playWeaponAnimation = false;
				InitWeaponObject();
				
				if (base.IsOwner) {
					if (_networkFirstPersonAnimator)
						_networkFirstPersonAnimator.ForceTrigger("immediateDeployWeapon");

					if (_networkThirdPersonAnimator)
						_networkThirdPersonAnimator.ForceTrigger("immediateDeployWeapon");

					WeaponStateServerRpc(new WeaponStateMessage(deploying: true, immediate: true));
				}

				return;
			}

			DestroyWeaponObject();

			_deployState = DeployState.Deploying;
			_displayedWeapon = WeaponType.None;
			_transitionTime = 0.0f;
			_playWeaponAnimation = true;

			if (base.IsOwner) {
				if (_networkFirstPersonAnimator)
					_networkFirstPersonAnimator.SetTrigger("deployWeapon");

				if (_networkThirdPersonAnimator)
					_networkThirdPersonAnimator.SetTrigger("deployWeapon");

				WeaponStateServerRpc(new WeaponStateMessage(deploying: true, immediate: false));
			}
		}

		public void HolsterWeapon(bool immediate = false) {
			if (_deployState != DeployState.Deployed && _currentWeapon != WeaponType.None)
				return;

			if (_currentWeapon == WeaponType.None)
				return;

			if (immediate) {
				_deployState = DeployState.Holstered;
				_transitionTime = 1.0f;
				_playWeaponAnimation = false;
				DestroyWeaponObject();

				if (base.IsOwner) {
					if (_networkFirstPersonAnimator)
						_networkFirstPersonAnimator.ForceTrigger("immediateHolsterWeapon");

					if (_networkThirdPersonAnimator)
						_networkThirdPersonAnimator.ForceTrigger("immediateHolsterWeapon");

					WeaponStateServerRpc(new WeaponStateMessage(deploying: false, immediate: true));
				}

				return;
			}

			_deployState = DeployState.Holstering;
			_transitionTime = 0.0f;
			_playWeaponAnimation = true;

			if (base.IsOwner) {
				if (_networkFirstPersonAnimator)
					_networkFirstPersonAnimator.SetTrigger("holsterWeapon");

				if (_networkThirdPersonAnimator)
					_networkThirdPersonAnimator.SetTrigger("holsterWeapon");

				WeaponStateServerRpc(new WeaponStateMessage(deploying: false, immediate: false));
			}
		}

		[ServerRpc]
		private void WeaponStateServerRpc(WeaponStateMessage msg) {
			WeaponStateClientRpc(msg);
		}

		[ClientRpc]
		private void WeaponStateClientRpc(WeaponStateMessage msg) {
			if (IsOwner)
				return;

			if (msg.deploying)
				DeployWeapon(msg.immediate);
			else
				HolsterWeapon(msg.immediate);
		}

		private bool CanShootWeapon() {
			if (_hasShootCooldown || _deployState != DeployState.Deployed || _currentWeapon == WeaponType.None)
				return false;

			Weapon info = database.GetWeaponInfo(_currentWeapon);

			if (!info.autoFire)
				return ClientInput.IsTriggered("Fire");

			return ClientInput.IsPressed("Fire");
		}

		private void ShootWeapon() {
			_hasShootCooldown = true;

			Weapon info = GetWeaponObject();

			ShootWeapon_HandleSpawnNetworking();
			
			if (!info.autoFire || !_triggerFinger) {
				Timer timer = Timer.CreateCountdown(resetTimersAction, info.shootTime, repeating: info.autoFire);
				timer.Start();

				_timers.AddTimer(timer);
			}

			ShootWeapon_SetAnimators();

			_triggerFinger = info.autoFire;
		}

		// TODO: RPCs should sync a struct telling the WeaponType, the position vector and rotation quaternion (syncing the entire GameObject is unreliable)

		private void ShootWeapon_HandleSpawnNetworking() {
			if (!IsOwner)
				return;

			if (IsServer)
				ShootWeapon_SpawnProjectile(CreateSpawnMessage());
			else
				RequestProjectileSpawnServerRpc();
		}

		[ServerRpc]
		private void RequestProjectileSpawnServerRpc() {
			SpawnProjectileClientRpc();
		}

		[ClientRpc]
		private void SpawnProjectileClientRpc() {
			if (!base.IsOwner)
				ShootWeapon_SpawnProjectile(CreateSpawnMessage());
		}

		private ProjectileSpawnMessage CreateSpawnMessage() {
			Weapon info = GetWeaponObject();
			return new ProjectileSpawnMessage(_currentWeapon, info.projectileOrigin.position, info.projectileOrigin.rotation, !_triggerFinger ? 0 : info.spread);
		}

		private void ShootWeapon_SpawnProjectile(ProjectileSpawnMessage msg) {
			// Ensure that the prefab for the projectile has been set
			_projectileCreator.SetPrefab(database.GetWeaponInfo(msg.weaponType).projectilePrefab);

			GameObject projectile = _projectileCreator.Get();

			if (projectile) {
				projectile.transform.SetPositionAndRotation(_netcode.FirstPersonCameraTarget, _netcode.FirstPersonLookRotation);

				if (projectile.TryGetComponent(out RaycastBullet bullet)) {
					CheckRaycast(msg, bullet);
					return;
				}
			}
		}

		private void CheckRaycast(ProjectileSpawnMessage msg, RaycastBullet bullet) {
			Weapon info = database.GetWeaponInfo(msg.weaponType);

			Vector3 origin = bullet.transform.position;
			Quaternion rotation = bullet.transform.rotation;

			// Apply spread to the bullet
			if (info.spread > 0 && _triggerFinger) {
				rotation *= Quaternion.Euler(msg.weaponSpread.x, msg.weaponSpread.y, 0);
				bullet.transform.rotation = rotation;
			}

			Vector3 forward = rotation * Vector3.forward;
			Vector3 end = origin + forward * bullet.Range;

			// Only the server should check for raycasts; any interactions will be forwarded to the clients
			if (base.IsServer) {
				// Check if the raycast hits anything
				Ray ray = new Ray(origin, forward);
				if (Physics.Raycast(ray, out RaycastHit hit, bullet.Range, gameObject.layer.ToLayerMask().Exclusion())) {
					end = hit.point;

					GameObject hitObject = hit.collider.gameObject;

					// TODO: apply damage to the hit object
				}
			}

			// If there isn't a line of sight from where the projectile actually spawns and where the trail starts, destroy the projectile
			// This is to prevent the trail from being visible through walls
			if (Physics.Linecast(origin, msg.weaponPosition, gameObject.layer.ToLayerMask().Exclusion())) {
				bullet.GetComponent<PooledObject>().ReturnToPool();
				return;
			}

			bullet.trailStart = msg.weaponPosition;
			bullet.trailEnd = end;
		}

		private void ShootWeapon_SetAnimators() {
			if (_networkFirstPersonAnimator)
				_networkFirstPersonAnimator.SetTrigger("shoot");

			if (_networkThirdPersonAnimator)
				_networkThirdPersonAnimator.SetTrigger("shoot");
		}

		private static void ResetShootTriggers(GameObject obj, Timer timer) {
			PlayerWeaponInfo self = obj.GetComponent<PlayerWeaponInfo>();

			self._hasShootCooldown = false;

			if (self._networkFirstPersonAnimator)
				self._networkFirstPersonAnimator.ResetTrigger("shoot");

			if (self._networkThirdPersonAnimator)
				self._networkThirdPersonAnimator.ResetTrigger("shoot");

			// If the weapon autofires, check the input and shoot again
			// Checking in Update may be too late
			Weapon info = self.GetWeaponObject();

			if (info.autoFire && self.CanShootWeapon()) {
				self._triggerFinger = true;
				self.ShootWeapon_HandleSpawnNetworking();
				self.ShootWeapon_SetAnimators();
				self._hasShootCooldown = true;
			} else {
				self._timers.RemoveTimer(timer);
				self._triggerFinger = false;
			}
		}

		public void TickWeaponTransition(float normalizedTime) {
			if (_playWeaponAnimation) {
				float oldTime = _transitionTime;
				_transitionTime = normalizedTime;
				if (_transitionTime >= 1.0f) {
					_transitionTime = 1.0f;
					_playWeaponAnimation = false;
				}

				CheckWeaponVisibility(oldTime, _transitionTime);
			}
		}

		private void CheckWeaponVisibility(float lastTick, float currentTick) {
			switch (_deployState) {
				case DeployState.Deploying:
					if (lastTick < _deployShowTime && currentTick >= _deployShowTime) {
						// Make the weapon visible
						InitWeaponObject();
					}
					break;
				case DeployState.Holstering:
					if (lastTick < _holsterHideTime && currentTick >= _holsterHideTime) {
						// Hide the weapon
						DestroyWeaponObject();
					}
					break;
			}
		}

		private void InitWeaponObject() {
			DestroyWeaponObject();
			_displayedWeapon = _currentWeapon;
			_deployState = DeployState.Deployed;
			
			if (_displayedWeapon == WeaponType.None)
				return;

			_weaponObject = database.InstantiateWeapon(_displayedWeapon);
			AttachWeaponToHand();

			if (base.IsOwner) {
				_firstPersonWeaponObject = database.InstantiateWeapon(_displayedWeapon, clientside: true);
				AttachWeaponToFirstPersonHand();
			}

			Weapon info = _weaponObject.GetComponent<Weapon>();

			if (_firstPersonAnimator)  {
				_firstPersonAnimator.SetBool("weaponDeployed", true);
				_firstPersonAnimator.SetFloat("shootSpeed", info.shootAnimationMultiplier);
			}

			if (_thirdPersonAnimator) {
				_thirdPersonAnimator.SetBool("weaponDeployed", true);
				_thirdPersonAnimator.SetFloat("shootSpeed", info.shootAnimationMultiplier);
			}
		}

		private void AttachWeaponToHand() {
			if (!_weaponObject)
				return;

			Weapon info = _weaponObject.GetComponent<Weapon>();
			if (info) {
				// Attach the weapon to the right hand bone
				Transform rightHand = _thirdPersonAnimator.GetBoneTransform(HumanBodyBones.RightHand);
				info.rightHandBone.SetParent(rightHand, false);

				// The player is right-handed, so only the left hand needs to be set up for IK
			//	_leftHandIKTarget = info.leftHandBone;
			}
		}

		private void AttachWeaponToFirstPersonHand() {
			if (!_firstPersonWeaponObject)
				return;

			Weapon info = _firstPersonWeaponObject.GetComponent<Weapon>();
			if (info) {
				// Attach the weapon to the right hand bone
				Transform rightHand = _firstPersonAnimator.GetBoneTransform(HumanBodyBones.RightHand);
				info.rightHandBone.SetParent(rightHand, false);

				// The player is right-handed, so only the left hand needs to be set up for IK
			//	_firstPersonLeftHandIKTarget = info.leftHandBone;
			}
		}

		private void DestroyWeaponObject() {
			_displayedWeapon = WeaponType.None;
			_deployState = DeployState.Holstered;

			TypeExtensions.DestroyOrDespawnAndSetNull(ref _weaponObject);
			TypeExtensions.DestroyOrDespawnAndSetNull(ref _firstPersonWeaponObject);
		//	TypeExtensions.DestroyAndSetNull(ref _leftHandIKTarget);
		//	TypeExtensions.DestroyAndSetNull(ref _firstPersonLeftHandIKTarget);
		//	TypeExtensions.DestroyAndSetNull(ref _rightHandIKTarget);
		//	TypeExtensions.DestroyAndSetNull(ref _firstPersonRightHandIKTarget);

			if (_firstPersonAnimator)
				_firstPersonAnimator.SetBool("weaponDeployed", false);

			if (_thirdPersonAnimator)
				_thirdPersonAnimator.SetBool("weaponDeployed", false);
		}

		// Networking stuff
		private struct ProjectileSpawnMessage : INetworkSerializable {
			public WeaponType weaponType;
			public Vector3 weaponPosition;
			public Quaternion weaponRotation;
			public Vector2 weaponSpread;

			public ProjectileSpawnMessage(WeaponType weaponType, Vector3 weaponPosition, Quaternion weaponRotation, float weaponSpread) {
				this.weaponType = weaponType;
				this.weaponPosition = weaponPosition;
				this.weaponRotation = weaponRotation;
				this.weaponSpread = weaponSpread > 0 ? new Vector2(Random.Range(-weaponSpread, weaponSpread), Random.Range(-weaponSpread, weaponSpread)) : Vector2.zero;
			}

			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
				if (serializer.IsWriter) {
					var writer = serializer.GetFastBufferWriter();

					writer.WriteValueSafe(weaponType);
					writer.WriteValueSafe(weaponPosition);
					writer.WriteValueSafe(weaponRotation);
					writer.WriteValueSafe(weaponSpread);
				} else {
					var reader = serializer.GetFastBufferReader();

					reader.ReadValueSafe(out weaponType);
					reader.ReadValueSafe(out weaponPosition);
					reader.ReadValueSafe(out weaponRotation);
					reader.ReadValueSafe(out weaponSpread);
				}
			}
		}

		private struct WeaponStateMessage : INetworkSerializable {
			public bool deploying;
			public bool immediate;

			public WeaponStateMessage(bool deploying, bool immediate) {
				this.deploying = deploying;
				this.immediate = immediate;
			}

			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
				if (serializer.IsWriter) {
					var writer = serializer.GetFastBufferWriter();

					using (var bitWriter = writer.EnterBitwiseContext()) {
						bitWriter.TryBeginWriteBits(1);

						bitWriter.WriteBit(deploying);
					}
				} else {
					var reader = serializer.GetFastBufferReader();

					using (var bitReader = reader.EnterBitwiseContext()) {
						bitReader.TryBeginReadBits(1);

						bitReader.ReadBit(out deploying);
					}
				}
			}
		}

		private struct WeaponTypeChangeMessage : INetworkSerializable {
			public WeaponType weapon;

			public WeaponTypeChangeMessage(WeaponType weapon) {
				this.weapon = weapon;
			}

			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
				if (serializer.IsWriter) {
					var writer = serializer.GetFastBufferWriter();

					writer.WriteValueSafe(weapon);
				} else {
					var reader = serializer.GetFastBufferReader();

					reader.ReadValueSafe(out weapon);
				}
			}
		}

		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
			if (serializer.IsWriter) {
				var writer = serializer.GetFastBufferWriter();
				writer.WriteValueSafe(_currentWeapon);
				writer.WriteValueSafe(_previousWeapon);
				writer.WriteValueSafe(_displayedWeapon);
				writer.WriteValueSafe(_deployState);
				writer.WriteValueSafe(_transitionTime);
				writer.WriteValueSafe(_deployShowTime);
				writer.WriteValueSafe(_holsterHideTime);

				using (var bitWriter = writer.EnterBitwiseContext()) {
					bitWriter.TryBeginWriteBits(3);

					bitWriter.WriteBit(_hasShootCooldown);
					bitWriter.WriteBit(_triggerFinger);
					bitWriter.WriteBit(_playWeaponAnimation);
				}
			} else {
				var reader = serializer.GetFastBufferReader();
				reader.ReadValueSafe(out _currentWeapon);
				reader.ReadValueSafe(out _previousWeapon);
				reader.ReadValueSafe(out _displayedWeapon);
				reader.ReadValueSafe(out _deployState);
				reader.ReadValueSafe(out _transitionTime);
				reader.ReadValueSafe(out _deployShowTime);
				reader.ReadValueSafe(out _holsterHideTime);

				using (var bitReader = reader.EnterBitwiseContext()) {
					bitReader.TryBeginReadBits(3);

					bitReader.ReadBit(out _hasShootCooldown);
					bitReader.ReadBit(out _triggerFinger);
					bitReader.ReadBit(out _playWeaponAnimation);
				}
			}

			base.OnSynchronize(ref serializer);
		}

		// TODO: none of this works.  too lazy to fix it right now though

		public void HandleFirstPersonIK(Animator animator) {
		//	HandleIK(animator, _firstPersonLeftHandIKTarget, null);
		}

		public void HandleThirdPersonIK(Animator animator) {
		//	HandleIK(animator, _leftHandIKTarget, null);
		}

		private void HandleIK(Animator animator, GameObject leftHandTarget, GameObject rightHandTarget) {
			if (animator) {
				// IK should only be active when weapon is fully deployed; the other states either have a transition animation or no weapon
				if (_deployState == DeployState.Deployed) {
					// Set the right hand target position and rotation, if one has been assigned
					if (rightHandTarget)
						HandleIK_SetParameters(animator, true, rightHandTarget);
					else
						ClearRightHandIK(animator);

					// Set the left hand target position and rotation, if one has been assigned
					if (leftHandTarget)
						HandleIK_SetParameters(animator, false, leftHandTarget);
					else
						ClearLeftHandIK(animator);
				} else {
					ClearRightHandIK(animator);
					ClearLeftHandIK(animator);
				}
			}
		}

		private void HandleIK_SetParameters(Animator animator, bool rightHand, GameObject target) {
			AvatarIKGoal goal = rightHand ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand;
			AvatarIKHint hint = rightHand ? AvatarIKHint.RightElbow : AvatarIKHint.LeftElbow;

			animator.SetIKPositionWeight(goal, 1.0f);
			animator.SetIKRotationWeight(goal, 1.0f);
			animator.SetIKPosition(goal, target.transform.position);

			Quaternion rotation = target.transform.rotation;
			// Adjust the rotation so that the hand is in the correct orientation (it is offset by a 90 degree rotation in two axes)
			// TODO: right hand doesn't use IK, will this work?
			rotation *= rightHand ? Quaternion.Euler(0, 90, 90) : Quaternion.Euler(0, -90, 90);

			animator.SetIKRotation(goal, rotation);

			// Set the hint position
			animator.SetIKHintPositionWeight(hint, 1.0f);
			animator.SetIKHintPosition(hint, target.transform.position);
		}

		public void ClearIK(Animator animator) {
			if (animator) {
				ClearRightHandIK(animator);
				ClearLeftHandIK(animator);
			}
		}

		private void ClearRightHandIK(Animator animator) {
			// Reset the right hand target position and rotation
			animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.0f);
			animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0.0f);
			animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0.0f);
		}

		private void ClearLeftHandIK(Animator animator) {
			// Reset the left hand target position and rotation
			animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.0f);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0.0f);
			animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0.0f);
		}
	}
}
