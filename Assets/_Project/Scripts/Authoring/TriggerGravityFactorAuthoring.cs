using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public struct TriggerGravityFactor : IComponentData
{
    public float GravityFactor;
    public float DampingFactor;
}

public class TriggerGravityFactorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float GravityFactor = 0f;
    public float DampingFactor = 0.9f;

    void OnEnable() {}

    void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (enabled)
        {
            dstManager.AddComponentData(entity, new TriggerGravityFactor()
            {
                GravityFactor = GravityFactor,
                DampingFactor = DampingFactor,
            });
        }
    }
}


// This system sets the PhysicsGravityFactor of any dynamic body that enters a Trigger Volume.
// A Trigger Volume is defined by a PhysicsShapeAuthoring with the `Is Trigger` flag ticked and a
// TriggerGravityFactor behaviour added.
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ExportPhysicsWorld))]
//[UpdateBefore(typeof(EndFramePhysicsSystem))]
[UpdateBefore(typeof(ProjectileMoveSystem))]
public partial class TriggerGravityFactorSystem : JobComponentSystem
{
    StepPhysicsWorld m_StepPhysicsWorldSystem;
    EntityQuery m_TriggerGravityGroup;
    PreTransformGroupBarrier preTransformBarrier;
    BuildPhysicsWorld m_BuildPhysicsWorldSystem;

    protected override void OnCreate()
    {
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        preTransformBarrier = World.GetOrCreateSystem<PreTransformGroupBarrier>();
        m_TriggerGravityGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(Character)
            }
        });
    }
    
    [BurstCompile]
    struct TriggerGravityFactorJob : ITriggerEventsJob
    {
//        [ReadOnly] public ComponentDataFromEntity<TriggerGravityFactor> TriggerGravityFactorGroup;
         public ComponentDataFromEntity<Health> HealthGroup;
        [ReadOnly] public ComponentDataFromEntity<DamageProjectile> DamageProjectileGroup;
        [ReadOnly] public ComponentDataFromEntity<PlayerCharacter> CharacterGroup;
        public EntityCommandBuffer entityCommandBuffer;
//        public ComponentDataFromEntity<PhysicsGravityFactor> PhysicsGravityFactorGroup;
        //public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityGroup;

        public void Execute(TriggerEvent triggerEvent)
        {
            
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            bool isBodyATrigger = HealthGroup.HasComponent(entityA);
            bool isBodyBTrigger = DamageProjectileGroup.HasComponent(entityB);
            bool isBodyCTrigger = CharacterGroup.HasComponent(entityA);
//            Debug.Log($"HealthGroup {isBodyATrigger}, DamageProjectileGroup {isBodyBTrigger} isBodyCTrigger {isBodyCTrigger}");
            // Ignoring Triggers overlapping other Triggers
            if (isBodyCTrigger) return;
//            if (isBodyATrigger && isBodyBTrigger 
//                               //&& isBodyCTrigger
//                )
//                return;
            if (isBodyATrigger && isBodyBTrigger 
                              // && !isBodyCTrigger
                               )
            {
                Health h = HealthGroup[entityA];
                h.Value -= DamageProjectileGroup[entityB].Damage;
                HealthGroup[entityA] = h;
                entityCommandBuffer.DestroyEntity(entityB);
            }
//            bool isBodyADynamic = PhysicsVelocityGroup.HasComponent(entityA);
//            bool isBodyBDynamic = PhysicsVelocityGroup.HasComponent(entityB);
//
//            // Ignoring overlapping static bodies
//            if ((isBodyATrigger && !isBodyBDynamic) ||
//                (isBodyBTrigger && !isBodyADynamic))
//                return;

//            var triggerEntity = isBodyATrigger ? entityA : entityB;
//            var dynamicEntity = isBodyATrigger ? entityB : entityA;
//
//            var triggerGravityComponent = TriggerGravityFactorGroup[entityB];
            // tweak PhysicsGravityFactor
            {
                
                
                
//                var component = PhysicsGravityFactorGroup[dynamicEntity];
//                component.Value = triggerGravityComponent.GravityFactor;
//                PhysicsGravityFactorGroup[dynamicEntity] = component;
//                Debug.Log($"Trigger Damage");
            }
            // damp velocity
//            {
//                var component = PhysicsVelocityGroup[dynamicEntity];
//                component.Linear *= triggerGravityComponent.DampingFactor;
//                PhysicsVelocityGroup[dynamicEntity] = component;
                
//                Debug.Log($"damp velocityr");
//            }
        }
    }


//    protected override void OnStartRunning()
//    {
//        base.OnStartRunning();
//        this.RegisterPhysicsRuntimeSystemReadOnly();
//    }
//
//    protected override void OnUpdate()
//    {
//        if (m_TriggerGravityGroup.CalculateEntityCount() == 0)
//        {
//            return;
//        }
//
//        Dependency = new TriggerGravityFactorJob
//        {
//            TriggerGravityFactorGroup = GetComponentDataFromEntity<TriggerGravityFactor>(true),
//            PhysicsGravityFactorGroup = GetComponentDataFromEntity<PhysicsGravityFactor>(),
//            PhysicsVelocityGroup = GetComponentDataFromEntity<PhysicsVelocity>(),
//        }.Schedule(m_StepPhysicsWorldSystem.Simulation, Dependency);
//    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = new TriggerGravityFactorJob
        {
            //TriggerGravityFactorGroup = GetComponentDataFromEntity<TriggerGravityFactor>(true),
            HealthGroup = GetComponentDataFromEntity<Health>(),
            DamageProjectileGroup = GetComponentDataFromEntity<DamageProjectile>(true),
            //PhysicsGravityFactorGroup = GetComponentDataFromEntity<PhysicsGravityFactor>(),
            //PhysicsVelocityGroup = GetComponentDataFromEntity<PhysicsVelocity>(),
            CharacterGroup = GetComponentDataFromEntity<PlayerCharacter>(),
            entityCommandBuffer = preTransformBarrier.CreateCommandBuffer(),
        }.Schedule(m_StepPhysicsWorldSystem.Simulation,ref m_BuildPhysicsWorldSystem.PhysicsWorld, inputDeps);
        preTransformBarrier.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}