// <copyright file="ObjectDefinitionSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.Common
{
    using Anarchy;
    using Anarchy.Extensions;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Mono.Cecil.Cil;
    using Game;
    using Game.Common;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Routes;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Entities.UniversalDelegates;
    using Unity.Mathematics;
    using UnityEngine;

    /// <summary>
    /// Overrides vertical position of creation definition.
    /// </summary>
    public partial class ObjectDefinitionSystem : GameSystemBase
    {
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private EntityQuery m_ObjectDefinitionQuery;
        private ILog m_Log;
        private Unity.Mathematics.Random m_Random;
        private int m_PreviousRandomSeed;
        private float m_ElevationDelta;
        private float m_ElevationVariance;
        private ToolRaycastSystem m_ToolRaycastSystem;

        /// <summary>
        /// Sets the elevation delta.
        /// </summary>
        public float ElevationDelta
        {
            set { m_ElevationDelta = value; }
        }

        /// <summary>
        /// Sets the elevation variance.
        /// </summary>
        public float ElevationVariance
        {
            set { m_ElevationVariance = value; }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ToolRaycastSystem = World.GetOrCreateSystemManaged<ToolRaycastSystem>();
            m_Log.Info($"[{nameof(ObjectDefinitionSystem)}] {nameof(OnCreate)}");
            m_ObjectDefinitionQuery = SystemAPI.QueryBuilder()
                .WithAllRW<Game.Tools.ObjectDefinition>()
                .WithAll<CreationDefinition, Updated>()
                .WithNone<Deleted, Overridden>()
                .Build();

            m_Random = new ();
            RequireForUpdate(m_ObjectDefinitionQuery);
        }


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if ((m_ToolSystem.activeTool != m_ObjectToolSystem && m_ToolSystem.activeTool.toolID != "Line Tool") || (!AnarchyMod.Instance.Settings.ShowElevationToolOption && !AnarchyMod.Instance.Settings.ConstrainBrush))
            {
                return;
            }

            if (m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Create && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Line && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Curve && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Stamp)
            {
                return;
            }

            NativeArray<Entity> entities = m_ObjectDefinitionQuery.ToEntityArray(Allocator.Temp);

            if (AnarchyMod.Instance.Settings.ShowElevationToolOption)
            {
                EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.Temp);
                // Determine if RandomSeeds are fixed.
                bool seedsAreRadomized = false;
                if (entities.Length > 1)
                {
                    for (int i = 0; i < entities.Length; i++)
                    {
                        if (!EntityManager.TryGetComponent(entities[i], out CreationDefinition currentCreationDefinition))
                        {
                            continue;
                        }

                        if (m_PreviousRandomSeed == 0)
                        {
                            m_PreviousRandomSeed = currentCreationDefinition.m_RandomSeed;
                            continue;
                        }

                        if (m_PreviousRandomSeed != currentCreationDefinition.m_RandomSeed)
                        {
                            seedsAreRadomized = true;
                            m_PreviousRandomSeed = currentCreationDefinition.m_RandomSeed;
                            break;
                        }
                    }
                }
                else if (entities.Length == 1 &&
                         EntityManager.TryGetComponent(entities[0], out CreationDefinition currentCreationDefinition))
                {
                    /*
                    if (!TryGetObjectToolSeed(out uint objectToolSeed))
                    {
                        // If it's impossible to tell just assume randomized.
                        seedsAreRadomized = true;
                        m_PreviousRandomSeed = currentCreationDefinition.m_RandomSeed;
                    }
                    else
                    {
                        // Seed should be random and is random.
                        if (objectToolSeed != m_PreviousObjectToolRandomSeed &&
                            m_PreviousRandomSeed != currentCreationDefinition.m_RandomSeed)
                        {
                            seedsAreRadomized = true;
                            m_PreviousRandomSeed = currentCreationDefinition.m_RandomSeed;
                            m_PreviousObjectToolRandomSeed = objectToolSeed;
                        }

                        // Object tool hasn't been randomized.
                        else if (objectToolSeed == m_PreviousObjectToolRandomSeed)
                        {
                            seedsAreRadomized = true;
                            m_PreviousRandomSeed = currentCreationDefinition.m_RandomSeed;
                        }
                    }*/
                    seedsAreRadomized = true;
                    m_PreviousRandomSeed = currentCreationDefinition.m_RandomSeed;
                }

                foreach (Entity entity in entities)
                {
                    if (!EntityManager.TryGetComponent(entity, out CreationDefinition currentCreationDefinition))
                    {
                        continue;
                    }

                    if (!EntityManager.TryGetComponent(entity, out ObjectDefinition currentObjectDefinition))
                    {
                        continue;
                    }

                    if (!m_PrefabSystem.TryGetPrefab(currentCreationDefinition.m_Prefab, out PrefabBase prefabBase))
                    {
                        continue;
                    }

                    if (!m_PrefabSystem.TryGetEntity(prefabBase, out Entity prefabEntity) ||
                        (EntityManager.TryGetComponent(prefabEntity, out PlaceableObjectData placeableObjectData)
                        && ((placeableObjectData.m_Flags & PlacementFlags.RoadEdge) == PlacementFlags.RoadEdge
                        || (placeableObjectData.m_Flags & PlacementFlags.RoadNode) == PlacementFlags.RoadNode
                        || (placeableObjectData.m_Flags & PlacementFlags.RoadSide) == PlacementFlags.RoadSide)))
                    {
                        continue;
                    }

                    if (prefabBase is not BuildingPrefab)
                    {
                        float currentElevationDelta = m_ElevationDelta;
                        if (m_ElevationVariance != 0f)
                        {
                            if (!seedsAreRadomized)
                            {
                                m_Random.InitState((uint)((math.abs(currentObjectDefinition.m_Position.x) % 1.2456f) * (math.abs(currentObjectDefinition.m_Position.y) % 3.456f) * (math.abs(currentObjectDefinition.m_Position.z) % 5.356f) * 100000));
                                for (int i = 0; i < m_Random.NextInt(50); i++)
                                {
                                    m_Random.NextInt();
                                }

                                for (int i = 0; i < m_Random.NextInt(50); i++)
                                {
                                    m_Random.NextFloat();
                                }
                            }
                            else
                            {
                                m_Random.InitState((uint)currentCreationDefinition.m_RandomSeed);
                            }

                            currentElevationDelta += m_Random.NextFloat(-m_ElevationVariance, m_ElevationVariance);
                        }

                        if (!EntityManager.HasComponent<StackData>(currentCreationDefinition.m_Prefab))
                        {
                            currentObjectDefinition.m_Elevation = Mathf.Max(currentElevationDelta, 0);
                            currentObjectDefinition.m_Position.y += currentElevationDelta;
                        }
                        else
                        {
                            currentObjectDefinition.m_Position.y += currentElevationDelta;
                        }

                        buffer.SetComponent(entity, currentObjectDefinition);
                    }
                }

                buffer.Playback(EntityManager);
                buffer.Dispose();
            }

            if (m_ToolSystem.activeTool == m_ObjectToolSystem
               && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush &&
               AnarchyMod.Instance.Settings.ConstrainBrush)
            {
                foreach (Entity entity in entities)
                {
                    if (!EntityManager.TryGetComponent(entity, out ObjectDefinition currentObjectDefinition))
                    {
                        continue;
                    }

                    if (m_ToolRaycastSystem.GetRaycastResult(out RaycastResult result) &&
                        !EntityManager.HasComponent<Deleted>(result.m_Owner))
                    {
                        float2 objectXZ = new (currentObjectDefinition.m_Position.x, currentObjectDefinition.m_Position.z);
                        float2 raycastXZ = new (result.m_Hit.m_Position.x, result.m_Hit.m_Position.z);
                        if (Vector2.Distance(objectXZ, raycastXZ) > (0.333f * m_ObjectToolSystem.brushSize))
                        {
                            EntityManager.DestroyEntity(entity);
                            continue;
                        }
                    }
                }
            }

            entities.Dispose();
        }

        /*
        private bool TryGetObjectToolSeed(out uint objectToolSeed)
        {
            object randomSeedObj = m_ObjectToolSystem.GetMemberValue("m_RandomSeed");
            objectToolSeed = 0;
            if (randomSeedObj is not null && randomSeedObj is RandomSeed)
            {
                RandomSeed randomSeed = (RandomSeed)randomSeedObj;
                object seedObj = randomSeed.GetMemberValue("m_Seed");
                if (seedObj is not null && seedObj is uint)
                {
                    objectToolSeed = (uint)seedObj;
                    return true;
                }
            }

            return false;
        }*/
    }
}
