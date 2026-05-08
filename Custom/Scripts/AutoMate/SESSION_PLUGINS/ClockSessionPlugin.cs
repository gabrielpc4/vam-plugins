using System;

namespace Blazedust
{
    //mod by geesp0t to also show version number
    public class ClockSessionPlugin : MVRScript
    {
		// https://docs.microsoft.com/en-us/dotnet/api/system.datetime.tostring?view=netframework-4.8 for different formats.
		// T = ToLongTimeString
		// t = ToShortTimeString
		// HH:mm:ss = 24:59:59
		const string TIME_FORMAT = "HH:mm:ss";

        public string versionNum = "";
		
		int counter = 0;
        public override void Init()
        {
        }

        void Start()
        {
            SuperController.singleton.SyncVersionText();
            versionNum = SuperController.singleton.versionText.text.Substring(9);
        }
		
		void Update()
		{			
			if ((counter++ % 10 == 0) && SuperController.singleton.versionText != null) 
			{
                SuperController.singleton.versionText.text = "v" + versionNum + "   " + string.Format(DateTime.Now.ToString(TIME_FORMAT));
			}
		}

        void OnDestroy()
        {
			SuperController.singleton.SyncVersionText();
        }

    }
}