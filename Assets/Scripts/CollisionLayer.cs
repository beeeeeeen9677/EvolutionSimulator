using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CollisionLayer
{
    // LayerName = 1 << n  // n is layer number

    // Layer is in bitwise
    // << is left shift operator (for bit)

    // for test ---
    Player = 1 << 6,
    TestCollectable = 1 << 7,
    // ------------



    Animal = 1 << 8,
    Grass = 1 << 9,
    SelectionLayer = 1 << 10, // for mouse raycast select unit
    Ground = 1 << 11,

    Sensor = 1<< 12, // for sensor collision

    Lake = 1 << 13,
}


public static class TargetCollisionLayers
{
    public static CollisionLayer[] targetLayers = { CollisionLayer.Grass, CollisionLayer.Animal }; // collide with corresponding layer acoording to sensorNumber
}
