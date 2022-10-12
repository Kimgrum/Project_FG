using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FGDefine;

public class CreateRoomUI : MonoBehaviour
{
    [SerializeField] CustomRoomWindowUI customRoomWindow;

    [SerializeField] Image mapImage;
    [SerializeField] Text masterIDText;
    [SerializeField] Text personnelText;
    [SerializeField] InputField userInputField;

    ENUM_MAP_TYPE currMap
    {
        set
        {
            mapImage.sprite = Managers.Resource.Load<Sprite>($"Art/Sprites/Maps/{value}_M");
        }
    }

    private void OnEnable()
    {
        masterIDText.text = "닉네임 받아와야함";
        userInputField.text = "";
        currMap = ENUM_MAP_TYPE.BasicMap;
    }
    
    public void OnClick_CreatRoom()
    {
        Managers.UI.popupCanvas.Open_LoadingPopup();

        userInputField.text = userInputField.text.Trim();

        if(userInputField.text == "")
        {
            Managers.UI.popupCanvas.Open_NotifyPopup("방 제목을 입력하지 않았습니다.");
            return;
        }
        
        // 금지어 체크해야 함
    
        PhotonLogicHandler.Instance.TryCreateRoom(Open_CustomRoom, null, userInputField.text);
        Managers.UI.popupCanvas.Close_LoadingPopup();
    }

    private void Open_CustomRoom()
    {
        customRoomWindow.Open();
        this.gameObject.SetActive(false);
    }
}
