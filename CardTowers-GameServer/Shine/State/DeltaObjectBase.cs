using System;
using System.Collections.Generic;

namespace CardTowers_GameServer.Shine.State
{
    public abstract class DeltaObjectBase<TDelta> : IDeltaObject<TDelta> where TDelta : Delta
    {
        private readonly Dictionary<Type, IDeltaAction<TDelta>> deltaActions;

        protected TDelta currentDelta;

        protected DeltaObjectBase()
        {
            deltaActions = new Dictionary<Type, IDeltaAction<TDelta>>();
        }

        public TDelta GenerateDelta()
        {
            TDelta delta = CreateDelta();
            ApplyDelta(delta);
            return delta;
        }

        public void ApplyDelta(TDelta delta)
        {
            currentDelta = delta;
            OnDeltaApplied(delta);
        }

        protected abstract TDelta CreateDelta();

        protected virtual void OnDeltaApplied(TDelta delta)
        {
            // Additional logic to be implemented by derived classes
        }

        protected void RegisterDeltaAction<TAction>(IDeltaAction<TDelta> deltaAction) where TAction : IDeltaAction<TDelta>
        {
            Type actionType = typeof(TAction);
            if (!deltaActions.ContainsKey(actionType))
            {
                deltaActions.Add(actionType, deltaAction);
            }
        }

        public void ApplyDeltaAction<TAction>(TAction deltaAction) where TAction : IDeltaAction<TDelta>
        {
            Type actionType = typeof(TAction);
            if (deltaActions.TryGetValue(actionType, out IDeltaAction<TDelta> registeredAction))
            {
                TDelta delta = CreateDelta();
                registeredAction.Execute(this, delta);
                ApplyDelta(delta);
            }
            else
            {
                // No delta action registered for this type
                throw new InvalidOperationException($"No delta action registered for type {actionType.Name}");
            }
        }

    }
}
