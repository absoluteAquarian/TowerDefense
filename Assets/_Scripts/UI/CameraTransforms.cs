using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Debug/Camera-Transforms")]
public class CameraTransforms : MonoBehaviour, ISerializationCallbackReceiver {
	public static bool DisplayCameraLines { get; private set; } = false;

	[SerializeField] private bool showCameraDebugLines;

	[SerializeField] private Text _playerInfo;
	[SerializeField] private Text _cameraInfo;
	[SerializeField] private Text _cameraControlInfo;

	private string _origPlayerText, _origCameraText, _origCameraControlText;

	// Start is called before the first frame update
	void Start() {
		_origPlayerText = _playerInfo.text;
		_origCameraText = _cameraInfo.text;
		_origCameraControlText = _cameraControlInfo.text;
	}

	// Update is called once per frame
	void Update() {
		// Get the "Player" object
		GameObject player = GameObject.Find("Player");

		// Get the "CameraFollow" component of the main camera
		CameraFollow view = Camera.main.GetComponent<CameraFollow>();

		// Get the "FirstPersonView" component of the main camera
		FirstPersonView viewScript = Camera.main.GetComponent<FirstPersonView>();

		// Update the text
		_playerInfo.text = _origPlayerText
			.Replace("<POSITION>", FormatVector(player.transform.position))
			.Replace("<FORWARD>", FormatVector(player.transform.forward))
			.Replace("<ROTATION>", FormatVector(player.transform.rotation.eulerAngles));

		_cameraInfo.text = _origCameraText
			.Replace("<POSITION>", FormatVector(view.transform.position))
			.Replace("<FORWARD>", FormatVector(view.transform.forward))
			.Replace("<ROTATION>", FormatVector(view.transform.rotation.eulerAngles));

		_cameraControlInfo.text = _origCameraControlText
			.Replace("<LOCKED>", viewScript.IsLocked.ToString())
			.Replace("<ROTATION>", FormatVector(viewScript.ViewRotation));
	}

	private static string FormatVector(Vector3 vector) {
		return $"({Format(vector.x)}, {Format(vector.y)}, {Format(vector.z)})";
	}

	private static string Format(float value) {
		return $"{value:+0.000;-0.000}";
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize() {
		DisplayCameraLines = showCameraDebugLines;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() {
		// showCameraDebugLines = DisplayCameraLines;
	}
}
