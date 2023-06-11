﻿using System.Diagnostics;

namespace EmpireTests;

public class PlayerTests
{
    private EmpireTheGame empire;
    private Player player;
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
            "   \r\n" +
            "   \r\n" +
            "   \r\n";

        Assert.NotNull(player);
        Assert.Equal(expectedFoggyMapString, player.Dump());

    }
    [Fact]
    public void PlayerCanSeeAroundArmy()
    {
        player.AddUnit(new Army(0, 0, player));

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

}

public class PlayerTestsMultipleArmies
{
    private EmpireTheGame empire;
    private Player player;
    public PlayerTestsMultipleArmies()
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

    [Fact]
    public void PlayersArmiesCanSeeEachother()
    {
        player.AddUnit(new Army(0, 0, player));
        player.AddUnit(new Army(1, 0, player));



        string expectedFoggyMapString =
            "aa.\r\n" +
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
    public void ArmyCantStepIntoFriendlyArmy()
    {
        var army1 = new Army(0, 0, player);
        var army2 = new Army(1, 0, player);
        player.AddUnit(army1);
        player.AddUnit(army2);

        empire.MoveTo(1, 0, army1);


        string expectedFoggyMapString =
            "aa.\r\n" +
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



}


public class PlayerTestsStandingOrderGivesFeedback
{
    private EmpireTheGame empire;
    private Player player;
    public PlayerTestsStandingOrderGivesFeedback()
    {
        string map = """
                        oooooooooooooooooooooooo
                        ..oooooooooooooooooooooo
                        .......................#
                        """;
        empire = new EmpireTheGame(map, playerCount: 1);
        player = empire.AddPlayer();
    }

    [Fact]
    public void PlayersArmiesHandleStandingOrderAndFeedback()
    {
        var army = new Army(0, 0, player);
        player.AddUnit(army);

        //a wish
        //army.standingOrder = StandingOrders.LongGoto;
        //army.TargetX = 8;
        //army.TargetY = 0;

        //player .AddStandingOrder(new MoveOrder(army, 1, 0));

        /*
        string expectedFoggyMapString =
            "aa.\r\n" +
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
        */
    }







}


public class PlayerTestsCities
{
    private EmpireTheGame empire;
    private Player player;
    public PlayerTestsCities()
    {
        string map = """
                         o#oo
                         ..o.
                         ..#o
                         """;
        //var empire = EmpireTheGame.FromString(map, playerCount: 2);
        empire = new EmpireTheGame(map, playerCount: 1);
        player = empire.AddPlayer();
    }

    [Fact]
    public void PlayersArmiesConquerCitiesWin()
    {
        var army = new Army(0, 0, player);
        player.AddUnit(army);
        Army.rnd = new RandomForTesting(new double[] { 1 });
        empire.MoveTo(1, 0, army);



        string expectedFoggyMapString = """
                         o1o 
                         ..o 
                             
                         """ + "\r\n";

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

        
        var army2 = new Army(2, 1, player);
        player.AddUnit(army2);
        empire.MoveTo(1, 0, army2);

        Assert.Equal((1, 0), (army2.X, army2.Y)); //while in a city, army should have correct loc


    }


    [Fact]
    public void PlayersArmiesSeesTransport()
    {
        var army = new Army(0, 0, player);
        player.AddUnit(army);
        var trans = new Transport(0,1,player);
        player.AddUnit(trans);

        Army.rnd = new RandomForTesting(new double[] { 1 });
        empire.MoveTo(1, 0, army);



        string expectedFoggyMapString = """
                         o1o 
                         t.o 
                         ..  
                         """ + "\r\n";

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

        // move army into transport
        var army2 = new Army(0, 0, player);
        player.AddUnit(army2);
        empire.MoveTo(0, 1, army2);

        Assert.True(army2.IsContained);
        Assert.Equal((0, 1), (army2.X, army2.Y)); //while in a transport, army should have correct loc
        Assert.Equal(expectedFoggyMapString, empire.Players.First().Dump());


    }



}


