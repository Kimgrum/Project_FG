using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FGDefine;
using System;

public class CharacterSelectArea : MonoBehaviour
{
    public void Init(Action<ENUM_CHARACTER_TYPE> _charSelectCallBack)
    {
        CharacterSelectElementUI characterSelectElementUI;

        for(int i = (int)ENUM_CHARACTER_TYPE.Default + 1; i < (int)ENUM_CHARACTER_TYPE.Max; i++)
        {
            characterSelectElementUI = Managers.Resource.Instantiate("UI/CharacterSelectElement", transform).GetComponent<CharacterSelectElementUI>();

            characterSelectElementUI.Init(_charSelectCallBack, (ENUM_CHARACTER_TYPE)i);
        }
    }
}