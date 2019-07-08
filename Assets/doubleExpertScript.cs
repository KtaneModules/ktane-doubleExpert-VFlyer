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

	public KMSelectable[] btns;
	public KMSelectable switchBtn;
	public GameObject switchObj;
	public TextMesh display;

	//Logging
	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

	int startTime;
    DayOfWeek day;

	QuirkInfo qi;
	int keyNumber;
	InstructionSet[] sets;
	List<char> appliedRules = new List<char>();

	Coroutine setDisplay;

	int currentInstructionSet = 0;
	int latestInstructionSet = 0;

	void Awake()
	{
        startTime = (int)(bomb.GetTime() / 60);
		day = DateTime.Now.DayOfWeek;

		moduleId = moduleIdCounter++;
		switchBtn.OnInteract += delegate () { FlipSwitch(); return false; };
		btns[0].OnInteract += delegate () { PrevSet(); return false; };
		btns[1].OnInteract += delegate () { NextSet(); return false; };
	}

	void FlipSwitch()
	{
       	switchBtn.AddInteractionPunch(.5f);
		switchObj.transform.Rotate(0, 180f, 0);
        Audio.PlaySoundAtTransform("switch", transform);

	}

	void PrevSet()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        btns[0].AddInteractionPunch(.5f);

		if(currentInstructionSet == 0)
			return;

		currentInstructionSet--;
		if(setDisplay != null) StopCoroutine(setDisplay);
		setDisplay = StartCoroutine(DisplaySet(currentInstructionSet));
	}

	void NextSet()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        btns[1].AddInteractionPunch(.5f);

		if(currentInstructionSet == sets.Length - 1)
			return;

		currentInstructionSet++;
		if(currentInstructionSet > latestInstructionSet) latestInstructionSet = currentInstructionSet;
		if(setDisplay != null) StopCoroutine(setDisplay);
		setDisplay = StartCoroutine(DisplaySet(currentInstructionSet));
	}

	void Start () 
	{
		CheckQuirks();
		GenerateInstructionSets();
		StartCoroutine(DisplaySet(currentInstructionSet));
	}

	void CheckQuirks()
	{
		qi = new QuirkInfo(moduleId, bomb, startTime);
	}
	
	void GenerateInstructionSets()
	{
		keyNumber = rnd.Next() % 40 + 30;

		sets = new InstructionSet[7];

        Debug.LogFormat("[Double Expert #{0}] ------------Instruction Sets------------", moduleId);

		sets[0] = new KeyNumberSet(keyNumber);

        Debug.LogFormat("[Double Expert #{0}] Instruction set 1 reads: \"{1}\"", moduleId, sets[0].GetText());

		for(int i = 1; i < sets.Length; i++)
		{
			sets[i] = new InstructionSet(bomb, qi);
			if(i == sets.Length - 1) sets[i].SetFinalText();
        	Debug.LogFormat("[Double Expert #{0}] Instruction set {1} reads: \"{2}\"", moduleId, i+1, sets[i].GetText());
		}
	}

	char GetRandomChar()
	{
		char[] chars = new char[] { '|', '\\', '!', '"', '@', '#', '£', '$', '§', '%','&', '/', '{', '(', '[', ')', ']', '=', '}', '?', '\'', '»', '«', '<', '>', '€', ',', ';', '.', ':', '-', '_', '*', '+'};
		return chars[rnd.Next() % chars.Length];
	}

	bool CheckRule(char rule)
	{
		appliedRules.Add(rule);

		switch(rule)
		{
			case 'A': return (bomb.GetSerialNumber().IndexOfAny(qi.vowels.ToArray()) != -1) || qi.mayNinth;
			case 'B': return bomb.GetBatteryCount() > 3 || qi.mayNinth;
			case 'C': return bomb.IsPortPresent(Port.Parallel) && qi.portCondition;
			case 'D': return bomb.GetPortPlateCount() > 2 || qi.portCondition;
			case 'E': return bomb.GetSerialNumber().IndexOfAny(new char[] {'2', '3', '5', '7'}) != -1;
			case 'F': return bomb.IsPortPresent(Port.StereoRCA) && qi.portCondition;
			case 'G': return qi.vowels.Contains(appliedRules.ElementAt(appliedRules.Count() - 2));
			case 'H': return keyNumber > 11 || qi.mayNinth;
			case 'I': return bomb.IsIndicatorPresent(Indicator.BOB) || qi.mayNinth;
			case 'J': return day == DayOfWeek.Wednesday;
			case 'K': return bomb.GetSolvedModuleNames().Count() < 6;
			case 'L': return keyNumber % 2 == qi.evenRemainder;
			case 'M': return !bomb.GetPortPlates().Any((x) => x.Length == 0);
			case 'N': return (bomb.GetSolvableModuleNames().Count() - bomb.GetSolvedModuleNames().Count()) < 5;
			case 'O': return bomb.GetSerialNumber().IndexOfAny("DOUBLE".ToArray()) != -1;
			case 'P': return bomb.IsPortPresent(Port.DVI) && qi.portCondition;
			case 'R': return GetUniqueDigits() > 2 || qi.mayNinth;
			case 'S': return GetGreatestPortCount() <= 1 && qi.portCondition;
			case 'T': return "PREVIOUS".ToList().Exists(x => x == appliedRules.ElementAt(appliedRules.Count() - 2)) || qi.mayNinth;
			case 'U': return (bomb.GetTime() / 60) % 2 == qi.evenRemainder;
			case 'V': return bomb.GetSolvedModuleNames().Count == 5;
			case 'W': return keyNumber == bomb.GetBatteryCount();
			case 'X': return keyNumber > 12;
			case 'Y': return keyNumber < 65 || qi.mayNinth;
			case 'Z': return keyNumber < 0;
		}

		appliedRules.Remove(rule);
        Debug.LogFormat("[Double Expert #{0}] No rule matches character {1}. Ignoring.", moduleId, rule);
		return false;
	}

	int GetUniqueDigits()
	{
		List<int> digits = new List<int>();
		IEnumerable<int> sn = bomb.GetSerialNumberNumbers();

		foreach(int digit in sn)
		{
			if( (digit % 2 == (qi.evenRemainder - 1) * -1) && !digits.Exists(x => x == digit) )
				digits.Add(digit);
		}

		return digits.Count();
	}

	int GetGreatestPortCount()
    {
        Port[] ports = {Port.DVI, Port.PS2, Port.Parallel, Port.RJ45, Port.Serial, Port.StereoRCA };
        int cnt = 0;
        for(int i = 0; i < ports.Length; i++)
        {
            if(bomb.GetPortCount(ports[i]) > cnt)
                cnt = bomb.GetPortCount(ports[i]);
        }

        return cnt;
    }

	IEnumerator DisplaySet(int set)
	{
		String instr = sets[set].GetText();

		int prob = latestInstructionSet - currentInstructionSet;
		
		display.text = "";

		String[] words = instr.Split(new char[] {' '});
		int charCnt = 0;

		for(int i = 0; i < words.Length; i++)
		{
			for(int j = 0; j < words[i].Length; j++)
			{
				if(rnd.Next() % 12 < prob)
				{
					char[] chars = words[i].ToCharArray();
					chars[j] = GetRandomChar();
					words[i] =  new string(chars);
				}
			}
		}

		for(int i = 0; i < words.Length; i++)
		{
			if(charCnt + words[i].Length > 21)
			{
				display.text += "\n";
				charCnt = 0;
			}
			else if(charCnt != 0)
			{
				display.text += ' ';
				charCnt++;
				yield return new WaitForSeconds(0.01f);
			}

			charCnt += words[i].Length;

			for(int j = 0; j < words[i].Length; j++)
			{
				display.text += words[i][j];
				yield return new WaitForSeconds(0.01f);
			}
		}

		yield return true;
	}
}
