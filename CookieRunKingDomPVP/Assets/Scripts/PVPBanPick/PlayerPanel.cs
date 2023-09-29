using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerPanel : MonoBehaviour
{
	#region PublicMember
	public TextMeshProUGUI playerNameText;
	public List<CookieThumbnail> pickList;
	public System.Action<int> onClick = null;
	#endregion

	#region Public
	public void SetData(BanPickPlayerData data)
	{
		if (data != null)
		{
			// 플레이어 정보 텍스트 설정
			if (playerNameText != null)
			{
				playerNameText.text = string.Format("LV.{0} {1}", data.playerLevel.ToString("00"), data.playerName);
			}

			// 픽 리스트 설정
			for (int i = 0; i < pickList.Count; i++)
			{
				var pick = pickList[i];
				if (pick != null)
				{
					var needShow = i < data.pickList.Count && data.pickList[i] >= 0;
					pick.gameObject.SetActive(needShow);
					if (needShow)
					{
						pick.SetData(data.pickList[i]);
						pick.SetActive(true);
						pick.SetBan(data.pickList[i] == data.banCookie);
						pick.onClick = onClick;
					}
				}
			}
		}
	}
	#endregion
}
