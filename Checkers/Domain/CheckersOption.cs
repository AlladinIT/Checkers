namespace Domain;

public class CheckersOption
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
    
    public int Width { get; set; } = 8;
    public int Height { get; set; } = 8;
    public int RowsOfPieces { get; set; } = 3;
    public bool BlackStarts { get; set; } = true;
    public bool CapturingIsMandatory { get; set; } = true;
    public bool FlyingKings { get; set; } = true;
    public bool PiecesCapturingForwardAndBackward { get; set; } = true;



    public ICollection<CheckersGame>? CheckersGames { get; set; }
    
    public override string ToString()
    {
        return
            $"{Width}x{Height} Rows of pieces: {RowsOfPieces}, White Start: {BlackStarts}," +
            $" Capturing is mandatory: {CapturingIsMandatory}, Flying Kings: {FlyingKings}," +
            $" Pieces can move forward and backward: {PiecesCapturingForwardAndBackward}";
    }
    

}