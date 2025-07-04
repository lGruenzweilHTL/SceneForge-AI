public abstract class SceneDiff
{
    public int? InstanceId { get; set; }
    public abstract int Priority { get; }
    public string TempId { get; set; } // TODO: implement temp id functionality: issue #19
}

public class CreateObjectDiff : SceneDiff
{
    public override int Priority => 1; // High priority for new objects
    public string Name { get; set; }

    public override string ToString()
    {
        return "CreateObjectDiff: " + Name;
    }
}
public class RemoveObjectDiff : SceneDiff
{
    public override int Priority => 1; // High priority for removed objects
    // only uses instance id
    
    public override string ToString()
    {
        return "RemoveObjectDiff: " + (InstanceId.HasValue ? InstanceId.ToString() : TempId);
    }
}
public class AddComponentDiff : SceneDiff, IComponentDiff
{
    public override int Priority => 2; // Medium priority for added components
    public string ComponentType { get; set; }
    
    public override string ToString()
    {
        return "AddComponentDiff: " + ComponentType;
    }
}
public class RemoveComponentDiff : SceneDiff, IComponentDiff
{
    public override int Priority => 2; // Medium priority for removed components
    public string ComponentType { get; set; }
    
    public override string ToString()
    {
        return "RemoveComponentDiff: " + ComponentType;
    }
}

public class UpdatePropertyDiff : SceneDiff, IComponentDiff
{
    public override int Priority => 3; // Low priority for property updates
    public string ComponentType { get; set; }
    public string PropertyName { get; set; }
    public object OldValue { get; set; }
    public object NewValue { get; set; }
    
    public override string ToString()
    {
        return $"UpdatePropertyDiff: {PropertyName} {OldValue}\u2192{NewValue}";
    }
}

public interface IComponentDiff
{
    string ComponentType { get; }
}