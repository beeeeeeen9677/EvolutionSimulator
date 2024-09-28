using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CollisionLayer
{
    // LayerName = 1 << n  // n is layer number

    // Layer is in bitwise
    // << is left shift operator (for bit)
    Player = 1 << 6,
    TestCollectable = 1 << 7,
}