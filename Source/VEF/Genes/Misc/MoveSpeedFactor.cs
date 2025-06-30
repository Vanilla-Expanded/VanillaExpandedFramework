namespace VEF.Genes;

public class MoveSpeedFactor
{
    public float moveSpeedFactor;
    // If the tag is specified (not null/empty), only a single gene with that tag can apply the bonus.
    // Without the tag, there's no limit on amount of genes with speed factor bonus.
    // Generally, if using the tag try to make moveSpeedFactor identical across the board.
    public string tag = null;
}