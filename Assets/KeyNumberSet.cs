class KeyNumberSet : InstructionSet
{
    int keyNumber;

    public KeyNumberSet(int keyNumber)
    {
        this.keyNumber = keyNumber;
    }

    public override string GetText()
    {
        return "Starting Key Number is " + keyNumber + ".";
    }
}