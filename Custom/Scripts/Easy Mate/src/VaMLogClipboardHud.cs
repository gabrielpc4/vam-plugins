using System;

namespace geesp0t
{
    /// <summary>
    /// Log copy/clear UI lives on <see cref="MainUIButtons"/>. This plugin is a
    /// no-op so older saves or session lists that still reference
    /// VaMLogClipboardHud.cslist continue to load without a second HUD.
    /// </summary>
    public class VaMLogClipboardHud : MVRScript
    {
        public override void Init()
        {
        }
    }
}
