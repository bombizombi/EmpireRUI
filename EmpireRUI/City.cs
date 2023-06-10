namespace EmpireRUI;

public class City
{
    public int x;
    public int y;
    public ProductionEnum production;

    public int remaining;

    public City(int inx, int iny)
    {
        x = inx;
        y = iny;

        //production = ProductionEnum.army; //
        //production = ProductionEnum.transport; //
        production = ProductionEnum.uninitialized; //

        //remaining = freshProduction[(int)production];
        remaining = -1;
    }

    public void ResetProduction()
    {
        production = ProductionEnum.uninitialized; //
        remaining = -1;
    }


    public IUnit NewTurn(Player player)
    {

        remaining--;
        if (remaining == 0)
        {
            //create diff units from "production" var
            //Army a = new Army(x, y, player);
            IUnit a = CreateIUnit(production, player);
            player.AddUnit(a);

            //reset production
            remaining = continuingProduction[(int)production];

            return a;
        }
        return null;
    }

    public static int[] freshProduction = new int[] { 6, 12, 24, 30, 24, 36, 48, 60 };
    public static int[] continuingProduction = new int[] { 5, 10, 20, 25, 20, 30, 40, 50 };


    //reuse city as an Unit factory
    public IUnit CreateIUnit(ProductionEnum prod, Player player)
    {
        IUnit? unit = null;
        switch (prod)
        {
            case ProductionEnum.army:
                unit = new Army(x, y, player);
                break;
                /*
            case ProductionEnum.transport:
                unit = new Transport(x, y, player);
                break;
            case ProductionEnum.figher:
                unit = new Fighter(x, y, player);
                break;
                */

            default:
                Debugger.Break();
                break;

        }
        return unit;
    }

    public void SetProduction(int newProd)
    {
        if (newProd != (int)production)
        {
            production = (ProductionEnum)newProd;
            remaining = freshProduction[(int)production];

        }
    }



}
