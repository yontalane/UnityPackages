# Changelog

## 1.0.4 - 2022.12.24

**Added**

* Option for synchronous (not just asynchronous) path finding.

**Changed**

* Callback takes integer parameters instead of Vector2Int.

**Fixed**

## 1.0.3 - 2022.12.23

**Added**

* Override constructor and FindPath() to not use Vector2Int.

**Changed**

**Fixed**

## 1.0.2 - 2022.12.23

**Added**

**Changed**

* Undid generic class definition.
* Got rid of IGridNode.
* GridNav navigates using a callback to determine if a coordinate is pathable.

**Fixed**

## 1.0.1 - 2022.12.23

**Added**

**Changed**

* IGridNode no longer requires a transform.
* GridNavigator is defined using an IGridNode implementation (```GridNavigator<T> where T : IGridNode```).

**Fixed**

## 1.0.0 - 2022.12.23

**Added**

* Grid Nav

**Changed**

**Fixed**
