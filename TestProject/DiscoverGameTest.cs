using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Nitrox.Discovery.Models;
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

    [NonCiTestMethod]
    public void TestExecute()
    {
        // TODO: Allow this to run on CI systems.

        //Arrange
        DiscoverGame task = new()
        {
            GameName = "Subnautica",
            IncludeLibraries = [new TaskItem(nameof(GameLibraries.ALL))],
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