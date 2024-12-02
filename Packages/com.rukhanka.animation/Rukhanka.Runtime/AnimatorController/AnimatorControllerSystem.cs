using Rukhanka;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
#if RUKHANKA_WITH_NETCODE
using Unity.NetCode;
#endif
using UnityEngine;
using static Rukhanka.AnimatorControllerSystemJobs;

/////////////////////////////////////////////////////////////////////////////////////////////////////

[assembly: RegisterGenericSystemType(typeof(AnimatorControllerSystem<PredictedAnimatorControllerQuery>))]
[assembly: RegisterGenericSystemType(typeof(AnimatorControllerSystem<AnimatorControllerQuery>))]

/////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{

[DisableAutoCreation]
public partial struct AnimatorControllerSystem<T>: ISystem where T: AnimatorControllerQueryCreator, new()
{
	EntityQuery animatorControllerQuery;

/////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnCreate(ref SystemState ss)
	{
		var queryCreator = new T();
		animatorControllerQuery = queryCreator.CreateQuery(ref ss);
		
		ss.RequireForUpdate(animatorControllerQuery);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnUpdate(ref SystemState ss)
	{
		var dt = SystemAPI.Time.DeltaTime;
		var frameCount = Time.frameCount;

	#if RUKHANKA_DEBUG_INFO
		SystemAPI.TryGetSingleton<DebugConfigurationComponent>(out var dc);
	#endif

		var controllerLayersBufferHandle = SystemAPI.GetBufferTypeHandle<AnimatorControllerLayerComponent>();
		var controllerParametersBufferHandle = SystemAPI.GetBufferTypeHandle<AnimatorControllerParameterComponent>();
		var animatorOverrideAnimationsLookup = SystemAPI.GetComponentLookup<AnimatorOverrideAnimations>(true);
		var entityTypeHandle = SystemAPI.GetEntityTypeHandle();
		var controllerEventsBufferLookup = SystemAPI.GetBufferLookup<AnimatorControllerEventComponent>();
		var animDBSingleton = SystemAPI.GetSingleton<BlobDatabaseSingleton>();
		
		var stateMachineProcessJob = new StateMachineProcessJob()
		{
			controllerLayersBufferHandle = controllerLayersBufferHandle,
			controllerParametersBufferHandle = controllerParametersBufferHandle,
			dt = dt,
			frameIndex = frameCount,
			entityTypeHandle = entityTypeHandle,
			controllerEventsBufferLookup = controllerEventsBufferLookup,
			animationDatabase = animDBSingleton.animations,
			animatorOverrideAnimationLookup = animatorOverrideAnimationsLookup,
		#if RUKHANKA_DEBUG_INFO
			doAnimatorProcessLogging = dc.logAnimatorControllerProcesses,
			doAnimatorEventsLogging = dc.logAnimatorControllerEvents,
		#endif
		};

		ss.Dependency = stateMachineProcessJob.ScheduleParallel(animatorControllerQuery, ss.Dependency);
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////

public interface AnimatorControllerQueryCreator
{
	EntityQuery CreateQuery(ref SystemState ss);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////

public struct PredictedAnimatorControllerQuery: AnimatorControllerQueryCreator
{
	public EntityQuery CreateQuery(ref SystemState ss)
	{
		var eqBuilder0 = new EntityQueryBuilder(Allocator.Temp)
		.WithAllRW<AnimatorControllerLayerComponent>()
	#if RUKHANKA_WITH_NETCODE
		.WithAll<Simulate, PredictedGhost>()
	#endif
		;
		var animatorControllerQuery = ss.GetEntityQuery(eqBuilder0);
		return animatorControllerQuery;
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////

public struct AnimatorControllerQuery: AnimatorControllerQueryCreator
{
	public EntityQuery CreateQuery(ref SystemState ss)
	{
		var eqBuilder0 = new EntityQueryBuilder(Allocator.Temp)
		.WithAllRW<AnimatorControllerLayerComponent>()
	#if RUKHANKA_WITH_NETCODE
		.WithNone<GhostInstance>()
	#endif
		;
		var animatorControllerQuery = ss.GetEntityQuery(eqBuilder0);
		return animatorControllerQuery;
	}
}
}
