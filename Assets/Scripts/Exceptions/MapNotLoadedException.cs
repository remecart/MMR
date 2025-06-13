using System;

public class MapNotLoadedException : Exception
{
    public MapNotLoadedException()
        : base("The map has not been loaded yet. Please load a map before accessing it.")
    {
    }

    public MapNotLoadedException(string message)
        : base(message)
    {
    }

    public MapNotLoadedException(string message, Exception inner)
        : base(message, inner)
    {
    }
}