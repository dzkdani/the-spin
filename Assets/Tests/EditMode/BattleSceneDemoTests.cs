using System.IO;
using NUnit.Framework;

public class BattleSceneDemoTests
{
    [Test]
    public void SceneWiresFiveWaveDemo()
    {
        string scene = File.ReadAllText(
            "Assets/Game/Scenes/BattleScene.unity");

        StringAssert.Contains("totalEnemies: 5", scene);
        StringAssert.Contains("a4b5c6d7e8f90123456789abcdef0234", scene);
        StringAssert.Contains("b5c6d7e8f90123456789abcdef023456", scene);
        StringAssert.Contains("c6d7e8f90123456789abcdef02345678", scene);
        StringAssert.Contains("d7e8f90123456789abcdef0234567890", scene);
        StringAssert.Contains("e8f90123456789abcdef023456789012", scene);
        StringAssert.Contains("f3a4b5c6d7e8f90123456789abcdef01", scene);
    }
}
