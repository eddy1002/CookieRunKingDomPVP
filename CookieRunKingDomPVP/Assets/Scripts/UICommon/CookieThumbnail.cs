using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookieThumbnail : MonoBehaviour
{
	#region PublicMember
	public bool isActive;
	public int cookieID;
	public TextMeshProUGUI cookieNameText;
	public Image cookieIcon;
	public Image background;
	public Button cookieButton;
	public GameObject cookieBan;
	public System.Action<int> onClick = null;
	#endregion

	#region Public
	/// <summary>
	/// ID로 쿠키런 섬네일 설정하기
	/// </summary>
	/// <param name="id"></param>
	public void SetData(int id)
	{
		cookieID = id;
		// 쿠키런 이름 설정
		if (cookieNameText != null)
		{
			cookieNameText.text = string.Format("Cookie_{0}", cookieID.ToString("00"));
		}
		// 버튼 클릭 콜백 설정
		if (cookieButton != null)
		{
			cookieButton.onClick.RemoveAllListeners();
			cookieButton.onClick.AddListener(() => { onClick?.Invoke(cookieID); });
		}
	}

	/// <summary>
	/// 활성화 상태 변경하기
	/// </summary>
	/// <param name="active"></param>
	public void SetActive(bool active)
	{
		if (isActive != active)
		{
			isActive = active;
			// 쿠키런 초상화 설정
			if (cookieIcon != null)
			{
				cookieIcon.color = isActive ? Color.white : Color.black;
			}
			// 섬네일 배경 설정
			if (background != null)
			{
				background.color = isActive ? Color.white : Color.gray;
			}
		}
	}

	/// <summary>
	/// 쿠키런 금지 상태 설정
	/// </summary>
	/// <param name="isBan"></param>
	public void SetBan(bool isBan)
	{
		if (cookieBan != null && cookieBan.gameObject.activeSelf != isBan)
		{
			cookieBan.SetActive(isBan);
		}
	}
	#endregion
}
