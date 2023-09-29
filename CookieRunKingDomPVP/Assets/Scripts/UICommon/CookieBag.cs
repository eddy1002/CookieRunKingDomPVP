using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookieBag : MonoBehaviour
{
    #region PublicMember
    public RectTransform canvas;
    public RectTransform rect;
    public ScrollRect scrollRect;
    public float space = 0f;
    public int cookieCount = 50;
    public RectTransform child;
    public System.Action<int> onClick = null;
    #endregion

    #region PrivateMember
    private List<int> deActiveList = new List<int>();
    private readonly List<RectTransform> itemList = new List<RectTransform>();
    private float lastWidth = 0f;
    #endregion

    // Update is called once per frame
    void Update()
    {
        InitBag();
    }

    #region Public
    /// <summary>
    /// 쿠키 아이디를 기반으로 섬네일의 활성화를 설정
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isActive"></param>
    public void SetCookieThumbnailActiveByID(int id, bool isActive)
    {
        // 아이디가 유효한지 검사
        if (id >= 0 && id < cookieCount)
        {
            // 순환하면서 검색
            foreach (var item in itemList)
            {
                if (item != null && item.TryGetComponent<CookieBagItem>(out var bagItem))
                {
                    // 해당 섬네일이 존재하면 갱신
                    if (bagItem.listIndex == id && bagItem.TryGetComponent<CookieThumbnail>(out var thumbnail))
                    {
                        thumbnail.SetActive(isActive);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 모든 섬네일의 활성화를 클리어
    /// </summary>
    public void ClearDeActive()
    {
        // 순환하면서 클리어
        foreach (var item in itemList)
        {
            if (item != null && item.TryGetComponent<CookieThumbnail>(out var thumbnail))
            {
                thumbnail.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 비활성화 리스트를 적용
    /// </summary>
    /// <param name="deActiveList"></param>
    public void SetDeActiveList(List<int> deActiveList)
    {
        this.deActiveList = deActiveList;

        ClearDeActive();

        foreach (var deActive in deActiveList)
        {
            SetCookieThumbnailActiveByID(deActive, false);
        }
    }
    #endregion

    #region Private
    /// <summary>
    /// 가방 초기화
    /// </summary>
    private void InitBag()
	{
        var width = GetWidth();
        if (lastWidth != width)
        {
            lastWidth = width;

            MakeItemList();
            ApplyItemList();

            if (scrollRect != null)
            {
                scrollRect.onValueChanged.RemoveAllListeners();
                scrollRect.onValueChanged.AddListener(OnValueChanged);
            }
        }
	}

    /// <summary>
    /// 스크롤 뷰포트 너비 반환
    /// </summary>
    /// <returns></returns>
    private float GetWidth()
    {
        if (canvas != null && rect != null && scrollRect != null && scrollRect.viewport != null)
        {
            if (scrollRect.TryGetComponent<RectTransform>(out var scroll))
            {
                return canvas.sizeDelta.x + rect.sizeDelta.x + scroll.sizeDelta.x + scrollRect.viewport.sizeDelta.x;
            }
        }
        return 0f;
    }

    /// <summary>
    /// 스크롤 아이템의 너비 반환
    /// </summary>
    /// <returns></returns>
    private float GetItemSize()
    {
        if (child != null)
        {
            return child.sizeDelta.x * child.localScale.x;
        }
        return 0f;
    }

    /// <summary>
    /// 스크롤 아이템 리스트 초기화
    /// </summary>
    private void MakeItemList()
    {
        if (child != null && scrollRect != null)
        {
            // 원본은 숨기기
            child.gameObject.SetActive(false);

            var width = GetWidth();
            var cellWidth = GetItemSize() + space;
            if (cellWidth > 0f)
            {
                // 필요한 아이템 갯수 계산
                var count = Mathf.CeilToInt(width / cellWidth) + 3;

                // 만약 현재 불필요한 아이템이 있다면 제거
                var safeCount = 0;
                while (safeCount < count && itemList.Count > count)
                {
                    var removeItem = itemList[^1];
                    itemList.Remove(removeItem);
                    Destroy(removeItem.gameObject);
                    safeCount++;
                }

                // 부족하면 아이템 생성
                if (scrollRect.content != null)
                {
                    safeCount = 0;
                    while (safeCount < count && itemList.Count < count)
                    {
                        var newItem = Instantiate(child.gameObject, scrollRect.content);
                        if (newItem != null && newItem.TryGetComponent<RectTransform>(out var newRect))
                        {
                            newItem.gameObject.SetActive(true);
                            itemList.Add(newRect);
                        }
                        safeCount++;
                    }

                    // 컨텐츠 크기 조정
                    scrollRect.content.sizeDelta = new Vector2(cellWidth * cookieCount - space, scrollRect.content.sizeDelta.y);
                }
            }
        }
    }

    /// <summary>
    /// 아이템을 index에 따라 현재 스크롤 위치에 기반으로 위치 및 정보 적용
    /// </summary>
    /// <param name="index"></param>
    private void ApplyItem(int index)
    {
        if (itemList.Count > index)
        {
            var item = itemList[index];

            if (item != null && scrollRect != null && scrollRect.content != null)
            {
                var cellWidth = (GetItemSize() + space);
                if (cellWidth > 0f)
                {
                    // 스크롤 X좌표 양수 변환
                    var scrollX = -scrollRect.content.localPosition.x;
                    // 아이템 크기를 단위로 인덱스 변환
                    var point = scrollX / cellWidth - index;
                    var diff = Mathf.FloorToInt(point / itemList.Count) + 1;
                    // 실제 아이템 인덱스 계산
                    var listIndex = index - 2 + diff * itemList.Count;

                    if (item.TryGetComponent<CookieBagItem>(out var bagItem))
                    {
                        if (listIndex < 0 || bagItem.listIndex != listIndex)
                        {
                            bagItem.listIndex = listIndex;

                            // 스크롤 아이템 위치 조정
                            item.localPosition = new Vector2(listIndex * cellWidth, 0f);

                            // 쿠키 섬네일 설정
                            var needShow = listIndex >= 0 && listIndex < cookieCount;
                            item.gameObject.SetActive(needShow);
                            if (needShow && item.TryGetComponent<CookieThumbnail>(out var thumbnail))
                            {
                                thumbnail.SetData(listIndex);
                                thumbnail.SetActive(!deActiveList.Contains(listIndex));
                                thumbnail.SetBan(false);
                                thumbnail.onClick = onClick;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 모든 아이템을 각 index에 따라 현재 스크롤 위치에 기반으로 위치 및 정보 적용
    /// </summary>
    private void ApplyItemList()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            ApplyItem(i);
        }
    }

    /// <summary>
    /// 스크롤 콜백 함수
    /// </summary>
    /// <param name="value"></param>
    private void OnValueChanged(Vector2 value)
    {
        ApplyItemList();
    }
    #endregion
}
