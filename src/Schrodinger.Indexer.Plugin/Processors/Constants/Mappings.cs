namespace Schrodinger.Indexer.Plugin.Processors.Constants;

public class Mappings
{
    public static Dictionary<string, string> TypeABCDEFGMap = new()
    {
        { "Background", "X" },
        { "Clothes", "Y" },
        { "Breed", "Z" },
        { "Face", "A" },
        { "Mustache", "A" },
        { "Necklace", "A" },
        { "Paw", "A" },
        { "Trousers", "A" },
        { "Belt", "A" },
        { "Shoes", "A" },
        { "Wings", "A" },
        { "Tail", "A" },
        { "Ride", "A" },
        { "Hat", "B" },
        { "Eyes", "B" },
        { "Pet", "B" },
        { "Weapon", "C" },
        { "Accessory", "C" },
        { "Mouth", "D" },
        { "Theme", "E" },
        { "Zodiac Signs", "F" },
        { "Quantum State", "G" }
    };

    public static Dictionary<string, string> TwoToNineTypeMap = new()
    {

        { "Weapon(Left Hand)", "Weapon" },
        { "Accessory(Right Hand)", "Accessory" },
        { "Ride (cars alike)", "Ride" },
        { "Wing", "Wings" },
        { "Moustauch", "Mustache" },
        { "Mustaches", "Mustache" },
    };
}