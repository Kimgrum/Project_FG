using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FGDefine;
public class LobbyCanvas : BaseCanvas
{
    [SerializeField] CustomMatchingUI customMatching;
    [SerializeField] MatchingWindowUI matchingWindow;

    public override void Init()
    {
        base.Init();
    }

    public void Set_InTheCustomRoom()
    {
        customMatching.gameObject.SetActive(true);

        customMatching.Set_InTheCustomRoom();
    }

    public void OnClick_CustomMathing()
    {
        if(PhotonLogicHandler.IsConnected)
        {
            customMatching.gameObject.SetActive(true);
        }
        else
        {
            Managers.UI.popupCanvas.Open_NotifyPopup("마스터 서버에 접속해있지 않습니다.");
        }
    }
    public void OnClick_Mathing() => matchingWindow.OnClick_Matching();
    public void OnClick_Training() => Managers.UI.popupCanvas.Open_SelectPopup
        (GoTo_TrainingScene, null, "훈련장에 입장하시겠습니까?");

    private void GoTo_TrainingScene()
    {
        Managers.Scene.LoadScene(ENUM_SCENE_TYPE.Training);
    }
}