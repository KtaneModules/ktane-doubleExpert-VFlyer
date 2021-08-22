using rnd= UnityEngine.Random;


class KeyNumberSet : InstructionSet
{
    int keyNumber;
    bool niceMessage = rnd.Range(0, 2) == 0;
    int idxRandomFileEnd = rnd.Range(0, 5);
    public KeyNumberSet(int keyNumber)
    {
        this.keyNumber = keyNumber;
    }

    public override string GetText()
    {
        return string.Format("{1}{0}. You are using the latest firmware of Double Expert{2}. Press NEXT.", keyNumber, niceMessage ? "The module's Starting Key Number is " : "Starting Key Number is ",new[] { ".exe", ".dat", ".app", ".cs", ".unity" }[idxRandomFileEnd]);
    }
}