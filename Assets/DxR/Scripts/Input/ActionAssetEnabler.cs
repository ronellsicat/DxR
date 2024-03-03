using UnityEngine;
using UnityEngine.InputSystem;

public class ActionAssetEnabler : MonoBehaviour {
	[SerializeField]
	InputActionAsset _actionAsset;
	public InputActionAsset actionAsset {
		get => _actionAsset;
		set => _actionAsset = value;
	}

	private void OnEnable() {
		if (_actionAsset != null) {
			_actionAsset.Enable();
		}
	}

}