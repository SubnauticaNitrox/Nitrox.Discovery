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
        errors = new();
        buildEngine.When(x => x.LogErrorEvent(Arg.Any<BuildErrorEventArgs>())).Do(info => errors.Add(info.Arg<BuildErrorEventArgs>()));
    }

    [TestMethod]
    public void TestExecute()
    {
        //Arrange
        DiscoverGame task = new()
        {
            GameName = "Subnautica",
            ExeName = "Subnautica",
            BuildEngine = buildEngine
        };

        //Act
        bool success = task.Execute();

        //Assert
        Assert.IsTrue(success);
        Assert.AreEqual(0, errors.Count);
        Assert.AreNotEqual("", task.GamePath);
        Console.WriteLine(task.GamePath);
    }
}