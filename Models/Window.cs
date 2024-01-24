namespace MkChap.Models;

public record Window(TimeSpan Start, TimeSpan Finish)
{
    public bool Contains(TimeSpan time) => time >= Start && time <= Finish;
}