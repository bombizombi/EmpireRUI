namespace EmpireTests
{
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

            EmpireRUI.EmpireTheGame empire = EmpireTheGame.FromString(map);
            int x = empire.SizeX;
            int y = empire.SizeY;
            Assert.Equal(3, empire.SizeX);
            Assert.Equal(3, empire.SizeY  );
            Assert.True(empire.Dump().Length > 0); //it prints something
            Assert.Equal(map, empire.Dump());
        }
    }
}