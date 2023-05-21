namespace CardTowers_GameServer.Shine.State
{
    public interface IGameStateSnapshot<TDelta> where TDelta : Delta
    {
        TDelta GetDelta();
    }
}