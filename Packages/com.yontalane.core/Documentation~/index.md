# Core

## Math

### Static Methods

| Name | Description |
| --- | ---|
| **Radian To Vector2** | Create a vector representation of an angle. |
| **Rotate By Radians** | Rotate a vector by radians. |
| **Rotate By Degrees** | Rotate a vector by degrees. |
| **Set X** | Set a single value of a vector. |
| **Set Y** | Set a single value of a vector. |
| **Set Z** | Set a single value of a vector. |
| **To Vector2 Int** | Convert a float-based vector to an integer-based vector. |
| **To Vector3 Int** | Convert a float-based vector to an integer-based vector. |

## Utility

### Static Methods

| Name | Description |
| --- | ---|
| **Highlight** | Select the target Selectable. |
| **Refresh Layout Groups Immediate And Recursive** | Sometimes Vertical or Horizontal LayoutGroups don't adjust to match the size of their content. This method forces them to adjust. |

## Serializable Dictionary

In order to be serialized in the Inspector, SerializableDictionary needs to be inherited and specifically called out as Serializable. For example:

```
[System.Serializable]
public class StringTextureDictionary : SerializableDictionary<string, Texture> { }

public StringTextureDictionary dict = new StringTextureDictionary();
```

### Properties

| Name | Description |
| --- | ---|
| **Count** | The number of key/value pairs contained within the dictionary. |

### Public Methods

| Name | Description |
| --- | ---|
| **Get Key At** | Get the key at the provided index. |
| **Get Value At** | Get the value at the provided index. |
| **Get At** | Get the key/value pair at the provided index. |
| **Try Get** | If the key exists, set the `out` parameter to the key's associated value and return true. Otherwise, return false. |
| **Get** | Return the value for the key. |
| **Add** | Add a key/value pair. |
| **Insert** | Insert a key/value pair at the provided index. |
| **Remove** | Remove the provided key and its associated value. |
| **Remove At** | Remove the key/value pair at the provided index. |
| **Clear** | Clear the dictionary. |
| **Set** | Set the value associated with the provided key. |
| **Set At** | Set the key/value pair at the provided index. |
| **Index Of** | If the dictionary contains the provided key, return its index; otherwise, return -1. |
| **Contains** | Return true if the dictionary contains the provided key. Otherwise, return false. |

## Serializable Key Value Pair

### Properties

| Name | Description |
| --- | ---|
| **key** | The key. |
| **value** | The value. |

## Float Range

### Properties

| Name | Description |
| --- | ---|
| **min** | One of the two values in the range. |
| **max** | One of the two values in the range. |
| **Mid** | The midpoint between the range's min and max values. |
| **Random** | A random float between the range's min and max values. |

### Constructors

| Name | Description |
| --- | ---|
| **Float Range** | Creates a new Float Range. |

### Inherited Members
### Public Methods

| Name | Description |
| --- | ---|
| **To String** | Returns the min and max value in the form of a string. |

## Int Range

### Properties

| Name | Description |
| --- | ---|
| **min** | One of the two values in the range. |
| **max** | One of the two values in the range. |
| **Mid** | The midpoint (rounded to the nearest integer) between the range's min and max values. |
| **Random** | A random integer between the range's min and max values. |

### Constructors

| Name | Description |
| --- | ---|
| **Float Range** | Creates a new Int Range. |

### Inherited Members
### Public Methods

| Name | Description |
| --- | ---|
| **To String** | Returns the min and max value in the form of a string. |
