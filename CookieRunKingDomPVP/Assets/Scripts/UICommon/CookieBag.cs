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
    /// ��Ű ���̵� ������� �������� Ȱ��ȭ�� ����
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isActive"></param>
    public void SetCookieThumbnailActiveByID(int id, bool isActive)
    {
        // ���̵� ��ȿ���� �˻�
        if (id >= 0 && id < cookieCount)
        {
            // ��ȯ�ϸ鼭 �˻�
            foreach (var item in itemList)
            {
                if (item != null && item.TryGetComponent<CookieBagItem>(out var bagItem))
                {
                    // �ش� �������� �����ϸ� ����
                    if (bagItem.listIndex == id && bagItem.TryGetComponent<CookieThumbnail>(out var thumbnail))
                    {
                        thumbnail.SetActive(isActive);
                    }
                }
            }
        }
    }

    /// <summary>
    /// ��� �������� Ȱ��ȭ�� Ŭ����
    /// </summary>
    public void ClearDeActive()
    {
        // ��ȯ�ϸ鼭 Ŭ����
        foreach (var item in itemList)
        {
            if (item != null && item.TryGetComponent<CookieThumbnail>(out var thumbnail))
            {
                thumbnail.SetActive(true);
            }
        }
    }

    /// <summary>
    /// ��Ȱ��ȭ ����Ʈ�� ����
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
    /// ���� �ʱ�ȭ
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
    /// ��ũ�� ����Ʈ �ʺ� ��ȯ
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
    /// ��ũ�� �������� �ʺ� ��ȯ
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
    /// ��ũ�� ������ ����Ʈ �ʱ�ȭ
    /// </summary>
    private void MakeItemList()
    {
        if (child != null && scrollRect != null)
        {
            // ������ �����
            child.gameObject.SetActive(false);

            var width = GetWidth();
            var cellWidth = GetItemSize() + space;
            if (cellWidth > 0f)
            {
                // �ʿ��� ������ ���� ���
                var count = Mathf.CeilToInt(width / cellWidth) + 3;

                // ���� ���� ���ʿ��� �������� �ִٸ� ����
                var safeCount = 0;
                while (safeCount < count && itemList.Count > count)
                {
                    var removeItem = itemList[^1];
                    itemList.Remove(removeItem);
                    Destroy(removeItem.gameObject);
                    safeCount++;
                }

                // �����ϸ� ������ ����
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

                    // ������ ũ�� ����
                    scrollRect.content.sizeDelta = new Vector2(cellWidth * cookieCount - space, scrollRect.content.sizeDelta.y);
                }
            }
        }
    }

    /// <summary>
    /// �������� index�� ���� ���� ��ũ�� ��ġ�� ������� ��ġ �� ���� ����
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
                    // ��ũ�� X��ǥ ��� ��ȯ
                    var scrollX = -scrollRect.content.localPosition.x;
                    // ������ ũ�⸦ ������ �ε��� ��ȯ
                    var point = scrollX / cellWidth - index;
                    var diff = Mathf.FloorToInt(point / itemList.Count) + 1;
                    // ���� ������ �ε��� ���
                    var listIndex = index - 2 + diff * itemList.Count;

                    if (item.TryGetComponent<CookieBagItem>(out var bagItem))
                    {
                        if (listIndex < 0 || bagItem.listIndex != listIndex)
                        {
                            bagItem.listIndex = listIndex;

                            // ��ũ�� ������ ��ġ ����
                            item.localPosition = new Vector2(listIndex * cellWidth, 0f);

                            // ��Ű ������ ����
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
    /// ��� �������� �� index�� ���� ���� ��ũ�� ��ġ�� ������� ��ġ �� ���� ����
    /// </summary>
    private void ApplyItemList()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            ApplyItem(i);
        }
    }

    /// <summary>
    /// ��ũ�� �ݹ� �Լ�
    /// </summary>
    /// <param name="value"></param>
    private void OnValueChanged(Vector2 value)
    {
        ApplyItemList();
    }
    #endregion
}
