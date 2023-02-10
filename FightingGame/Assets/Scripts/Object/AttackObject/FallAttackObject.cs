using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FGDefine;
using System;

// 해당 스크립트에서만 사용할 예정인 열거형
public enum ENUM_FALLOBJECTSTATE_TYPE
{
    Generate = 0, // 생성
    Fall = 1, // 낙하 (Loop)
    Explode = 2, // 폭발
}

public class FallAttackObject : GenerateAttackObject
{
    Animator anim;
    protected Rigidbody2D rigid2D;
    [SerializeField] Vector2 shotPowerVec;
    ENUM_FALLOBJECTSTATE_TYPE currMyState = ENUM_FALLOBJECTSTATE_TYPE.Generate;

    float masterPosVecY;

    public override void Init()
    {
        base.Init();

        if (rigid2D == null)
            rigid2D = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponent<Animator>();

        var param = MakeSyncAnimParam();
        SyncAnimator(anim, param);

        if (isServerSyncState)
        {
            SyncPhysics(rigid2D);
        }
    }

    private AnimatorSyncParam[] MakeSyncAnimParam()
    {
        AnimatorSyncParam[] syncParams = new AnimatorSyncParam[]
        {
            new AnimatorSyncParam("GenerateTrigger", AnimParameterType.Trigger),
            new AnimatorSyncParam("FallTrigger", AnimParameterType.Trigger),
            new AnimatorSyncParam("ExplodeTrigger", AnimParameterType.Trigger),
        };

        return syncParams;
    }

    [BroadcastMethod]
    public override void Activate_AttackObject(Vector2 _summonPosVec, ENUM_TEAM_TYPE _teamType, bool _reverseState)
    {
        currMyState = ENUM_FALLOBJECTSTATE_TYPE.Generate;

        masterPosVecY = _summonPosVec.y; // 시전자의 y좌표(월드) 저장

        base.Activate_AttackObject(_summonPosVec, _teamType, _reverseState);

        Set_AnimTrigger(ENUM_FALLOBJECTSTATE_TYPE.Generate);
    }

    private void Set_AnimTrigger(ENUM_FALLOBJECTSTATE_TYPE fallObjectState)
    {
        SetAnimTrigger(fallObjectState.ToString() + "Trigger");
        currMyState = fallObjectState;

        if(fallObjectState == ENUM_FALLOBJECTSTATE_TYPE.Fall)
        {
            Vector2 updateShotPowerVec = new Vector2(reverseState ? shotPowerVec.x * -1f : shotPowerVec.x, shotPowerVec.y);
            rigid2D.AddForce(updateShotPowerVec);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 낙하상태가 아니거나, 시전자의 좌표에 도달하기 전이면 리턴 (임시)
        if (currMyState != ENUM_FALLOBJECTSTATE_TYPE.Fall 
            || masterPosVecY < transform.position.y)
            return;

        if(collision.tag == ENUM_TAG_TYPE.Ground.ToString())
        {
            Set_AnimTrigger(ENUM_FALLOBJECTSTATE_TYPE.Explode);
            rigid2D.velocity = Vector2.zero;
        }
    }

    public void AnimEvent_Falling()
    {
        Set_AnimTrigger(ENUM_FALLOBJECTSTATE_TYPE.Fall);
    }
}