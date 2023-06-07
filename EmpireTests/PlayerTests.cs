namespace EmpireTests;

public class PlayerTests
{
    public PlayerTests()
    {
        string map = """
                         ...
                         ..o
                         ..#
                         """;

        //var empire = EmpireTheGame.FromString(map, playerCount: 2);
        empire = new EmpireTheGame(map, playerCount: 1);
        player = new Player(empire);

    }

    private EmpireTheGame empire;
    private Player player;
    [Fact]
    public void EmpireCanLoadTextMap()
    {
        /*
        string map = """
                         o..
                         ..o
                         ..#
                         """;

        //var empire = EmpireTheGame.FromString(map, playerCount: 2);
        var empire = new EmpireTheGame (map, playerCount: 1);
        var player = new Player(empire);*/

        string expectedFoggyMapString = 
            "   \r\n"+
            "   \r\n"+
            "   \r\n";

        Assert.NotNull(player);
        Assert.Equal(expectedFoggyMapString, player.Dump());

    }
    [Fact]
    public void PlayerCanSeeAroundArmy()
    {
        string expectedFoggyMapString =
            "a. \r\n" +
            ".. \r\n" +
            "   \r\n";
        Player.Add(new Army(0, 0));

        bool observableHappened = false;
        empire.Players.First().DumpObs.Subscribe(x =>
        {
            observableHappened = true;
            Assert.Equal(expectedFoggyMapString, x);
        });
        Assert.True(observableHappened);

    }


}
