# Planning of SceneForge AI

## Prompt Structure

Prompts are structured in the following way:

```plaintext
User Instruction:
...

Scene Objects:
[
    {
        "uid": "unique_id",
        "name": "object_name",
        "path": "path in scene",
        "components": [
            {
                "type": "component_type",
                "properties": {
                    "property_name": "property_value"
                }
            },
            ...
        ]
    },
    ...
]

Additional Data (optional):
{
    "sprites": [
        {
            "name": "sprite_name",
            "path": "path in project"
        },
        ...
    ],
    "textures": [
        {
            "name": "texture_name",
            "path": "path in project"
        },
        ...
    ],
    "materials": [
        {
            "name": "material_name",
            "path": "path in project"
        },
        ...
    ],
    ...
}
```

### Additional Information

Prompts may include additional information important to the task.

- User Instruction (always): A description of the task to be performed.
- Scene Objects (always): A list of objects in the scene, each with a unique identifier, name, path, and components.
- Sprites (optional): A list of sprites in the Project folder, each with a name and path.
- Textures (optional): A list of textures in the Project folder, each with a name and path.
- Materials (optional): A list of materials in the Project folder, each with a name and path.
- Shaders (optional): A list of shaders in the Project folder, each with a name and path.
- Animations (optional): A list of animations in the Project folder, each with a name and path.
- Sounds (optional): A list of sounds in the Project folder, each with a name and path.
- Scripts (optional): A list of scripts in the Project folder, each with a name and path.
- Fonts (optional): A list of fonts in the Project folder, each with a name and path.


### Serialization of Unity Types

- Color: Serialized as an array of four floats, representing RGBA values. For example, a red color would be represented as `[1, 0, 0, 1]`.
- Vector2: Serialized as an array of two floats, representing X and Y coordinates. For example, a vector pointing to the right would be represented as `[1, 0]`.
- Vector3: Serialized as an array of three floats, representing X, Y, and Z coordinates. For example, a vector pointing to the right would be represented as `[1, 0, 0]`.
- Quaternion: Serialized using the euler angles format, represented as an array of three floats. For example, a rotation of 90 degrees around the Y-axis would be represented as `[0, 90, 0]`.
- Transform: Serialized as an object with position, rotation, and scale properties. For example, a transform with a position of (1, 2, 3), a rotation of (0, 90, 0), and a scale of (1, 1, 1) would be represented as:
```json
{
  "position": [1, 2, 3],
  "rotation": [0, 90, 0],
  "scale": [1, 1, 1]
}
```


### Serialization of Components

Components are serialized as objects with a type and properties as seen in the example below:
```json
{
  "type": "component_type",
  "properties": {
    "property_name": "property_value"
  }
}
```

#### Serialization of Common Components

- Transform: Serialized as an object with position, rotation, and scale properties.
- MeshRenderer: Serialized as an object with properties such as material, castShadows, and receiveShadows.
- MeshFilter: Serialized as an object with properties such as mesh.
- Collider: Serialized as an object with properties such as isTrigger, material, and bounds.

## Response Structure

Responses should be structured in the following way:

```plaintext
Some response text...

json
{
  "uid": {
    "Image": {
      "color": [1, 0, 0, 1]
    }
  }
}

Some more response text...
```

### Creating new Objects

Whenever the provided UID does not match any existing object, a new one will be automatically created.
The newly created object will be added to the UID Map and will be available to use in the same response.
The response must include the uid of the new object's parent, which will be used to create the object in the scene.
If no parent is specified, the new object will be created at the root of the scene.
All other components or properties will be applied in diff format to a newly created component with default values.
A Transform component will always be created for new objects, even if not specified in the response.

Example:
```json
{
  "newUid": {
    "parent": "uid2",
    "Image": {
        "color": [1, 0, 0, 1]
    },
    "Transform": {
      "position": [10, 20, 30],
      "rotation": [0, 0, 0],
      "scale": [1.5, 2, 1]
    }
  }
}
```