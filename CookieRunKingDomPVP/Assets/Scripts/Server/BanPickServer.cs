using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanPickServer : MonoBehaviour
{
    #region PublicMember
    public BanPickData data;
    public BanPickUI ui;
    public bool isStart = false;
    public float nextStepTime = 0f;
    public float aiTimer = 5f;
    public List<float> remainTimeList = new List<float>();
    #endregion

    #region PrivateMember
    private float aiWait = 0f;
    private readonly List<int> randomList = new List<int>();
    private bool myReady = false;
    private bool enemyReady = false;
    #endregion

    #region Mono
    private void Start()
	{
        BanPickStart();
        SendBanPickData();
    }

    private void Update()
    {
        // 제한 시간 갱신
        if (isStart && Time.realtimeSinceStartup > nextStepTime)
        {
            NextStep();
        }

		// AI 타이머 갱신
		if (aiWait < aiTimer)
		{
            aiWait += Time.deltaTime;
            if (aiWait >= aiTimer)
            {
                DoAI();
            }
		}
    }
    #endregion

    #region Packet
    /// <summary>
    /// OK 버튼 클릭
    /// </summary>
    /// <param name="isMe"></param>
    public void ResponseOK(bool isMe)
    {
        if (isMe)
        {
            myReady = true;
        }
		else
		{
            enemyReady = true;
		}

        // 모두 OK한 상황에는 바로 다음 단계로 전환
        if (myReady && enemyReady)
		{
            NextStep();
        }
    }

    /// <summary>
    /// 클라이언트에게 데이터 전송
    /// </summary>
    private void SendBanPickData()
    {
        if (ui != null)
        {
            ui.ResponseBanPickData(data);
        }
    }
    #endregion

    #region Private
    /// <summary>
    /// 밴픽 시작
    /// </summary>
    private void BanPickStart()
    {
        // 데이터 생성
        if (data == null)
        {
            data = new BanPickData();
        }

        // 데이터 초기화
        data.ClearData();

        // 플레이어 설정
        if (data.myData == null)
        {
            data.myData = new BanPickPlayerData();
        }
        data.myData.ClearData();
        data.myData.playerLevel = Random.Range(20, 40);
        data.myData.playerName = "Player";

        // 적플레이어 설정
        if (data.enemyData == null)
        {
            data.enemyData = new BanPickPlayerData();
        }
        data.enemyData.ClearData();
        data.enemyData.playerLevel = Random.Range(20, 40);
        data.enemyData.playerName = "Enemy";

        // 밴픽 단계 설정
        data.step = BanPickData.BANPICK_STEP.PICK_1;

        // 선턴 랜덤 설정
        data.isMyTurn = Random.value > 0.5f;

        // 제한 시간 설정
        SetTimerByStep(data.step);

        // 시작 여부 설정
        if (nextStepTime > 0f)
        {
            isStart = true;
            myReady = !data.isMyTurn;
            enemyReady = data.isMyTurn;
        }
    }

    /// <summary>
    /// 밴픽 제한 시간 설정(초)
    /// </summary>
    /// <param name="time"></param>
    private void SetTimer(float time)
    {
        if (data != null)
        {
            data.remainSecends = Time.realtimeSinceStartup + time;
            nextStepTime = Time.realtimeSinceStartup + time;
        }
    }

    /// <summary>
    /// 밴픽 단계를 기준으로 제한 시간 설정(초)
    /// </summary>
    /// <param name="step"></param>
    private void SetTimerByStep(BanPickData.BANPICK_STEP step)
    {
        int intStep = (int)data.step;
        if (remainTimeList != null && remainTimeList.Count > intStep)
        {
            SetTimer(remainTimeList[intStep]);
        }
        else
        {
            SetTimer(120f);
        }
    }

    /// <summary>
    /// 밴픽 단계 증가
    /// </summary>
    private void NextStep()
	{
        if (data != null)
        {
            if (data.step < BanPickData.BANPICK_STEP.BATTLE_START)
            {
                // 픽 단계에서 플레이어가 고르지 않은 것은 랜덤 선택
                if (data.isMyTurn && data.step < BanPickData.BANPICK_STEP.BAN)
                {
                    var intStep = (int)data.step;
                    if (intStep >= 0 && intStep < ui.pickIndexStart.Count && intStep < ui.pickIndexEnd.Count)
                    {
                        var start = ui.pickIndexStart[intStep];
                        var end = ui.pickIndexEnd[intStep];

                        if (data.myData.pickList[start] < 0)
                        {
                            data.myData.pickList[start] = GetRandomCookieNonSelected();
                        }
                        if (data.myData.pickList[end] < 0)
                        {
                            data.myData.pickList[end] = GetRandomCookieNonSelected();
                        }
                    }
				}
                // 밴 단계에서 플레이어가 선택하지 않으면 랜덤 선택
                else if (data.step == BanPickData.BANPICK_STEP.BAN)
                {
                    if (data.enemyData.banCookie < 0)
                    {
                        data.enemyData.banCookie = data.enemyData.pickList[Random.Range(0, data.enemyData.pickList.Count)];
                    }
                }

                // 단계 증가
                data.step++;

                // 턴 전환
                if (data.step < BanPickData.BANPICK_STEP.BAN)
                {
                    data.isMyTurn = !data.isMyTurn;
                    myReady = !data.isMyTurn;
                    enemyReady = data.isMyTurn;
                }
                else
                {
                    data.isMyTurn = true;
                    myReady = enemyReady = false;
                }

                // 시간 제한 설정
                SetTimerByStep(data.step);

                // 클라에 정보 전송
                SendBanPickData();

                // AI 타이머 초기화
                aiWait = 0;
            }
            else
            {
                isStart = false;
            }
		}
	}

    /// <summary>
    /// 선택되지 않는 쿠키를 랜덤으로 반환
    /// </summary>
    /// <returns></returns>
    private int GetRandomCookieNonSelected()
    {
        if (ui != null)
        {
            randomList.Clear();
            for (int i = 0; i < ui.cookieBag.cookieCount; i++)
            {
                if (!data.myData.pickList.Contains(i) && !data.enemyData.pickList.Contains(i))
                {
                    randomList.Add(i);
                }
            }
            if (randomList.Count > 0)
            {
                return randomList[Random.Range(0, randomList.Count)];
            }
        }
        return -1;
    }

    /// <summary>
    /// AI 수행
    /// </summary>
    private void DoAI()
	{
		if (data != null)
		{
            // 픽 단계
            if (data.step < BanPickData.BANPICK_STEP.BAN && !data.isMyTurn)
            {
                var intStep = (int)data.step;
                if (intStep >= 0 && intStep < ui.pickIndexStart.Count && intStep < ui.pickIndexEnd.Count)
                {
                    var start = ui.pickIndexStart[intStep];
                    var end = ui.pickIndexEnd[intStep];

                    if (data.enemyData.pickList[start] < 0)
                    {
                        data.enemyData.pickList[start] = GetRandomCookieNonSelected();
                    }
                    if (data.enemyData.pickList[end] < 0)
                    {
                        data.enemyData.pickList[end] = GetRandomCookieNonSelected();
                    }
                }
                ResponseOK(false);
            }
            // 밴 단계
            else if (data.step == BanPickData.BANPICK_STEP.BAN)
            {
                data.myData.banCookie = data.myData.pickList[Random.Range(0, data.myData.pickList.Count)];
                ResponseOK(false);
            }
        }
	}
    #endregion
}
