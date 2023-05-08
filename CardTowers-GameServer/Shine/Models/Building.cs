using System.Collections;
using System.Collections.Generic;

public abstract class Building
{
    public int Tier { get; protected set; }
    public int Damage { get; protected set; }
    public float AttackSpeed { get; protected set; }
}
