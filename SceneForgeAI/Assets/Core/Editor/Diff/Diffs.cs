public abstract class SceneDiff
{
    public int InstanceId { get; set; }
    public abstract int Priority { get; }
}

public class CreateObjectDiff : SceneDiff
{
    public override int Priority => 1; // High priority for new objects
    public string Name { get; set; }
}
public class RemoveObjectDiff : SceneDiff
{
    public override int Priority => 1; // High priority for removed objects
    // only uses instance id
}
public class AddComponentDiff : SceneDiff
{
    public override int Priority => 2; // Medium priority for added components
    public string ComponentType { get; set; }
}
public class RemoveComponentDiff : SceneDiff
{
    public override int Priority => 2; // Medium priority for removed components
    public string ComponentType { get; set; }
}
public class UpdatePropertyDiff : SceneDiff
{
    public override int Priority => 3; // Low priority for property updates
    public string ComponentType { get; set; }
    public string PropertyName { get; set; }
    public object OldValue { get; set; }
    public object NewValue { get; set; }
}