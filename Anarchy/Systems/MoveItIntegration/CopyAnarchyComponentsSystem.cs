// <copyright file="CopyAnarchyComponentsSystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Anarchy.Systems.MoveItIntegration
{
    using System;
    using System.Reflection;
    using Anarchy.Components;
    using Anarchy.Systems.ObjectElevation;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Citizens;
    using Game.Common;
    using Game.Creatures;
    using Game.Objects;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Copies Anarchy components while using Move It's copy feature.
    /// </summary>
    public partial class CopyAnarchyComponentsSystem : GameSystemBase
    {
        private const string MoveItToolID = "MoveItTool";
        private ILog m_Log;
        private ToolBaseSystem m_MoveItTool;
        private PropertyInfo m_MIT_Copying;
        private MethodInfo m_MIT_TryGetOriginalEntity;
        private EntityQuery m_MITOriginalQuery;
        private ToolSystem m_ToolSystem;
        private bool m_FoundOriginalType;

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = AnarchyMod.Instance.Log;
            m_Log.Info($"{nameof(CopyAnarchyComponentsSystem)}.{nameof(OnCreate)}");

            // System references
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();

            Enabled = false;
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            if (m_MoveItTool is not null)
            {
                return;
            }

            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (!m_FoundOriginalType)
                {
                    foreach (Assembly assembly in assemblies)
                    {
                        Type type = assembly.GetType("MoveIt.Components.MIT_Original");
                        if (type != null)
                        {
                            m_Log.Info($"Found {type.FullName} in {type.Assembly.FullName}. ");
                            ComponentType originalType = ComponentType.ReadOnly(type);
                            m_FoundOriginalType = true;

                            m_MITOriginalQuery = GetEntityQuery(new EntityQueryDesc[]
                            {
                                new EntityQueryDesc
                                {
                                    All = new ComponentType[]
                                    {
                                        ComponentType.ReadOnly<Game.Tools.Temp>(),
                                        originalType,
                                    },
                                    Any = new ComponentType[]
                                    {
                                        ComponentType.ReadOnly<Static>(),
                                        ComponentType.ReadOnly<Game.Objects.Object>(),
                                        ComponentType.ReadOnly<Game.Tools.EditorContainer>(),
                                    },
                                    None = new ComponentType[]
                                    {
                                        ComponentType.ReadOnly<Owner>(),
                                        ComponentType.ReadOnly<Animal>(),
                                        ComponentType.ReadOnly<Game.Creatures.Pet>(),
                                        ComponentType.ReadOnly<Creature>(),
                                        ComponentType.ReadOnly<Moving>(),
                                        ComponentType.ReadOnly<Household>(),
                                        ComponentType.ReadOnly<Vehicle>(),
                                        ComponentType.ReadOnly<Game.Common.Event>(),
                                        ComponentType.ReadOnly<Game.Routes.TransportStop>(),
                                        ComponentType.ReadOnly<Game.Routes.TransportLine>(),
                                        ComponentType.ReadOnly<Game.Routes.TramStop>(),
                                        ComponentType.ReadOnly<Game.Routes.TrainStop>(),
                                        ComponentType.ReadOnly<Game.Routes.AirplaneStop>(),
                                        ComponentType.ReadOnly<Game.Routes.BusStop>(),
                                        ComponentType.ReadOnly<Game.Routes.ShipStop>(),
                                        ComponentType.ReadOnly<Game.Routes.TakeoffLocation>(),
                                        ComponentType.ReadOnly<Game.Routes.TaxiStand>(),
                                        ComponentType.ReadOnly<Game.Routes.Waypoint>(),
                                        ComponentType.ReadOnly<Game.Routes.MailBox>(),
                                        ComponentType.ReadOnly<Game.Routes.WaypointDefinition>(),
                                    },
                                },
                            });
                        }
                    }
                }

                if (World.GetOrCreateSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(MoveItToolID)) is ToolBaseSystem moveItTool)
                {
                    // Found it
                    m_Log.Info($"{nameof(ResetTransformSystem)}.{nameof(OnGameLoadingComplete)} found Move It.");
                    m_MIT_Copying = moveItTool.GetType().GetProperty("Copying");
                    m_MIT_TryGetOriginalEntity = moveItTool.GetType().GetMethod("GetOriginalEntity", BindingFlags.Public | BindingFlags.Static);
                    if (m_MIT_Copying is not null &&
                        m_MIT_TryGetOriginalEntity is not null &&
                        m_FoundOriginalType == true)
                    {
                        m_MoveItTool = moveItTool;
                        Enabled = true;
                        m_Log.Info($"{nameof(CopyAnarchyComponentsSystem)}.{nameof(OnGameLoadingComplete)} saved moveItTool System Enabled");
                    }
                    else
                    {
                        if (m_MIT_Copying is null)
                        {
                            m_Log.Info($"{nameof(CopyAnarchyComponentsSystem)}.{nameof(OnGameLoadingComplete)} Could not find Move It's Copying property.");
                        }

                        if (m_MIT_TryGetOriginalEntity is null)
                        {
                            m_Log.Info($"{nameof(CopyAnarchyComponentsSystem)}.{nameof(OnGameLoadingComplete)} Could not find Move It's TryGetOriginalEntity method.");
                        }
                    }
                }
                else
                {
                    m_Log.Info($"{nameof(ResetTransformSystem)}.{nameof(OnGameLoadingComplete)} move it tool not found");
                }
            }
            catch (Exception e)
            {
                m_Log.Info($"{nameof(ResetTransformSystem)}.{nameof(OnGameLoadingComplete)} Encountered exception {e}.");
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool.toolID != MoveItToolID ||
                m_MoveItTool is null ||
                m_MIT_Copying is null ||
               !m_FoundOriginalType ||
                m_MIT_TryGetOriginalEntity is null ||
               !(bool)m_MIT_Copying.GetValue(m_MoveItTool) ||
                m_MITOriginalQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.Temp);

            NativeArray<Entity> entities = m_MITOriginalQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                object results = m_MIT_TryGetOriginalEntity.Invoke(m_MoveItTool, new object[] { entities[i] });
                if (results is null ||
                    (Entity)results == Entity.Null)
                {
                    m_Log.Debug($"{nameof(CopyAnarchyComponentsSystem)}.{nameof(OnUpdate)} Original Entity results invalid.");
                    continue;
                }

                Entity original = (Entity)results;
                if (EntityManager.HasComponent<PreventOverride>(original))
                {
                    buffer.AddComponent<PreventOverride>(entities[i]);
                }

                if (EntityManager.HasComponent<TransformRecord>(original) &&
                    EntityManager.TryGetComponent<Game.Objects.Transform>(entities[i], out Game.Objects.Transform transform))
                {
                    buffer.AddComponent(entities[i], new TransformRecord() { m_Position = transform.m_Position, m_Rotation = transform.m_Rotation });
                }
            }

            buffer.Playback(EntityManager);
            buffer.Dispose();
        }
    }
}
