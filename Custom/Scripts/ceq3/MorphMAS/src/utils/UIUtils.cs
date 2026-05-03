using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.UIUtils
{
    public class UIUtils
    {
        private static Font _font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        private static Sprite _buttonBackgroundSprite = SuperController.singleton.dynamicButtonPrefab.GetComponentInChildren<Image>().sprite;

        static public void RemoveUI(MVRScript script, List<UIDynamic> ui)
        {
            if (ui == null) return;

            foreach (UIDynamic uiElement in ui)
            {
                if (uiElement is UIDynamicButton)
                {
                    script.RemoveButton((UIDynamicButton)uiElement);
                }
                else if (uiElement is UIDynamicColorPicker)
                {
                    script.RemoveColorPicker((UIDynamicColorPicker)uiElement);
                }
                else if (uiElement is UIDynamicPopup)
                {
                    script.RemovePopup((UIDynamicPopup)uiElement);
                }
                else if (uiElement is UIDynamicSlider)
                {
                    script.RemoveSlider((UIDynamicSlider)uiElement);
                }
                else if (uiElement is UIDynamicTextField)
                {
                    script.RemoveTextField((UIDynamicTextField)uiElement);
                }
                else if (uiElement is UIDynamicToggle)
                {
                    script.RemoveToggle((UIDynamicToggle)uiElement);
                }
                else
                {
                    script.RemoveSpacer(uiElement);
                }

            }
        }

        static public UIDynamic CreateSpacer(MVRScript script, float size = 1, bool rightSide = false)
        {
            UIDynamic spacer = script.CreateSpacer(rightSide);
            spacer.height = size;
            return spacer;
        }

        public static UIDynamic CreateHeader(MVRScript script, string text, bool rightSide = false, int size = 20, Color? color = null, float paddingBottom = 0.33f)
        {
            UIDynamic header = UIUtils.CreateSpacer(script, size * (1 + paddingBottom), rightSide);
            Text textComponent = header.gameObject.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = _font;
            textComponent.fontSize = size;
            textComponent.fontStyle = FontStyle.Bold;
            if (color != null) textComponent.color = (Color)color;
            return header;
        }

        public static UIDynamicTextField CreateTextInput(MVRScript script, JSONStorableString storableString, bool rightSide = false)
        {
            UIDynamicTextField textField = script.CreateTextField(storableString, rightSide);
            textField.GetComponentInChildren<ScrollRect>().vertical = false;

            InputField inputField = textField.gameObject.AddComponent<InputField>();
            inputField.textComponent = textField.UItext;
            inputField.text = storableString.val;
            inputField.onValueChanged.AddListener((string newValue) => storableString.val = newValue);

            textField.GetComponentInChildren<Image>().sprite = _buttonBackgroundSprite;
            textField.height = 28f;
            textField.backgroundColor = Color.white;

            return textField;
        }

        public static void PromptFile(bool savePrompt, string defaultLocation, string fileExtension, uFileBrowser.FileBrowserCallback callback)
        {
            if (savePrompt)
            {
                SuperController.singleton.GetMediaPathDialog(callback, fileExtension, defaultLocation, false, true, false);
                uFileBrowser.FileBrowser browser = SuperController.singleton.mediaFileBrowserUI;
                browser.SetTextEntry(true);
                browser.ActivateFileNameField();
            }
            else
            {
                SuperController.singleton.GetMediaPathDialog(callback, fileExtension, defaultLocation, false, true, false, null, false);
            }
        }
    }
}
