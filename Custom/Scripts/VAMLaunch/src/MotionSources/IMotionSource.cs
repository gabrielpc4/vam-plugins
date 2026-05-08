namespace VAMLaunchPlugin.MotionSources
{
    public interface IMotionSource
    {
        void OnInit(VAMLaunch plugin, string desiredTargetPrefix);
        void OnInitStorables(VAMLaunch plugin);
        bool OnUpdate(ref byte outPos, ref byte outSpeed);
        void OnSimulatorUpdate(float prevPos, float newPos, float deltaTime);
        void OnDestroy(VAMLaunch plugin);
        void SetInvert(bool newInvert);
    }
}