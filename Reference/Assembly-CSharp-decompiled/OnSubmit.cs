using UnityEngine;
using UnityEngine.UI;

public class InputFieldAction : MonoBehaviour
{
	public delegate void OnSubmit();

	public OnSubmit onSubmitHandlers;

	public InputField inputField;

	protected bool wasFocusedLastFrame;

	public void Submit()
	{
		if (onSubmitHandlers != null)
		{
			onSubmitHandlers();
		}
	}

	private void Awake()
	{
		if (inputField == null)
		{
			inputField = GetComponent<InputField>();
		}
	}

	private void Update()
	{
		if (wasFocusedLastFrame && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
		{
			Submit();
		}
		wasFocusedLastFrame = inputField != null && inputField.isFocused;
	}
}
