using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FGDefine;

public class MultiAttackObject : AttackObject
{
    protected AttackObject attackObject;

    [SerializeField] Vector3 subPos;

    public override void Init()
    {
        attackObjectType = ENUM_ATTACKOBJECT_TYPE.Multi;

        base.Init();

        if (isServerSyncState)
        {
            SyncTransformView(transform);
        }
    }

	[BroadcastMethod]
    public override void ActivatingAttackObject(ENUM_TEAM_TYPE _teamType, bool _reverseState)
    {
        base.ActivatingAttackObject(_teamType, _reverseState);

        if (reverseState)
        {
            transform.localEulerAngles = new Vector3(0, 180, 0);
            transform.position += new Vector3(subPos.x * -1.0f, subPos.y, 0);
        }
        else
        {
            transform.localEulerAngles = Vector3.zero;
            transform.position += subPos;
        }

        gameObject.SetActive(true);
    }

    protected void Summon_AttackObject(int _attackTypeNum)
    {
        bool isControl = true;
        
        if(isServerSyncState)
            isControl = PhotonLogicHandler.IsMine(viewID);

        if (!isControl) return;

        attackObject = null;
        ENUM_ATTACKOBJECT_NAME attackObjectName = (ENUM_ATTACKOBJECT_NAME)_attackTypeNum;

        if (isServerSyncState)
            attackObject = Managers.Resource.InstantiateEveryone(attackObjectName.ToString(), Vector2.zero).GetComponent<AttackObject>();
        else
            attackObject = Managers.Resource.GetAttackObject(attackObjectName.ToString());

        if (attackObject != null)
        {
            attackObject.Set_TargetTransform(this.transform);

            if (isServerSyncState)
            {
                PhotonLogicHandler.Instance.TryBroadcastMethod<AttackObject, ENUM_TEAM_TYPE, bool>
                    (attackObject, attackObject.ActivatingAttackObject, teamType, reverseState);
            }
            else
                attackObject.ActivatingAttackObject(teamType, reverseState);
        }
        else
        {
            Debug.Log($"ENUM_SKILL_TYPE에서 해당 번호를 찾을 수 없음 : {_attackTypeNum}");
        }
    }

    public void AnimEvent_DestoryMine()
    {
        Managers.Resource.Destroy(this.gameObject);
    }
}
