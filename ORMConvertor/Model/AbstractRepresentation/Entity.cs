namespace Model.AbstractRepresentation;

public class Entity : Node
{
    public Entity()
    {
        Metadata["storage.paradigm"] = "relational";
    }
}
