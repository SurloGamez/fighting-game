using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC
{
    private float health;

    public NPC(float health)
    {
        this.health = health;
    }

    public float Health
    {
        get { return health; } set {  health = value; }
    }

}

public class Chicken : NPC 
{
    float hunger;

    public Chicken(float health, float hunger) : base(health)
    {
        this.hunger = hunger;
    }

}

