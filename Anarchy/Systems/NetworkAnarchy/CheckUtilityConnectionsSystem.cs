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
    using System.Windows.Forms;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// A system that prevents objects from being overriden when placed on each other.
    /// </summary>
    public partial class CheckUtilityConnectionsSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_UtilityConnectionCheckQuery;
        private ModificationEndBarrier m_Barrier;
        private PrefabSystem m_PrefabSystem;
        private EntityArchetype m_ElectricalNodeConnectionArchetype;
        private EntityArchetype m_ElectricalEdgeArchetype;

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
            m_ElectricalEdgeArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Simulation.ElectricityFlowEdge>());
            m_UtilityConnectionCheckQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Net.Node, PrefabRef, Game.Net.ConnectedEdge, CheckUtilityNodeConnection>()
                .WithNone<Game.Simulation.ElectricityNodeConnection, Game.Simulation.WaterPipeNodeConnection, Game.Net.Marker, Game.Tools.EditorContainer, Game.Common.Owner, Game.Net.Waterway, Game.Net.LocalConnect>()
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
                if (EntityManager.TryGetComponent(entity, out PrefabRef prefabRef)
                    && m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out PrefabBase prefab)
                    && prefab is not null
                    && ((EntityManager.TryGetComponent(prefabRef.m_Prefab, out Game.Prefabs.ElectricityConnectionData electricityConnectionData)
                    && EvaluateEelctricityConnectionData(electricityConnectionData, entity))
                    || EntityManager.HasComponent<Game.Prefabs.WaterPipeConnectionData>(prefabRef.m_Prefab)))
                {
                    Entity electricalFlowNode = buffer.CreateEntity(m_ElectricalNodeConnectionArchetype);
                    buffer.AddComponent<Game.Simulation.ElectricityNodeConnection>(entity);
                    Game.Simulation.ElectricityNodeConnection electricityNodeConnection = new Game.Simulation.ElectricityNodeConnection()
                    {
                        m_ElectricityNode = electricalFlowNode,
                    };
                    buffer.SetComponent(entity, electricityNodeConnection);

                    if (EntityManager.TryGetBuffer(entity, isReadOnly:true, out DynamicBuffer<Game.Net.ConnectedEdge> connectedEdges))
                    {
                        foreach (Game.Net.ConnectedEdge connectedEdge in connectedEdges)
                        {
                            if (EntityManager.TryGetComponent(connectedEdge.m_Edge, out PrefabRef connectedEdgePrefabRef)
                                && m_PrefabSystem.TryGetPrefab(connectedEdgePrefabRef.m_Prefab, out PrefabBase connectedEdgePrefab)
                                && connectedEdgePrefab is not null
                                && ((EntityManager.TryGetComponent(connectedEdgePrefabRef.m_Prefab, out Game.Prefabs.ElectricityConnectionData connectedEdgeElectricityConnectionData)
                                && EvaluateEelctricityConnectionData(electricityConnectionData, connectedEdge.m_Edge))
                                || EntityManager.HasComponent<Game.Prefabs.WaterPipeConnectionData>(prefabRef.m_Prefab)))
                            {
                                if (EntityManager.TryGetComponent(connectedEdge.m_Edge, out Game.Simulation.ElectricityNodeConnection connectedEdgeElectricityNodeConnection)
                                    && EntityManager.TryGetBuffer(connectedEdgeElectricityNodeConnection.m_ElectricityNode, isReadOnly: true, out DynamicBuffer<Game.Simulation.ConnectedFlowEdge> edgeFlowEdgeBuffer))
                                {
                                    Entity newFlowEdge = CreateFlowEdge(buffer, m_ElectricalEdgeArchetype, electricalFlowNode, connectedEdgeElectricityNodeConnection.m_ElectricityNode, Game.Net.FlowDirection.Both, 40000, edgeFlowEdgeBuffer);
                                    buffer.AppendToBuffer(connectedEdgeElectricityNodeConnection.m_ElectricityNode, new ConnectedFlowEdge() { m_Edge = newFlowEdge });
                                    buffer.AppendToBuffer(electricalFlowNode, new ConnectedFlowEdge() { m_Edge = newFlowEdge });
                                }
                            }
                        }
                    }
                }

                buffer.RemoveComponent<CheckUtilityNodeConnection>(entity);
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

