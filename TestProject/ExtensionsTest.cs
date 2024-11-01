using Nitrox.Discovery.Extensions;

namespace TestProject;

[TestClass]
public class ExtensionsTest
{
    [TestMethod]
    public void GetUniqueNonCombinatoryFlags_ShouldReturnUniqueNonCombinatoryFlags()
    {
        TestEnumFlags.ALL.GetUniqueNonCombinatoryFlags().Should().BeEquivalentTo([TestEnumFlags.A, TestEnumFlags.B, TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E, TestEnumFlags.F]);
        TestEnumFlags.CDEF.GetUniqueNonCombinatoryFlags().Should().BeEquivalentTo([TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E, TestEnumFlags.F]);
        TestEnumFlags.E.GetUniqueNonCombinatoryFlags().Should().BeEquivalentTo([TestEnumFlags.E]);
        TestEnumFlags.NONE.GetUniqueNonCombinatoryFlags().Should().BeEmpty();
    }

    [TestMethod]
    public void GetUniqueNonCombinatorFlags_ShouldReturnAllUniquesWhenAllBitsSet()
    {
        ((TestEnumFlags)int.MaxValue).GetUniqueNonCombinatoryFlags().Should().BeEquivalentTo([TestEnumFlags.A, TestEnumFlags.B, TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E, TestEnumFlags.F]);
    }

    [TestMethod]
    public void InvalidFileName()
    {
        "Subnautica: Below Zero".ReplaceCommonInvalidFileNameChars().Should().Be("Subnautica Below Zero");
        "Subnautica: Below Zero".ReplaceCommonInvalidFileNameChars("_").Should().Be("Subnautica_ Below Zero");
    }

    [Flags]
    private enum TestEnumFlags
    {
        NONE = 0,
        F = 1 << 5,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,
        D = 1 << 3,
        DD = D,
        E = 1 << 4,
        AB = A | B,
        CD = C | D,
        CDEF = CD | E | F,
        ALL = AB | CDEF
    }
}