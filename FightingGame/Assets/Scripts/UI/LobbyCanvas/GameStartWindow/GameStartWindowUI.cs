using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FGDefine;

public class GameStartWindowUI : MonoBehaviour, IRoomPostProcess
{
    [SerializeField] UserInfoUI masterInfoUI;
    [SerializeField] UserInfoUI slaveInfoUI;

    UserInfoUI MyInfoUI
    {
        get
        {
            if(PhotonLogicHandler.IsMasterClient)
                return masterInfoUI;
            else
                return slaveInfoUI;
        }
    }
    UserInfoUI EnemyInfoUI
    {
        get
        {
            if (PhotonLogicHandler.IsMasterClient)
                return slaveInfoUI;
            else
                return masterInfoUI;
        }
    }

    [SerializeField] CharacterSelectArea characterSelectArea;

    [SerializeField] Text timerText;

    ENUM_CHARACTER_TYPE enemySelectionCharacterType = ENUM_CHARACTER_TYPE.Default;

    Coroutine settingInfoCheckCoroutine = null;
    Coroutine waitSelectionCharacterCoroutine = null;
    Coroutine waitGameStartCoroutine = null;

    private void OnDisable()
    {
        this.UnregisterRoomCallback();

        if(settingInfoCheckCoroutine != null)
            StopCoroutine(settingInfoCheckCoroutine);
        if (waitSelectionCharacterCoroutine != null)
            StopCoroutine(waitSelectionCharacterCoroutine);
        if (waitGameStartCoroutine != null)
            StopCoroutine(waitGameStartCoroutine);
    }
    
    public void Open()
    {
        if (this.gameObject.activeSelf)
            return;

        this.RegisterRoomCallback();

        characterSelectArea.Init(CallBack_SelectionCharacter);
        MyInfoUI.Active_SelectionCompleteBtn(CallBack_SelectionCharacterComplete);

        this.gameObject.SetActive(true);

        settingInfoCheckCoroutine = StartCoroutine(ISettingInfoCheck());
    }
    
    public void GameStart()
    {
        EnemyInfoUI.Set_SelectionCharacter(enemySelectionCharacterType);
        MyInfoUI.ChangeInfo_GameStart();
        EnemyInfoUI.ChangeInfo_GameStart();

        waitGameStartCoroutine = StartCoroutine(IWaitGameStart(3.0f));
    }

    public void OnUpdateRoomProperty(CustomRoomProperty property) { }
    public void OnUpdateRoomPlayerProperty(CustomPlayerProperty property)
    {
        if (property.isMasterClient)
            masterInfoUI.Init(property.data);
        else
            slaveInfoUI.Init(property.data);

        // 상대방의 캐릭터 선택 완료됨을 변수화해서 담음
        if (property.characterType != ENUM_CHARACTER_TYPE.Default
            && (PhotonLogicHandler.IsMasterClient != property.isMasterClient))
        {
            enemySelectionCharacterType = property.characterType;
        }
    }

    public void CallBack_SelectionCharacter(ENUM_CHARACTER_TYPE _selectedCharType)
    {
        MyInfoUI.Set_SelectionCharacter(_selectedCharType);
    }

    public void CallBack_SelectionCharacterComplete(ENUM_CHARACTER_TYPE _selectedCharacterType)
    {
        MyInfoUI.Deactive_SelectionCompleteBtn();
        characterSelectArea.Close();

        if (PhotonLogicHandler.IsMasterClient)
            PhotonLogicHandler.Instance.ChangeCharacter(_selectedCharacterType);
        else // 슬레이브라면, 마스터의 캐릭터 선택을 확인한다.
            waitSelectionCharacterCoroutine = StartCoroutine(IWaitSelectionCharacter(_selectedCharacterType));
    }

    private IEnumerator ISettingInfoCheck()
    {
        PhotonLogicHandler.Instance.RequestEveryPlayerProperty();

        yield return new WaitUntil(() => masterInfoUI.IsInit && slaveInfoUI.IsInit);

        Managers.UI.popupCanvas.Play_FadeInEffect();
        settingInfoCheckCoroutine = null;
    }
    
    private IEnumerator IWaitSelectionCharacter(ENUM_CHARACTER_TYPE _selectedCharacterType)
    {
        if(PhotonLogicHandler.IsMasterClient)
        {
            waitSelectionCharacterCoroutine = null;
            yield break;
        }

        yield return new WaitUntil(() => enemySelectionCharacterType != ENUM_CHARACTER_TYPE.Default);

        PhotonLogicHandler.Instance.ChangeCharacter(_selectedCharacterType);
        waitSelectionCharacterCoroutine = null;
    }

    private IEnumerator IWaitGameStart(float _delayTime)
    {
        yield return new WaitForSeconds(_delayTime);

        Managers.UI.popupCanvas.Play_FadeOutEffect();

        if(PhotonLogicHandler.IsMasterClient)
        {
            yield return new WaitForSeconds(1.0f);

            Managers.Scene.Sync_LoadScene(ENUM_SCENE_TYPE.Battle);
        }

        waitGameStartCoroutine = null;
    }
}
