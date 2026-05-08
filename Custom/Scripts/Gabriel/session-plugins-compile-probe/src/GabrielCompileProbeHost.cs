using MeshVR;

namespace geesp0t
{
    /// <summary>
    /// Minimal host so Path A staged cslists always contain at least one
    /// <c>MVRScript</c>. Remove from Path B cumulative lists once production
    /// scripts include their own hosts (GabrielHud, GabrielSessionOrchestrator,
    /// DildoOnHands, …).
    /// </summary>
    public class GabrielCompileProbeHost : MVRScript
    {
        public override void Init()
        {
        }
    }
}
