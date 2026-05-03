using System.Collections.Generic;

namespace Utils.MorphsPreset
{
    public class MorphsPreset
    {
        Dictionary<string, float> _morphsValue;

        public MorphsPreset(DAZCharacterSelector characterSelector)
        {
            _morphsValue = new Dictionary<string, float>();

            SaveBankValues(characterSelector.morphBank1);
            SaveBankValues(characterSelector.morphBank2);
            SaveBankValues(characterSelector.morphBank3);
        }

        private void SaveBankValues(DAZMorphBank morphBank)
        {
            if (morphBank == null) return;
            foreach (DAZMorph morph in morphBank.morphs)
            {
                if (morph.active && morph.visible && morph.appliedValue != morph.jsonFloat.defaultVal)
                {
                    _morphsValue[morph.uid] = morph.morphValue;
                }
            }
        }

        public void Load(DAZCharacterSelector characterSelector)
        {
            LoadBankValues(characterSelector.morphBank1);
            LoadBankValues(characterSelector.morphBank2);
            LoadBankValues(characterSelector.morphBank3);
        }

        private void LoadBankValues(DAZMorphBank morphBank)
        {
            if (morphBank == null) return;
            foreach (DAZMorph morph in morphBank.morphs)
            {
                if (_morphsValue.ContainsKey(morph.uid))
                {
                    morph.morphValue = _morphsValue[morph.uid];
                }
                else
                {
                    morph.SetDefaultValue();
                }
            }
        }
    }
}
