# Patch v1.7.24.3
* Removed querying for Applied component when checking for changes to Locked Transforms.
* Fixed critical problems with backup (now primary) solution for EDT Transform Gizmo Tool.
* For Checking Locked Transforms, reduced use of ECBs in favor of EntityManager to avoid accidently trying to modify components on invalid entities leading to critical problems.