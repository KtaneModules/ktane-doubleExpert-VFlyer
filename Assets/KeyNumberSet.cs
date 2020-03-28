using rnd= UnityEngine.Random;


class KeyNumberSet : InstructionSet
{
    int keyNumber;
    bool niceMessage = rnd.Range(0, 2) == 0;
    public KeyNumberSet(int keyNumber)
    {
        this.keyNumber = keyNumber;
    }

    public override string GetText()
    {
        return (niceMessage ? "The module's Starting Key Number is " : "Starting Key Number is ") + keyNumber + ". Press NEXT.";
    }
}