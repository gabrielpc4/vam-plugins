using System;
using UnityEngine;

namespace octopussy
{
    public class Sine : AtomFloatSelector
    {
        protected JSONStorableFloat frequencyJSON;
        protected JSONStorableFloat baseValueJSON;
        protected JSONStorableFloat amplitudeJSON;

        float time;

        protected override void SyncTarget()
        {
        }

        public override void Init()
        {
            try
            {
                base.Init();

                baseValueJSON = new JSONStorableFloat("base value", 0f, 0f, 1f, false);
                frequencyJSON = new JSONStorableFloat("frequency", 0f, 0f, 1f, false);
                amplitudeJSON = new JSONStorableFloat("amplitude", 0f, 0f, 1f, false);

                RegisterFloat(frequencyJSON);
                RegisterFloat(baseValueJSON);
                RegisterFloat(amplitudeJSON);

                /*CreateButton("Play").button.onClick.AddListener(() => Play() );
                JSONStorableAction actionJSON = new JSONStorableAction("Play", Play);
                RegisterAction(actionJSON);*/

                CreateSlider(baseValueJSON, true);
                CreateSlider(frequencyJSON, true);
                CreateSlider(amplitudeJSON, true);

                time = 0;
            }
            catch (Exception e)
            {
                SuperController.LogError(e.ToString());
            }
        }

        public void Update() {
            if (receiverTarget != null)
            {
                time += Time.deltaTime;
                receiverTarget.val =
                    baseValueJSON.val + Mathf.Sin(time * frequencyJSON.val) * amplitudeJSON.val;
            }
        }
    }
}
