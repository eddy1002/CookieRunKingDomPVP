using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanPickData
{
	#region Enum
    public enum BANPICK_STEP
	{
		NONE = -1,
		PICK_1,
		PICK_1_2,
		PICK_2_3,
		PICK_3_4,
		PICK_4_5,
		PICK_5_6,
		PICK_6,
		BAN,
		BATTLE_START,
	}
	#endregion

	#region PublicMember
	public BanPickPlayerData myData;
    public BanPickPlayerData enemyData;
    public bool isMyTurn;
	public BANPICK_STEP step;
	public float remainSecends;
	#endregion

	#region Public
	public void ClearData()
	{
		if (myData != null)
		{
			myData.ClearData();
		}
		if (enemyData != null)
		{
			enemyData.ClearData();
		}
		isMyTurn = true;
		step = BANPICK_STEP.NONE;
		remainSecends = 0f;
	}
	#endregion
}
