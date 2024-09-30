// <copyright file="CheckUtilityConnectionsSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.NetworkAnarchy
{
    using Anarchy;
    using Anarchy.Components;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Simulation;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Checks electrical and water pipe node connections and re-establish the utility network around broken nodes/segments if needed.
    /// </summary>
    public partial class CheckUtilityConnectionsSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_UtilityConnectionCheckQuery;
        private ModificationEndBarrier m_Barrier;
        private PrefabSystem m_PrefabSystem;
        private EntityArchetype m_ElectricalNodeConnectionArchetype;
        private EntityArchetype m_ElectricalEdgeArchetype;
        private EntityArchetype m_WaterEdgeArchetype;
        private EntityArchetype m_WaterPipeNodeConectionArchetype;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckUtilityConnectionsSystem"/> class.
        /// </summary>
        public CheckUtilityConnectionsSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(CheckUtilityConnectionsSystem)} Created.");
            m_Barrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ElectricalNodeConnectionArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Simulation.ElectricityFlowNode>(), ComponentType.ReadWrite<Game.Simulation.ConnectedFlowEdge>());
            m_WaterPipeNodeConectionArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Simulation.WaterPipeNode>(), ComponentType.ReadWrite<Game.Simulation.ConnectedFlowEdge>());
            m_ElectricalEdgeArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Simulation.ElectricityFlowEdge>());
            m_WaterEdgeArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Simulation.WaterPipeEdge>());
            m_UtilityConnectionCheckQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Net.Node, PrefabRef, Game.Net.ConnectedEdge, CheckUtilityNodeConnection>()
                .WithNone<Game.Simulation.ElectricityNodeConnection, Game.Simulation.WaterPipeNodeConnection, Game.Tools.Temp, Game.Common.Deleted>()
                .Build();
            RequireForUpdate(m_UtilityConnectionCheckQuery);
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_UtilityConnectionCheckQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer();
            NativeArray<Entity> entities = m_UtilityConnectionCheckQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out PrefabRef prefabRef)
                    || !m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out PrefabBase prefab)
                    || prefab is null)
                {
                    continue;
                }

                if (EntityManager.TryGetComponent(prefabRef.m_Prefab, out Game.Prefabs.ElectricityConnectionData electricityConnectionData)
                    && EvaluateEelctricityConnectionData(electricityConnectionData, entity))
                {
                    m_Log.Debug($"{nameof(CheckUtilityConnectionsSystem)}.{nameof(OnUpdate)} creating electrical flow node entity.");
                    Entity electricalFlowNode = buffer.CreateEntity(m_ElectricalNodeConnectionArchetype);
                    buffer.AddComponent<Game.Simulation.ElectricityNodeConnection>(entity);
                    Game.Simulation.ElectricityNodeConnection electricityNodeConnection = new Game.Simulation.ElectricityNodeConnection()
                    {
                        m_ElectricityNode = electricalFlowNode,
                    };
                    buffer.SetComponent(entity, electricityNodeConnection);

                    if (EntityManager.TryGetBuffer(entity, isReadOnly:true, out DynamicBuffer<Game.Net.ConnectedEdge> connectedEdges))
                    {
                        m_Log.Debug($"{nameof(CheckUtilityConnectionsSystem)}.{nameof(OnUpdate)} found electrical connected edges buffer.");
                        foreach (Game.Net.ConnectedEdge connectedEdge in connectedEdges)
                        {
                            m_Log.Debug($"{nameof(CheckUtilityConnectionsSystem)}.{nameof(OnUpdate)} checking electrical connected edge.");
                            if (EntityManager.TryGetComponent(connectedEdge.m_Edge, out PrefabRef connectedEdgePrefabRef)
                                && m_PrefabSystem.TryGetPrefab(connectedEdgePrefabRef.m_Prefab, out PrefabBase connectedEdgePrefab)
                                && connectedEdgePrefab is not null
                                && EntityManager.TryGetComponent(connectedEdgePrefabRef.m_Prefab, out Game.Prefabs.ElectricityConnectionData connectedEdgeElectricityConnectionData)
                                && EvaluateEelctricityConnectionData(connectedEdgeElectricityConnectionData, connectedEdge.m_Edge))
                            {
                                m_Log.Debug($"{nameof(CheckUtilityConnectionsSystem)}.{nameof(OnUpdate)} electrical passed check 1.");
                                if (EntityManager.TryGetComponent(connectedEdge.m_Edge, out Game.Simulation.ElectricityNodeConnection connectedEdgeElectricityNodeConnection)
                                    && EntityManager.TryGetBuffer(connectedEdgeElectricityNodeConnection.m_ElectricityNode, isReadOnly: true, out DynamicBuffer<Game.Simulation.ConnectedFlowEdge> edgeFlowEdgeBuffer))
                                {
                                    m_Log.Debug($"{nameof(CheckUtilityConnectionsSystem)}.{nameof(OnUpdate)} electrical passed check 2.");
                                    Entity newFlowEdge = CreateFlowEdge(buffer, m_ElectricalEdgeArchetype, electricalFlowNode, connectedEdgeElectricityNodeConnection.m_ElectricityNode, connectedEdgeElectricityConnectionData.m_Direction, connectedEdgeElectricityConnectionData.m_Capacity, edgeFlowEdgeBuffer);
                                }
                            }
                        }
                    }
                }

                if (EntityManager.HasComponent<Game.Prefabs.WaterPipeConnectionData>(prefabRef.m_Prefab))
                {
                    m_Log.Debug($"{nameof(CheckUtilityConnectionsSystem)}.{nameof(OnUpdate)} creating water pipe flow node entity.");
                    Entity waterPipeFlowNode = buffer.CreateEntity(m_WaterPipeNodeConectionArchetype);
                    buffer.AddComponent<Game.Simulation.WaterPipeNodeConnection>(entity);
                    Game.Simulation.WaterPipeNodeConnection waterPipeNodeConnection = new Game.Simulation.WaterPipeNodeConnection()
                    {
                        m_WaterPipeNode = waterPipeFlowNode,
                    };
                    buffer.SetComponent(entity, waterPipeNodeConnection);

                    if (EntityManager.TryGetBuffer(entity, isReadOnly: true, out DynamicBuffer<Game.Net.ConnectedEdge> connectedEdges))
                    {
                        m_Log.Debug($"{nameof(CheckUtilityConnectionsSystem)}.{nameof(OnUpdate)} found  water connected edges buffer.");
                        foreach (Game.Net.ConnectedEdge connectedEdge in connectedEdges)
                        {
                            m_Log.Debug($"{nameof(CheckUtilityConnectionsSystem)}.{nameof(OnUpdate)} checking water connected edge.");
                            if (EntityManager.TryGetComponent(connectedEdge.m_Edge, out PrefabRef connectedEdgePrefabRef)
                                && m_PrefabSystem.TryGetPrefab(connectedEdgePrefabRef.m_Prefab, out PrefabBase connectedEdgePrefab)
                                && connectedEdgePrefab is not null
                                && EntityManager.TryGetComponent(connectedEdgePrefabRef.m_Prefab, out Game.Prefabs.WaterPipeConnectionData waterPipeConnectionData))
                            {
                                m_Log.Debug($"{nameof(CheckUtilityConnectionsSystem)}.{nameof(OnUpdate)} passed water check 1.");
                                if (EntityManager.TryGetComponent(connectedEdge.m_Edge, out Game.Simulation.WaterPipeNodeConnection connectedEdgeWaterPipeNodeConnection)
                                   && EntityManager.TryGetBuffer(connectedEdgeWaterPipeNodeConnection.m_WaterPipeNode, isReadOnly: true, out DynamicBuffer<Game.Simulation.ConnectedFlowEdge> edgeWaterFlowEdgeBuffer))
                                {
                                    m_Log.Debug($"{nameof(CheckUtilityConnectionsSystem)}.{nameof(OnUpdate)} passed water check 2.");
                                    Entity newFlowEdge = CreateFlowEdge(buffer, m_WaterEdgeArchetype, waterPipeFlowNode, connectedEdgeWaterPipeNodeConnection.m_WaterPipeNode, Game.Net.FlowDirection.None, waterPipeConnectionData.m_FreshCapacity, edgeWaterFlowEdgeBuffer);
                                }
                            }
                        }
                    }
                }

                buffer.RemoveComponent<CheckUtilityNodeConnection>(entity);
                m_Log.Debug($"{nameof(CheckUtilityConnectionsSystem)}.{nameof(OnUpdate)} removed check component and complete.");
            }

        }

        private bool EvaluateEelctricityConnectionData(Game.Prefabs.ElectricityConnectionData electricityConnectionData, Entity entity)
        {
            if (electricityConnectionData.m_CompositionAll.m_General == 0 && electricityConnectionData.m_CompositionAll.m_Left == 0 && electricityConnectionData.m_CompositionAll.m_Right == 0)
            {
                return true;
            }

            if (electricityConnectionData.m_CompositionAll.m_General == CompositionFlags.General.Lighting)
            {
                if (!EntityManager.TryGetBuffer(entity, isReadOnly: true, out DynamicBuffer<Game.Net.ConnectedEdge> connectedEdges))
                {
                    if (EntityManager.TryGetComponent(entity, out Game.Net.Upgraded upgraded) && (upgraded.m_Flags.m_General & CompositionFlags.General.Lighting) == CompositionFlags.General.Lighting)
                    {
                        return true;
                    }

                    return false;
                }

                foreach (Game.Net.ConnectedEdge edge in connectedEdges)
                {
                    if (EntityManager.TryGetComponent(edge.m_Edge, out Game.Net.Upgraded upgraded) && (upgraded.m_Flags.m_General & CompositionFlags.General.Lighting) == CompositionFlags.General.Lighting)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Entity CreateFlowEdge(EntityCommandBuffer buffer, EntityArchetype archetype, Entity endPointNode, Entity midPointNode, Game.Net.FlowDirection flowDirection, int capacity, DynamicBuffer<Game.Simulation.ConnectedFlowEdge> edgeFlowEdgeBuffer)
        {
            if (edgeFlowEdgeBuffer.Length == 1 && EntityManager.TryGetComponent(edgeFlowEdgeBuffer[0].m_Edge, out Game.Simulation.ElectricityFlowEdge flowEdge))
            {
                if (flowEdge.m_Start == midPointNode)
                {
                    return ElectricityGraphUtils.CreateFlowEdge(buffer, archetype, midPointNode, endPointNode, flowDirection, capacity);
                }
            }

            return ElectricityGraphUtils.CreateFlowEdge(buffer, archetype, endPointNode, midPointNode, flowDirection, capacity);
        }
    }
}

