using PulsarModLoader;

namespace TalentTest
{
    public class Mod : PulsarMod
    {
        public override string Version => "0.0.1";

        public override string Author => "Mest";

        public override string Name => "TalentTest";

        public override string HarmonyIdentifier()
        {
            return $"{Author}.{Name}";
        }
    }
}
