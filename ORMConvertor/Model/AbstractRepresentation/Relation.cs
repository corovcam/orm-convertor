namespace Model.AbstractRepresentation;

public class Relation : Edge
{
    public Relation()
    {
        Metadata["storage.paradigm"] = "relational";
    }
}
