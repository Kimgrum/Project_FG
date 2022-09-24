/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 현재로써는 모든 행동중에 누르면 쿨타임이 돌아감... 쿨타임 돌리는 시점 생각해봐야할 듯
public class SkillUI : UIElement
{
    public GameObject coolTime;
    public Image coolTimeImage;
    [SerializeField] Text text_CoolTime;

    public float time_cooltime = 2;
    private float time_current;
    private float time_start;

    void Update()
    {
        if (isEnded)
            return;
        Check_CoolTime();
    }

    public override void init()
    {
        base.init();

        coolTimeImage = coolTime.GetComponent<Image>();

        Init_UI();
        Trigger_Skill();
    }

    // 이미지 표기 방법 설정 (원, 위를 기준으로 360도로 채움)
    private void Init_UI()
    {
        coolTimeImage.type = Image.Type.Filled;
        coolTimeImage.fillMethod = Image.FillMethod.Radial360;
        coolTimeImage.fillOrigin = (int)Image.Origin360.Top;
        coolTimeImage.fillClockwise = false;
    }

    private void Trigger_Skill()
    {
        if (!isEnded)
        {
            return;
        }

        Reset_CoolTime();
    }

    private void Check_CoolTime()
    {
        time_current = Time.time - time_start;
        if (time_current < time_cooltime)
        {
            Set_FillAmount(time_cooltime - time_current);
        }
        else if (!isEnded)
        {
            End_CoolTime();
        }
    }

    // 쿨타임 종료
    private void End_CoolTime()
    {
        Set_FillAmount(0);
        isEnded = true;
        coolTime.gameObject.SetActive(false);
        this.GetComponent<Button>().interactable = true;
    }

    // 쿨타임 값 리셋
    private void Reset_CoolTime()
    {
        coolTime.gameObject.SetActive(false);
        time_current = time_cooltime;
        time_start = Time.time;
        Set_FillAmount(time_cooltime);
        isEnded = true;
    }

    // 쿨타임 시간, 게이지 표기
    private void Set_FillAmount(float _value)
    {
        coolTimeImage.fillAmount = _value / time_cooltime;
        string txt = _value.ToString("0.0");
        text_CoolTime.text = txt;
    }

    public void OnCoolTime()
    { 
        time_start = Time.time;
        isEnded = false;
        coolTime.gameObject.SetActive(true);
        this.GetComponent<Button>().interactable = false;
    }
}*/