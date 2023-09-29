using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BanPickUI : MonoBehaviour
{
    #region PublicMember
    public PlayerPanel myPanel;
    public PlayerPanel enemyPanel;
    public TextMeshProUGUI remainTimeText;
    public TextMeshProUGUI systemMsgText;
    public Button okButton;
    public CookieBag cookieBag;
	public List<string> systemMsgListMy = new List<string>();
	public List<string> systemMsgListEnemy = new List<string>();
	public List<int> pickIndexStart = new List<int>();
	public List<int> pickIndexEnd = new List<int>();
	public BanPickServer server;
	#endregion

	#region PrivateMember
	private BanPickData data;
	private readonly List<int> deActiveList = new List<int>();
	#endregion

	#region Mono
	private void Start()
	{
		// ��ư �ݹ�
		if (myPanel != null)
		{
			myPanel.onClick = PickCookie;
		}
		if (enemyPanel != null)
		{
			enemyPanel.onClick = PickCookieBan;
		}
		if (cookieBag != null)
		{
			cookieBag.onClick = PickCookie;
		}
		if (okButton != null)
		{
			okButton.onClick.RemoveAllListeners();
			okButton.onClick.AddListener(SendOK);
		}
	}

	private void Update()
	{
		if (data != null && data.remainSecends >= Time.realtimeSinceStartup)
		{
			RefreshRemainTimer();
		}
	}
	#endregion

	#region Packet
	/// <summary>
	/// ������ ���� ���� �����͸� �޾Ƽ� ���� ����
	/// </summary>
	/// <param name="data"></param>
	public void ResponseBanPickData(BanPickData data)
	{
		this.data = data;
		Refresh();
	}

	/// <summary>
	/// ������ OK ��ư�� ���� ���� ����
	/// </summary>
	private void RequestOK()
	{
		if (data != null && data.isMyTurn && data.remainSecends >= Time.realtimeSinceStartup && data.step < BanPickData.BANPICK_STEP.BATTLE_START)
		{
			if (server != null)
			{
				// �� �ܰ迡�� ��� ������� �Ǵ�
				if (data.step < BanPickData.BANPICK_STEP.BAN)
				{
					var intStep = (int)data.step;
					if (intStep >= 0 && intStep < pickIndexStart.Count && intStep < pickIndexEnd.Count)
					{
						var start = pickIndexStart[intStep];
						var end = pickIndexEnd[intStep];

						if (data.myData.pickList[start] >= 0 && data.myData.pickList[end] >= 0)
						{
							if (okButton != null)
							{
								okButton.interactable = false;
							}
							server.ResponseOK(true);
						}
					}
				}
				// �� �ܰ迡�� ��� ��Ű�� �� �ߴ��� �Ǵ�
				else if (data.step == BanPickData.BANPICK_STEP.BAN)
				{
					if (data.enemyData.banCookie >= 0)
					{
						if (okButton != null)
						{
							okButton.interactable = false;
						}
						server.ResponseOK(true);
					}
				}
			}
		}
	}
	#endregion

	#region Private
	/// <summary>
	/// ���� �ð� �ؽ�Ʈ ����
	/// </summary>
	private void RefreshRemainTimer()
	{
		if (data != null && remainTimeText != null)
		{
			remainTimeText.text = Mathf.RoundToInt(data.remainSecends - Time.realtimeSinceStartup).ToString("00");
		}
	}

	/// <summary>
	/// ��ü���� UI ����
	/// </summary>
	private void Refresh()
	{
		if (data != null)
		{
			// �÷��̾� ����
			if (myPanel != null)
			{
				myPanel.SetData(data.myData);
			}

			// ���÷��̾� ����
			if (enemyPanel != null)
			{
				enemyPanel.SetData(data.enemyData);
			}

			// ��Ű ���� ����
			if (cookieBag != null)
			{
				deActiveList.Clear();

				// �÷��̾� �� ����Ʈ �߰�
				foreach (var cookie in data.myData.pickList)
				{
					deActiveList.Add(cookie);
				}

				// ���÷��̾� �� ����Ʈ �߰�
				foreach (var cookie in data.enemyData.pickList)
				{
					deActiveList.Add(cookie);
				}

				cookieBag.SetDeActiveList(deActiveList);
			}

			// ���� �ð� ����
			RefreshRemainTimer();

			// �ý��� �޽��� ����
			var intStep = (int)data.step;
			if (intStep >= 0 && intStep < systemMsgListMy.Count && intStep < systemMsgListEnemy.Count)
			{
				if (systemMsgText != null)
				{
					systemMsgText.text = data.isMyTurn ? systemMsgListMy[intStep] : systemMsgListEnemy[intStep];
				}
			}

			// Ȯ�� ��ư ����
			if (okButton != null)
			{
				okButton.gameObject.SetActive(data.step < BanPickData.BANPICK_STEP.BATTLE_START);
				okButton.interactable = data.isMyTurn;
			}
		}
	}

	/// <summary>
	/// ��Ű�� �� �ܰ迡�� ����
	/// </summary>
	/// <param name="id"></param>
	private void PickCookie(int id)
	{
		if (data != null && data.isMyTurn && data.remainSecends >= Time.realtimeSinceStartup && data.step < BanPickData.BANPICK_STEP.BAN)
		{
			// ���̵� ��ȿ üũ
			if (id < 0)
			{
				return;
			}

			// ������ �� ��Ű�� �Ұ�
			if (data.enemyData.pickList.Contains(id))
			{
				return;
			}

			var intStep = (int)data.step;
			if (intStep >= 0 && intStep < pickIndexStart.Count && intStep < pickIndexEnd.Count)
			{
				var start = pickIndexStart[intStep];
				var end = pickIndexEnd[intStep];

				// �̹� �� ��Ű
				if (data.myData.pickList.Contains(id))
				{
					// ���� �ܰ迡�� �� ��Ű�� ���� ���
					var index = data.myData.pickList.IndexOf(id);
					if (index == start || index == end)
					{
						data.myData.pickList[index] = -1;
						Refresh();
					}
				}
				// ������ ��ĭ�̸� �ش� ĭ�� ����
				else if (data.myData.pickList[start] < 0)
				{
					data.myData.pickList[start] = id;
					Refresh();
				}
				// ���� ��ĭ�̸� �ش� ĭ�� ����
				else if (data.myData.pickList[end] < 0)
				{
					data.myData.pickList[end] = id;
					Refresh();
				}
			}
		}
	}

	/// <summary>
	/// Ȯ�� ��ư �ݹ�
	/// </summary>
	private void SendOK()
	{
		RequestOK();
	}

	/// <summary>
	/// ���� ��Ű�� ��
	/// </summary>
	private void PickCookieBan(int id)
	{
		if (data != null && data.step == BanPickData.BANPICK_STEP.BAN && data.remainSecends >= Time.realtimeSinceStartup)
		{
			if (data.enemyData.pickList.Contains(id) && data.enemyData.banCookie != id)
			{
				data.enemyData.banCookie = id;
				Refresh();
			}
		}
	}
	#endregion
}
