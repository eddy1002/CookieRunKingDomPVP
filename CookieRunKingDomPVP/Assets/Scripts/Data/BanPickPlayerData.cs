using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanPickPlayerData
{
	#region PublicMember
	public string playerName;
	public int playerLevel;
	public List<int> pickList = new List<int>();
	public int banCookie = -1;
	#endregion

	#region Public
	public void ClearData()
	{
		pickList.Clear();
		for (int i = 0; i < 6; i++)
		{
			pickList.Add(-1);
		}
		banCookie = -1;
	}
	#endregion
}
