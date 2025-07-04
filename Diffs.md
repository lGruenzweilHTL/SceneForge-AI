# Structuring of Scene diffs

## Types
There are multiple types of Scene diff:
- SpawnObject
- DeleteObject
- AddComponent
- RemoveComponent
- UpdateProperty

## Serializability
Scene diffs must be **serializable** to JSON. If that is not possible, a method will be used to convert it to JSON.

## Properties
Different types of Scene diff require different properties. For easy integration with the AI, we will use the Instance-ID to represent Game Objects.

### SpawnObject
The SpawnObject only requires a few things:
- Parent Instance-ID (0 if root)
- Name
- Transform data (position, rotation, scale)

### DeleteObject
Only requires the **Instance-ID**

### AddComponent
- Object Instance-ID
- Component type

### RemoveComponent
- Object Instance-ID
- Component type
- **LATER**: maybe index for cases with multiple of the same component

### UpdateProperty
- Object Instance-ID
- Component (we use the type for now)
- New value (**Bring back Deserializers script**)

## Different implementations

There are 2 viable ways to implement this:

### Inheritance
Since every type of diff requires some form of Instance-ID (object itself or parent), we create a base class containing only this.
All other types of diffs are subclasses of this base class. The type of diff can be found using **reflection**.

#### Pros
- Easily serialized (no null value shenanigans)
- Easier to return in code (no need to look at the docs for which properties are needed)

#### Cons
- Reflection could be slow

### Enum-based
We create an enum for each type of diff type and have all required properties in the same class.

#### Pros
- Same as the old system, so easy to recover
- Faster than reflection

#### Cons
- Ugly code (users will need to look at the docs to know which properties are needed)
- Harder to ensure correct serialization (honestly not that difficult but still annoying)