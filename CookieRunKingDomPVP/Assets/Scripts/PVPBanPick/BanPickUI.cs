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
		// 버튼 콜백
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
	/// 서버로 부터 밴픽 데이터를 받아서 정보 갱신
	/// </summary>
	/// <param name="data"></param>
	public void ResponseBanPickData(BanPickData data)
	{
		this.data = data;
		Refresh();
	}

	/// <summary>
	/// 서버로 OK 버튼을 누른 것을 전송
	/// </summary>
	private void RequestOK()
	{
		if (data != null && data.isMyTurn && data.remainSecends >= Time.realtimeSinceStartup && data.step < BanPickData.BANPICK_STEP.BATTLE_START)
		{
			if (server != null)
			{
				// 픽 단계에서 모두 골랐는지 판단
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
				// 밴 단계에서 상대 쿠키를 밴 했는지 판단
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
	/// 제한 시간 텍스트 갱신
	/// </summary>
	private void RefreshRemainTimer()
	{
		if (data != null && remainTimeText != null)
		{
			remainTimeText.text = Mathf.RoundToInt(data.remainSecends - Time.realtimeSinceStartup).ToString("00");
		}
	}

	/// <summary>
	/// 전체적인 UI 갱신
	/// </summary>
	private void Refresh()
	{
		if (data != null)
		{
			// 플레이어 갱신
			if (myPanel != null)
			{
				myPanel.SetData(data.myData);
			}

			// 적플레이어 갱신
			if (enemyPanel != null)
			{
				enemyPanel.SetData(data.enemyData);
			}

			// 쿠키 가방 갱신
			if (cookieBag != null)
			{
				deActiveList.Clear();

				// 플레이어 픽 리스트 추가
				foreach (var cookie in data.myData.pickList)
				{
					deActiveList.Add(cookie);
				}

				// 적플레이어 픽 리스트 추가
				foreach (var cookie in data.enemyData.pickList)
				{
					deActiveList.Add(cookie);
				}

				cookieBag.SetDeActiveList(deActiveList);
			}

			// 제한 시간 설정
			RefreshRemainTimer();

			// 시스템 메시지 설정
			var intStep = (int)data.step;
			if (intStep >= 0 && intStep < systemMsgListMy.Count && intStep < systemMsgListEnemy.Count)
			{
				if (systemMsgText != null)
				{
					systemMsgText.text = data.isMyTurn ? systemMsgListMy[intStep] : systemMsgListEnemy[intStep];
				}
			}

			// 확인 버튼 설정
			if (okButton != null)
			{
				okButton.gameObject.SetActive(data.step < BanPickData.BANPICK_STEP.BATTLE_START);
				okButton.interactable = data.isMyTurn;
			}
		}
	}

	/// <summary>
	/// 쿠키를 픽 단계에서 선택
	/// </summary>
	/// <param name="id"></param>
	private void PickCookie(int id)
	{
		if (data != null && data.isMyTurn && data.remainSecends >= Time.realtimeSinceStartup && data.step < BanPickData.BANPICK_STEP.BAN)
		{
			// 아이디 유효 체크
			if (id < 0)
			{
				return;
			}

			// 상대방이 고른 쿠키는 불가
			if (data.enemyData.pickList.Contains(id))
			{
				return;
			}

			var intStep = (int)data.step;
			if (intStep >= 0 && intStep < pickIndexStart.Count && intStep < pickIndexEnd.Count)
			{
				var start = pickIndexStart[intStep];
				var end = pickIndexEnd[intStep];

				// 이미 고른 쿠키
				if (data.myData.pickList.Contains(id))
				{
					// 현재 단계에서 고른 쿠키면 선택 취소
					var index = data.myData.pickList.IndexOf(id);
					if (index == start || index == end)
					{
						data.myData.pickList[index] = -1;
						Refresh();
					}
				}
				// 시작이 빈칸이면 해당 칸에 선택
				else if (data.myData.pickList[start] < 0)
				{
					data.myData.pickList[start] = id;
					Refresh();
				}
				// 끝이 빈칸이면 해당 칸에 선택
				else if (data.myData.pickList[end] < 0)
				{
					data.myData.pickList[end] = id;
					Refresh();
				}
			}
		}
	}

	/// <summary>
	/// 확인 버튼 콜백
	/// </summary>
	private void SendOK()
	{
		RequestOK();
	}

	/// <summary>
	/// 상대방 쿠키를 밴
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
