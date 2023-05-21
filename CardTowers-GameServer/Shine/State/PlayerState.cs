using System;
using System.Collections.Generic;
using CardTowers_GameServer.Shine.Models;
using CardTowers_GameServer.Shine.State.Actions;
using CardTowers_GameServer.Shine.State.Deltas;

namespace CardTowers_GameServer.Shine.State
{
    public class PlayerState : DeltaObjectBase<PlayerDelta>, IGameState<PlayerDelta, IDeltaAction<PlayerDelta>>
    {
        private Dictionary<Type, IDeltaAction<PlayerDelta>> deltaActions;

        public PlayerState()
        {
            deltaActions = new Dictionary<Type, IDeltaAction<PlayerDelta>>();
            RegisterDeltaAction<SpendManaAction>(new SpendManaAction(1));
            // Register other delta actions as needed.
        }

        protected override PlayerDelta CreateDelta()
        {
            // Logic to create a delta from the current and previous state goes here.
            // For now, return an empty PlayerDelta.
            return new PlayerDelta();
        }

        public void ApplyDeltaAction(IDeltaAction<PlayerDelta> deltaAction)
        {
            Type actionType = deltaAction.GetType();
            if (deltaActions.TryGetValue(actionType, out IDeltaAction<PlayerDelta> registeredAction))
            {
                PlayerDelta delta = GenerateDelta();
                deltaAction.Execute(this, delta);
                ApplyDelta(delta);
            }
            else
            {
                // No delta action registered for this type
                throw new InvalidOperationException($"No delta action registered for type {actionType.Name}");
            }
        }

        public IGameStateSnapshot<PlayerDelta> CreateSnapshot()
        {
            // Logic to create a snapshot goes here.
            // For now, return a snapshot that contains the current delta.
            return new PlayerStateSnapshot { PlayerDelta = currentDelta };
        }

        public void RestoreFromSnapshot(IGameStateSnapshot<PlayerDelta> snapshot)
        {
            // Logic to restore state from a snapshot goes here.
            ApplyDelta(snapshot.GetDelta());
        }

        private void RegisterDeltaAction<TAction>(TAction deltaAction) where TAction : IDeltaAction<PlayerDelta>
        {
            Type actionType = typeof(TAction);
            if (!deltaActions.ContainsKey(actionType))
            {
                deltaActions.Add(actionType, deltaAction);
            }
        }
    }
}
