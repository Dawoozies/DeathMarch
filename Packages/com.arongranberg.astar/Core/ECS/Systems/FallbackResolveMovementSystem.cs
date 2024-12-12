#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.ECS.RVO;

	/// <summary>Copies <see cref="MovementControl"/> to <see cref="ResolvedMovement"/> when no local avoidance is used</summary>
	[BurstCompile]
	[UpdateAfter(typeof(FollowerControlSystem))]
	[UpdateAfter(typeof(RVOSystem))] // Has to execute after RVOSystem in case that system detects that some agents should not be simulated using the RVO system anymore.
	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]

	[RequireMatchingQueriesForUpdate]
	public partial struct FallbackResolveMovementSystem : ISystem {
		public void OnUpdate (ref SystemState systemState) {
			new CopyJob {}.Schedule();
		}

		[BurstCompile]
		[WithAll(typeof(SimulateMovement))]
		[WithOptions(EntityQueryOptions.FilterWriteGroup)]
		public partial struct CopyJob : IJobEntity {
			public void Execute (in MovementControl control, ref ResolvedMovement resolved) {
				resolved.targetPoint = control.targetPoint;
				resolved.speed = control.speed;
				resolved.turningRadiusMultiplier = 1.0f;
				resolved.targetRotation = control.targetRotation;
				resolved.targetRotationHint = control.targetRotationHint;
				resolved.targetRotationOffset = control.targetRotationOffset;
				resolved.rotationSpeed = control.rotationSpeed;
			}
		}
	}
}
#endif
