namespace EmpireTests;

public class EmpireTheGameTests
{
    [Fact]
    public void EmpireCanLoadTextMap()
    {
        string map = """
                         ...
                         ..o
                         ..#
                         """;

        var empire = EmpireTheGame.FromString(map);
        
        Assert.Equal(3, empire.SizeX);
        Assert.Equal(3, empire.SizeY);
        Assert.True(empire.Dump().Length > 0); //it prints something
        Assert.Equal(map, empire.Dump().TrimEnd());
    }
}

public class MoveTests
{
    private EmpireTheGame empire;
    private Player player;
    public MoveTests()
    {
        string map = """
                         oo.
                         ..o
                         ..#
                         """;
        empire = new EmpireTheGame(map, playerCount: 1);
        player = empire.AddPlayer();
    }


    [Fact]
    public void ArmyMovingRevealsNewTerrain()
    {
        player.AddUnit(new Army(0, 0, player));

        empire.DebugMoveRight();

        string expectedFoggyMapString =
            "oa.\r\n" +
            "..o\r\n" +
            "   \r\n";

        bool observableHappened = false;
        string result = "";
        int count = 0;
        empire.Players.First().DumpObs.Subscribe(x =>
        {
            count++;
            result = x;
            observableHappened = true;
        });

        Assert.True(observableHappened);
        Assert.Equal(expectedFoggyMapString, result);
        Assert.Equal(1, count);

    }

    [Fact]
    public void ArmyCantMoveIntoWater()
    {
        player.AddUnit(new Army(0, 0, player));

        empire.DebugMoveInDirection( dy:1);

        string expectedFoggyMapString =
            "ao \r\n" +
            ".. \r\n" +
            "   \r\n";

        bool observableHappened = false;
        string result = "";
        int count = 0;
        empire.Players.First().DumpObs.Subscribe(x =>
        {
            count++;
            result = x;
            observableHappened = true;
        });

        Assert.True(observableHappened);
        Assert.Equal(expectedFoggyMapString, result);
        Assert.Equal(1, count);

    }

    [Fact]
    public void ArmyCantMoveOutOfMap()
    {
        var a = new Army(0, 0, player);
        player.AddUnit(a);

        bool done = empire.DebugMoveInDirection(dy: -1);

        Assert.False(done);
        Assert.Equal(0, a.X);
        Assert.Equal(0, a.Y);
    }





}