using Microsoft.Build.Framework;
using Nitrox.Discovery.MSBuild;
using NSubstitute;

namespace TestProject;

[TestClass]
public class DiscoverGameTest
{
    private IBuildEngine buildEngine = null!;
    private List<BuildErrorEventArgs> errors = null!;

    [TestInitialize]
    public void Startup()
    {
        buildEngine = Substitute.For<IBuildEngine>();
        errors = new List<BuildErrorEventArgs>();
        buildEngine.When(x => x.LogErrorEvent(Arg.Any<BuildErrorEventArgs>())).Do(info => errors.Add(info.Arg<BuildErrorEventArgs>()));
    }

    [TestMethod]
    public void TestExecute()
    {
        //Arrange
        var task = new DiscoverGame
        {
            GameName = "Subnautica",
            BuildEngine = buildEngine
        };

        //Act
        var success = task.Execute();

        //Assert
        Assert.IsTrue(success);
        Assert.AreEqual(0, errors.Count);
        Console.WriteLine(task.GamePath);
    }
}
