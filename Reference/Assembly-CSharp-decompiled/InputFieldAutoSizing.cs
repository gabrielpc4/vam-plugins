using UnityEngine;
using UnityEngine.UI;

public class InputFieldAutoSizing : InputField
{
	public ScrollRect scrollRect;

	protected TextGenerator textGenerator;

	protected LayoutElement parentLayoutElement;

	protected int lastCaretPosition;

	protected void OnValueChange(string s)
	{
		if (textGenerator != null && parentLayoutElement != null)
		{
			Vector2 size = base.textComponent.rectTransform.rect.size;
			TextGenerationSettings generationSettings = base.textComponent.GetGenerationSettings(size);
			generationSettings.generateOutOfBounds = false;
			float num = textGenerator.GetPreferredHeight(base.text, generationSettings);
			if (num > base.textComponent.rectTransform.rect.height)
			{
				parentLayoutElement.preferredHeight = num;
			}
			else if (num < base.textComponent.rectTransform.rect.height)
			{
				parentLayoutElement.preferredHeight = num;
			}
		}
	}

	private void Update()
	{
		if (base.isFocused && lastCaretPosition != base.caretPosition)
		{
			lastCaretPosition = base.caretPosition;
			Vector2 size = base.textComponent.rectTransform.rect.size;
			TextGenerationSettings generationSettings = base.textComponent.GetGenerationSettings(size);
			generationSettings.generateOutOfBounds = false;
			float num = textGenerator.GetPreferredHeight(base.text, generationSettings);
			float num2 = num * 0.5f;
			float num3 = textGenerator.characters[base.caretPosition].cursorPos.y + num2;
			scrollRect.verticalNormalizedPosition = num3 / num;
		}
	}

	protected override void Start()
	{
		base.Start();
		textGenerator = new TextGenerator();
		if (base.transform.parent != null)
		{
			parentLayoutElement = base.transform.parent.GetComponent<LayoutElement>();
		}
		base.onValueChanged.AddListener(OnValueChange);
	}
}
