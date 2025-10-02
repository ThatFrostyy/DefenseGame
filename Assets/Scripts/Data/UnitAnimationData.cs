using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewUnitAnimationData", menuName = "RtV/Unit Animation Data", order = 2)]
public class UnitAnimationData : ScriptableObject
{
    [Header("Core Animations")]
    public AnimationClip appear;
    public AnimationClip idle;
    public List<AnimationClip> move;

    [Header("Combat Stances")]
    public AnimationClip rifleAttackIdle_Stand;
    public AnimationClip rifleAttackIdle_Crouch;
    public AnimationClip rifleShot_Stand;
    public AnimationClip rifleShot_Crouch;

    [Header("Movement While Attacking")]
    public AnimationClip moveAttack_Forward;
    public AnimationClip moveAttack_Backward;
    public AnimationClip moveAttack_Left;
    public AnimationClip moveAttack_Right;

    [Header("Death Animations")]
    public List<AnimationClip> deaths;

    public AnimationClip GetRandomMoveClip()
    {
        if (move == null || move.Count == 0) return null;
        int index = Random.Range(0, move.Count);
        return move[index];
    }

    public AnimationClip GetRandomDeathClip()
    {
        if (deaths == null || deaths.Count == 0) return null;
        int index = Random.Range(0, deaths.Count);
        return deaths[index];
    }
}