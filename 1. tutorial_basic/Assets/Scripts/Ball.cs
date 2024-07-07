using Fusion;

public class Ball : NetworkBehaviour
{
    // [Networked]: 해당 어트리뷰트가 있는 프로퍼티만 네트워킹 됨
    [Networked] private TickTimer life { get; set; }

    public void Init()
    {
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            Runner.Despawn(Object);
        else
            transform.position += 5 * transform.forward * Runner.DeltaTime;
    }
}