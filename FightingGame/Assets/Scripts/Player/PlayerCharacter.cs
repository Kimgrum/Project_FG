using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FGDefine;
using System;

public class PlayerCharacter : MonoBehaviour
{
    public ActiveCharacter activeCharacter;
    [SerializeField] PlayerCamera playerCamera;

    public ENUM_TEAM_TYPE teamType;
    public float moveDir = 0f;
    public bool inabilityState = false;

    Coroutine moveCoroutine = null;

    bool isMove = false;


    private void Update()
    {
        if (activeCharacter == null)
            return;

        if (!PhotonLogicHandler.IsMine(activeCharacter.ViewID))
            return;

        OnKeyboard(); // 디버깅용
    }

    public void Set_Character(ActiveCharacter _activeCharacter)
    {
        activeCharacter = _activeCharacter;
        activeCharacter.transform.parent = this.transform;

        if (Managers.Battle.isServerSyncState)
        {
            PhotonLogicHandler.Instance.TryBroadcastMethod<Character, ENUM_TEAM_TYPE>(activeCharacter, activeCharacter.Set_TeamType, teamType);
            activeCharacter.Set_Character();
        }
        else
        {
            activeCharacter.teamType = teamType;
            activeCharacter.Init();
            activeCharacter.Set_Character();
        }

        playerCamera.Init(activeCharacter.transform);
        Connect_InputController();
    }

    public void Connect_InputController()
    {
        Managers.Input.Connect_InputKeyController(OnPointDownCallBack, OnPointUpCallBack);
        Managers.Input.Connect_InputArrowKey(OnPointEnterCallBack);
    }

    public void OnPointDownCallBack(ENUM_INPUTKEY_NAME _inputKeyName)
    {
        switch (_inputKeyName)
        {
            case ENUM_INPUTKEY_NAME.Direction:
                isMove = true;
                if (moveCoroutine == null)
                    moveCoroutine = StartCoroutine(IMove());
                break;
            case ENUM_INPUTKEY_NAME.Attack:
                CharacterAttackParam attackParam = new CharacterAttackParam(ENUM_ATTACKOBJECT_NAME.Knight_Attack1, activeCharacter.reverseState);
                PlayerCommand(ENUM_PLAYER_STATE.Attack, attackParam);
                activeCharacter.Change_AttackState(true);
                break;
            case ENUM_INPUTKEY_NAME.Skill1:
                CharacterSkillParam skillParam1 = new CharacterSkillParam(0);
                PlayerCommand(ENUM_PLAYER_STATE.Skill, skillParam1);
                break;
            case ENUM_INPUTKEY_NAME.Skill2:
                CharacterSkillParam skillParam2 = new CharacterSkillParam(1);
                PlayerCommand(ENUM_PLAYER_STATE.Skill, skillParam2);
                break;
            case ENUM_INPUTKEY_NAME.Skill3:
                CharacterSkillParam skillParam3 = new CharacterSkillParam(2);
                PlayerCommand(ENUM_PLAYER_STATE.Skill, skillParam3);
                break;
            case ENUM_INPUTKEY_NAME.Jump:
                PlayerCommand(ENUM_PLAYER_STATE.Jump);
                break;
        }
    }

    public void OnPointUpCallBack(ENUM_INPUTKEY_NAME _inputKeyName)
    {
        switch (_inputKeyName)
        {
            case ENUM_INPUTKEY_NAME.Direction:
                isMove = false;
                break;
            case ENUM_INPUTKEY_NAME.Attack:
                activeCharacter.Change_AttackState(false);
                break;
        }
    }

    public void OnPointEnterCallBack(float _moveDir)
    {
        moveDir = _moveDir;
    }

    // 디버깅용이니 쿨하게 다 때려박기
    private void OnKeyboard()
    {
        // 공격
        if (Input.GetKeyDown(KeyCode.F))
        {
            CharacterAttackParam attackParam = new CharacterAttackParam(ENUM_ATTACKOBJECT_NAME.Knight_Attack1, activeCharacter.reverseState);
            PlayerCommand(ENUM_PLAYER_STATE.Attack, attackParam);
            activeCharacter.Change_AttackState(true); 
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            activeCharacter.Change_AttackState(false);
        }

        // 스킬 1번
        if (Input.GetKeyDown(KeyCode.R))
        {
            CharacterSkillParam skillParam = new CharacterSkillParam(0);
            PlayerCommand(ENUM_PLAYER_STATE.Skill, skillParam);
        }

        // 스킬 2번
        if (Input.GetKeyDown(KeyCode.T))
        {
            CharacterSkillParam skillParam = new CharacterSkillParam(1);
            PlayerCommand(ENUM_PLAYER_STATE.Skill, skillParam);
        }

        // 스킬 3번
        if (Input.GetKeyDown(KeyCode.Y))
        {
            CharacterSkillParam skillParam = new CharacterSkillParam(2);
            PlayerCommand(ENUM_PLAYER_STATE.Skill, skillParam);
        }

        // 점프
        if (Input.GetKeyDown(KeyCode.G))
        {
            PlayerCommand(ENUM_PLAYER_STATE.Jump);
        }

        // 이동
        if (Input.GetKeyDown(KeyCode.A))
        {
            moveDir = -1.0f;
            if (moveCoroutine == null)
                moveCoroutine = StartCoroutine(IMove());
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            moveDir = 1.0f;
            if (moveCoroutine == null)
                moveCoroutine = StartCoroutine(IMove());
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            isMove = false;
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
             isMove = false;
        }
    }
    
    public void PlayerCommand(ENUM_PLAYER_STATE nextState, CharacterParam param = null)
    {
        if (activeCharacter == null)
            return;

        if ( activeCharacter.currState == ENUM_PLAYER_STATE.Skill ||
            (activeCharacter.currState == ENUM_PLAYER_STATE.Hit || activeCharacter.currState == ENUM_PLAYER_STATE.Die))
            return;

        switch (nextState)
        {
            case ENUM_PLAYER_STATE.Idle:
                activeCharacter.Idle();
                break;
            case ENUM_PLAYER_STATE.Move:
                activeCharacter.Move(param);
                break;
            case ENUM_PLAYER_STATE.Jump:
                activeCharacter.Jump();
                break;
            case ENUM_PLAYER_STATE.Attack:
                activeCharacter.Attack(param);
                break;
            case ENUM_PLAYER_STATE.Skill:
                activeCharacter.Skill(param);
                break;
            case ENUM_PLAYER_STATE.Hit:
                Debug.Log("주의! PlayerCharacter에 Hit 명령이 들어옴");
                break;
            case ENUM_PLAYER_STATE.Die:
                activeCharacter.Die();
                break;
        }
    }

    protected IEnumerator IMove()
    {
        activeCharacter.Input_MoveKey(true);
        isMove = true;

        while (isMove)
        {
            PlayerCommand(ENUM_PLAYER_STATE.Move, new CharacterMoveParam(moveDir));
            yield return null;
        }

        activeCharacter.Input_MoveKey(false);
        if (activeCharacter.currState == ENUM_PLAYER_STATE.Move)
            PlayerCommand(ENUM_PLAYER_STATE.Idle);

        moveCoroutine = null;
    }
}