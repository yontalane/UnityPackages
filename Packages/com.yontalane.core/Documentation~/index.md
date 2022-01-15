# Core

## Math

## Utility

## SerializableDictionary

In order to be serialized in the Inspector, SerializableDictionary needs to be inherited and specifically called out as Serializable. For example:

```
[System.Serializable]
public class StringTextureDictionary : SerializableDictionary<string, Texture> { }

public StringTextureDictionary dict = new StringTextureDictionary();
```

## FloatRange

## IntRange
