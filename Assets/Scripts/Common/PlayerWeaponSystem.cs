using Unity.Entities;
using Unity.NetCode;
using Rukhanka;
using Unity.Burst;
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial class PlayerWeaponSystem : SystemBase
{
    FastAnimatorParameter aimingParam = new FastAnimatorParameter("Aiming");
    //private EntityQuery updateAnimatorAimingQuery;
    protected override void OnCreate()
    {
    }
    protected override void OnStartRunning()
    {
        //updateAnimatorAimingQuery = SystemAPI.QueryBuilder().WithAspect<AnimatorParametersAspect>().WithAll<OwnedAnimatorTag>().Build();
    }
    [BurstCompile]
    protected override void OnUpdate()
    {
        foreach (var aimInput in SystemAPI.Query<
        RefRW<PlayerAimInput>
        >().WithAll<Simulate>())
        {
            //var job = new ProcessPlayerInputJob
            //{
            //    AimInput = aimInput.Value,
            //    AimingParam = aimingParam
            //};
            //job.Schedule(updateAnimatorAimingQuery);
        }
    }
    protected override void OnDestroy()
    {
    }
    protected override void OnStopRunning()
    {
    }
}
[BurstCompile]
partial struct ProcessPlayerInputJob : IJobEntity
{
    public bool AimInput;
    public FastAnimatorParameter AimingParam;
    public void Execute(AnimatorParametersAspect paramAspect)
    {
        paramAspect.SetParameterValue(AimingParam, AimInput);
    }
}