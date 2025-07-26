## Patch V1.7.18
* Fix transform lock on network subobjects.
* Added support for relative transform lock on node subobjects. Nodes sometimes flip and that messes with relative location.
* Fix end walls for some networks with forced quays.
* Fix not re-enabling error checks in the Editor.
* Elevated is now mutually exclusive with Quay and Retaining Wall.
* Constant slope added as an option for Vanilla Quays networks.
* Updated some localizations.
* Handles networks with RaisedIsElevated and ElevatedIsRaised flags so that Elevated is no longer an option for Vanilla Quay networks. ERU can still apply them though.