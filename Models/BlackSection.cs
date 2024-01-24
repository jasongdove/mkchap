namespace MkChap.Models;

public record BlackSection(TimeSpan Start, TimeSpan Finish, State State)
{
    public TimeSpan Duration => Finish - Start;
    public TimeSpan Midpoint() => Start + (Finish - Start) / 2.0;
};
