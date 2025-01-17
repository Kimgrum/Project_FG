using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FGDefine;

public class SlaveProfileUI : BaseProfile
{
    [SerializeField] Text ReadyText;

    public bool IsReady
    {
        get;
        private set;
    }
    public override void Init(Profile_Info _profileInfo)
    {
        IsMine = !PhotonLogicHandler.IsMasterClient;

        base.Init(_profileInfo);
    }

    public void Set_ReadyState(bool _readyState)
    {
        if (IsReady == _readyState)
            return;

        IsReady = _readyState;

        if(IsReady)
        {
            ReadyText.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            ReadyText.color = new Color(1f, 1f, 1f, 0.4f);
        }

        if(IsMine)
        {
            if(IsReady)
            {
                PhotonLogicHandler.Instance.OnReady();
            }
            else
            {
                PhotonLogicHandler.Instance.OnUnReady();
            }
        }
    }

}
