using System.Diagnostics;

namespace EmpireTests;

public class PlayerTests
{
    public PlayerTests()
    {
        string map = """
                         oo.
                         ..o
                         ..#
                         """;

        //var empire = EmpireTheGame.FromString(map, playerCount: 2);
        empire = new EmpireTheGame(map, playerCount: 1);
        player = empire.AddPlayer();
        

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
        player.AddUnit(new Army(0, 0, player));

        Debug.WriteLine($"expected = {expectedFoggyMapString}");

        bool observableHappened = false;
        string rez = "pero";
        //Assert.True(observableHappened);
        empire.Players.First().DumpObs.Subscribe(x =>
        {
            rez = x;
            Debug.WriteLine("Actual:"+x);
            observableHappened = true;           
        });
        
        Assert.True(observableHappened);
        Assert.Equal(expectedFoggyMapString, rez);
        

        
    }


}
