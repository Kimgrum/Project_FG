using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGDefine
{

    [Serializable]
    public enum ENUM_LAYER_TYPE
    {
        /// <summary>
        /// ����� Built-In ���̾�
        /// </summary>
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2, // ����ĳ��Ʈ ����
        Water = 4, // �� �� Ư�� ��ֹ�
        UI = 5,

        /// <summary>
        /// ������� User Ŀ���� ���̾�
        /// </summary>
        Player = 6, // ����
    }

    /// <summary>
    /// �ش� ĳ���� Ÿ��
    /// </summary>
    public enum ENUM_CHARACTER_TYPE
    {
        Knight = 0,

        Max
    }

    public enum ENUM_EVENT_TYPE
    {
        Joystick = 0,

    }

    public enum ENUM_PLAYER_STATE
    {
        Idle,
        Move,
        Run,
        Jump,
        Attack,
        Interect,
        Die,

        Max
    }
}

