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

            var empire = EmpireTheGame.FromString(map);
            
            Assert.Equal(3, empire.SizeX);
            Assert.Equal(3, empire.SizeY  );
            Assert.True(empire.Dump().Length > 0); //it prints something
            Assert.Equal(map, empire.Dump().TrimEnd());
        }
    }
}