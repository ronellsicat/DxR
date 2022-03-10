using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;

public class ActionToActive : MonoBehaviour {
	[SerializeField] private InputActionReference _actionReference = null;

	[SerializeField] private GameObject _target = null;

	bool _debug = false;
	private bool _debugAttached = false;

	private void OnEnable() {

	}

	private void Start() {
		if (null == _target) _target = gameObject;
		Assert.IsNotNull(_actionReference);
		Assert.IsNotNull(_target);

		_target.SetActive(_actionReference.action.ReadValue<int>() > 0);

		if (_debug) Debug.Log($"Activating {_target.name} from action reference [{_actionReference}], " +
			$"initial={_actionReference.action.ReadValue<int>()}");
		_actionReference.action.started += InputCallback;
		_actionReference.action.canceled += InputCallback;
	}

	private void OnValidate() {
		if (_debug && !_debugAttached) {
			_actionReference.action.started += DebugCallback;
			_actionReference.action.canceled += DebugCallback;
			_debugAttached = true;
		} else if (!_debug && _debugAttached) {
			_actionReference.action.started -= DebugCallback;
			_actionReference.action.canceled -= DebugCallback;
			_debugAttached = false;
		}
	}

	private void DebugCallback(InputAction.CallbackContext context) => Debug.Log($"{context.action} {context.phase} " +
		$"val={context.ReadValue<int>()}");
	private void InputCallback(InputAction.CallbackContext context) {
		if (enabled) _target.SetActive(context.ReadValue<int>() > 0);
	}
}

