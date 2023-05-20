using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State;
using CardTowers_GameServer.Shine.Util;

public class PlayerState : IGameState
{
    public Player Player { get; private set; }
    public GameMap Map { get; private set; }
    public int Elixir { get; set; }

    public PlayerState(Player player, GameMap map)
    {
        Player = player;
        Map = map;
        Elixir = Constants.INITIAL_ELIXIR;
    }


    public void AddElixir(int amount)
    {
        Elixir += amount;
        if (Elixir > Constants.MAX_ELIXIR)
        {
            Elixir = Constants.MAX_ELIXIR;
        }
    }


    public void SpendElixir(int amount)
    {
        Elixir -= amount;
        if (Elixir < 0)
        {
            Elixir = 0;
        }
    }


    // To be implemented
    public void ApplyDeltaState(DeltaState deltaState)
    {
        throw new NotImplementedException();
    }

    // To be implemented
    public DeltaState GetDeltaState(IGameState oldState)
    {
        throw new NotImplementedException();
    }
}


