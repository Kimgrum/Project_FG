using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FGDefine;
using System;

public class HitAttackObject : AttackObject
{
    protected Skill skillValue;
    Coroutine runTimeCheckCoroutine = null;

    public override void Init()
    {
        base.Init();

        ENUM_ATTACKOBJECT_NAME attackObjectName = (ENUM_ATTACKOBJECT_NAME)Enum.Parse(typeof(ENUM_ATTACKOBJECT_NAME), gameObject.name.ToString());
        if (!Managers.Data.SkillDict.TryGetValue((int)attackObjectName, out skillValue))
        {
            Debug.Log($"{gameObject.name} 를 초기화하지 못했습니다.");
        }
    }

    [BroadcastMethod]
    public override void Activate_AttackObject(Vector2 _summonPosVec, ENUM_TEAM_TYPE _teamType, bool _reverseState)
    {
        base.Activate_AttackObject(_summonPosVec,_teamType,_reverseState);

        if (PhotonLogicHandler.IsMine(viewID))
            Start_RunTimeCheckCoroutine();
    }

    private void Start_RunTimeCheckCoroutine()
    {
        runTimeCheckCoroutine = StartCoroutine(IRunTimeCheck(skillValue.runTime));
    }

    public override void OnDisable()
    {
        if(runTimeCheckCoroutine != null)
            StopCoroutine(runTimeCheckCoroutine);

        base.OnDisable();
    }

    // 수정 시 참조 확인 (재정의해서 사용하고 있음)
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (isServerSyncState && PhotonLogicHandler.IsMine(viewID)) // 맞는 애가 처리하기 위해
            return;

        ActiveCharacter enemyCharacter = collision.GetComponent<ActiveCharacter>();

        if (enemyCharacter != null && skillValue != null)
        {
            if (enemyCharacter.teamType == teamType || enemyCharacter.invincibility)
                return;

            enemyCharacter.Hit(new CharacterAttackParam((ENUM_ATTACKOBJECT_NAME)skillValue.skillType, reverseState));

            // 피격된 캐릭터 위치를 기준으로 주어진 범위 내의 랜덤위치로 조정
            Vector2 randomHitPosVec = collision.transform.position;
            randomHitPosVec.x += UnityEngine.Random.Range(-0.5f, 0.5f);
            randomHitPosVec.y += UnityEngine.Random.Range(-0.3f, 1.0f);

            // 이펙트 생성 ( 임시 랜덤 )
            int effectNum = UnityEngine.Random.Range(0, 3);
            Summon_EffectObject(effectNum, randomHitPosVec);

            int effectNum2 = UnityEngine.Random.Range(3, 5);
            Summon_EffectObject(effectNum2, collision.transform.position);
            
            Sync_DestroyMine();
        }
        else
        {
            // Debug.Log($"{gameObject.name}이 {collision.gameObject.name}을 감지했으나 Hit하지 못함");
        }
    }

    public void Summon_EffectObject(int _effectTypeNum, Vector2 _targetTr)
    {
        ENUM_EFFECTOBJECT_NAME effectObjectName = (ENUM_EFFECTOBJECT_NAME)_effectTypeNum;

        EffectObject effectObject;

        if (isServerSyncState)
            effectObject = Managers.Resource.InstantiateEveryone("EffectObjects/"+effectObjectName.ToString(), _targetTr).GetComponent<EffectObject>();
        else
            effectObject = Managers.Resource.GetEffectObject(effectObjectName.ToString());

        if (effectObject != null)
        {
            if (isServerSyncState)
            {
                PhotonLogicHandler.Instance.TryBroadcastMethod<EffectObject, Vector2, bool>
                    (effectObject, effectObject.Activate_EffectObject, _targetTr, reverseState);
            }
            else
                effectObject.Activate_EffectObject(_targetTr, reverseState);
        }
        else
        {
            Debug.Log($"ENUM_EFFECTOBJECT_NAME에서 해당 번호를 찾을 수 없음 : {_effectTypeNum}");
        }
    }

    private IEnumerator IRunTimeCheck(float _runTime)
    {
        float realTime = 0.0f;

        while (realTime < _runTime && isUsing)
        {
            realTime += Time.deltaTime;

            yield return null;
        }

        runTimeCheckCoroutine = null;
        Sync_DestroyMine();
    }
}
