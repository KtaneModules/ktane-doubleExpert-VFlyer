using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class doubleExpertScript : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio Audio;

    static System.Random rnd = new System.Random();

	//Logging
	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

	int startTime;
	QuirkInfo qi;
	int keyNumber;
	InstructionSet[] sets;

	void Awake()
	{
        startTime = (int)(bomb.GetTime() / 60);

		moduleId = moduleIdCounter++;
		//btn.OnInteract += delegate () { PressButton(); return false; };
	}

	void Start () 
	{
		CheckQuirks();
		GenerateInstructionSets();
	}

	void CheckQuirks()
	{
		qi = new QuirkInfo(moduleId, bomb, startTime);
	}
	
	void GenerateInstructionSets()
	{
		keyNumber = rnd.Next() % 40 + 30;

		sets = new InstructionSet[9];

        Debug.LogFormat("[Double Expert #{0}] ------------Instruction Sets------------", moduleId);

		sets[0] = new KeyNumberSet(keyNumber);

        Debug.LogFormat("[Double Expert #{0}] Instruction set 1 reads: \"{1}\"", moduleId, sets[0].GetText());

		for(int i = 1; i < sets.Length; i++)
		{
			sets[i] = new InstructionSet(bomb, qi);
        	Debug.LogFormat("[Double Expert #{0}] Instruction set {1} reads: \"{2}\"", moduleId, i+1, sets[i].GetText());
		}
	}
}
