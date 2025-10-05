using UnityEngine;

[RequireComponent(typeof(Animation))]
public class UnitAnimation : MonoBehaviour
{
    private Animation anim;
    private UnitData unitData;

    void Awake()
    {
        anim = GetComponent<Animation>();
    }

    public void Setup(UnitData data)
    {
        unitData = data;
    }

    public void PlayAppear()
    {
        if (unitData.animationData.appear != null)
        {
            anim.Play(unitData.animationData.appear.name);
        }
    }

    public void PlayIdle()
    {
        if (unitData.animationData.idle != null && !anim.IsPlaying(unitData.animationData.idle.name))
        {
            anim.CrossFade(unitData.animationData.idle.name);
        }
    }

    public void PlayMove()
    {
        AnimationClip moveClip = unitData.animationData.GetRandomMoveClip();
        if (moveClip != null && !anim.IsPlaying(moveClip.name))
        {
            anim.CrossFade(moveClip.name);
        }
    }

    public void PlayAttackIdle()
    {
        if (unitData.animationData.rifleAttackIdle_Stand != null && !anim.IsPlaying(unitData.animationData.rifleAttackIdle_Stand.name))
        {
            anim.CrossFade(unitData.animationData.rifleAttackIdle_Stand.name);
        }
    }

    public void PlayAttackShot()
    {
        if (unitData.animationData.rifleShot_Stand != null)
        {
            anim.CrossFade(unitData.animationData.rifleShot_Stand.name);
            // After shooting, go back to aiming
            if (unitData.animationData.rifleAttackIdle_Stand != null)
            {
                anim.PlayQueued(unitData.animationData.rifleAttackIdle_Stand.name, QueueMode.CompleteOthers);
            }
        }
    }

    public void PlayMoveAttackForward()
    {
        if (unitData.animationData.moveAttack_Forward != null && !anim.IsPlaying(unitData.animationData.moveAttack_Forward.name))
        {
            anim.CrossFade(unitData.animationData.moveAttack_Forward.name);
        }
    }

    public void PlayMoveAttackBackward()
    {
        if (unitData.animationData.moveAttack_Backward != null && !anim.IsPlaying(unitData.animationData.moveAttack_Backward.name))
        {
            anim.CrossFade(unitData.animationData.moveAttack_Backward.name);
        }
    }

    public void PlayMoveAttackLeft()
    {
        if (unitData.animationData.moveAttack_Left != null && !anim.IsPlaying(unitData.animationData.moveAttack_Left.name))
        {
            anim.CrossFade(unitData.animationData.moveAttack_Left.name);
        }
    }

    public void PlayMoveAttackRight()
    {
        if (unitData.animationData.moveAttack_Right != null && !anim.IsPlaying(unitData.animationData.moveAttack_Right.name))
        {
            anim.CrossFade(unitData.animationData.moveAttack_Right.name);
        }
    }

    public void PlayDeath()
    {
        AnimationClip deathClip = unitData.animationData.GetRandomDeathClip();
        if (deathClip != null)
        {
            anim.Play(deathClip.name);
        }
    }
}