using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

class Condition
{
    KMBombInfo bomb;
    QuirkInfo qi;

    public Condition(KMBombInfo bomb, QuirkInfo qi)
    {
        this.bomb = bomb;
        this.qi = qi;
    }

    public String GetText()
    {
        return "";
    }

    public bool CheckCondition()
    {
        return true;
    }
}