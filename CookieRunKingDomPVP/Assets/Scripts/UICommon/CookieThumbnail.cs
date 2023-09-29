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
	/// ID�� ��Ű�� ������ �����ϱ�
	/// </summary>
	/// <param name="id"></param>
	public void SetData(int id)
	{
		cookieID = id;
		// ��Ű�� �̸� ����
		if (cookieNameText != null)
		{
			cookieNameText.text = string.Format("Cookie_{0}", cookieID.ToString("00"));
		}
		// ��ư Ŭ�� �ݹ� ����
		if (cookieButton != null)
		{
			cookieButton.onClick.RemoveAllListeners();
			cookieButton.onClick.AddListener(() => { onClick?.Invoke(cookieID); });
		}
	}

	/// <summary>
	/// Ȱ��ȭ ���� �����ϱ�
	/// </summary>
	/// <param name="active"></param>
	public void SetActive(bool active)
	{
		if (isActive != active)
		{
			isActive = active;
			// ��Ű�� �ʻ�ȭ ����
			if (cookieIcon != null)
			{
				cookieIcon.color = isActive ? Color.white : Color.black;
			}
			// ������ ��� ����
			if (background != null)
			{
				background.color = isActive ? Color.white : Color.gray;
			}
		}
	}

	/// <summary>
	/// ��Ű�� ���� ���� ����
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
