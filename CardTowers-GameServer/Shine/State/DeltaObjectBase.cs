using System;

namespace CardTowers_GameServer.Shine.State
{
    public abstract class DeltaObjectBase<TDelta> : IDeltaObject<TDelta> where TDelta : Delta
    {
        protected TDelta currentDelta;

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
        protected virtual void OnDeltaApplied(TDelta delta) { }
    }
}

