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
        // ���� �ð� ����
        if (isStart && Time.realtimeSinceStartup > nextStepTime)
        {
            NextStep();
        }

		// AI Ÿ�̸� ����
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
    /// OK ��ư Ŭ��
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

        // ��� OK�� ��Ȳ���� �ٷ� ���� �ܰ�� ��ȯ
        if (myReady && enemyReady)
		{
            NextStep();
        }
    }

    /// <summary>
    /// Ŭ���̾�Ʈ���� ������ ����
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
    /// ���� ����
    /// </summary>
    private void BanPickStart()
    {
        // ������ ����
        if (data == null)
        {
            data = new BanPickData();
        }

        // ������ �ʱ�ȭ
        data.ClearData();

        // �÷��̾� ����
        if (data.myData == null)
        {
            data.myData = new BanPickPlayerData();
        }
        data.myData.ClearData();
        data.myData.playerLevel = Random.Range(20, 40);
        data.myData.playerName = "Player";

        // ���÷��̾� ����
        if (data.enemyData == null)
        {
            data.enemyData = new BanPickPlayerData();
        }
        data.enemyData.ClearData();
        data.enemyData.playerLevel = Random.Range(20, 40);
        data.enemyData.playerName = "Enemy";

        // ���� �ܰ� ����
        data.step = BanPickData.BANPICK_STEP.PICK_1;

        // ���� ���� ����
        data.isMyTurn = Random.value > 0.5f;

        // ���� �ð� ����
        SetTimerByStep(data.step);

        // ���� ���� ����
        if (nextStepTime > 0f)
        {
            isStart = true;
            myReady = !data.isMyTurn;
            enemyReady = data.isMyTurn;
        }
    }

    /// <summary>
    /// ���� ���� �ð� ����(��)
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
    /// ���� �ܰ踦 �������� ���� �ð� ����(��)
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
    /// ���� �ܰ� ����
    /// </summary>
    private void NextStep()
	{
        if (data != null)
        {
            if (data.step < BanPickData.BANPICK_STEP.BATTLE_START)
            {
                // �� �ܰ迡�� �÷��̾ ���� ���� ���� ���� ����
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
                // �� �ܰ迡�� �÷��̾ �������� ������ ���� ����
                else if (data.step == BanPickData.BANPICK_STEP.BAN)
                {
                    if (data.enemyData.banCookie < 0)
                    {
                        data.enemyData.banCookie = data.enemyData.pickList[Random.Range(0, data.enemyData.pickList.Count)];
                    }
                }

                // �ܰ� ����
                data.step++;

                // �� ��ȯ
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

                // �ð� ���� ����
                SetTimerByStep(data.step);

                // Ŭ�� ���� ����
                SendBanPickData();

                // AI Ÿ�̸� �ʱ�ȭ
                aiWait = 0;
            }
            else
            {
                isStart = false;
            }
		}
	}

    /// <summary>
    /// ���õ��� �ʴ� ��Ű�� �������� ��ȯ
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
    /// AI ����
    /// </summary>
    private void DoAI()
	{
		if (data != null)
		{
            // �� �ܰ�
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
            // �� �ܰ�
            else if (data.step == BanPickData.BANPICK_STEP.BAN)
            {
                data.myData.banCookie = data.myData.pickList[Random.Range(0, data.myData.pickList.Count)];
                ResponseOK(false);
            }
        }
	}
    #endregion
}
