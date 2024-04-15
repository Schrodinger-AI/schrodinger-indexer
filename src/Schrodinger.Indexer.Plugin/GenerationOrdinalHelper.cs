namespace Schrodinger.Indexer.Plugin;

public class GenerationOrdinalHelper
{
    public static string ConvertToOrdinal(int number)
    {
        return number switch
        {
            0 => "Zeroth",
            1 => "First",
            2 => "Second",
            3 => "Third",
            4 => "Fourth",
            5 => "Fifth",
            6 => "Sixth",
            7 => "Seventh",
            8 => "Eighth",
            9 => "Ninth",
            _ => number.ToString()
        } + "Generation";
    }
}