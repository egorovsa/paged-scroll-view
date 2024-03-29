using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.PagedScrollView
{
    [ExecuteInEditMode]
    [AddComponentMenu("UI/PagedScrollView")]
    [RequireComponent(typeof(ScrollRectEx))]
    public class PagedScrollView : MonoBehaviour
    {
        [Range(100f, 1000f)]
        public float Threshold = 300;

        [Range(0f, 80f)]
        public float MinimalDistancePercent = 30;

        [Range(0f, 1f)]
        public float AnimationSpeed = 0.3f;

        [SerializeField]
        public UnityEvent<int> OnPageChange;

        private int RunTimePageIndex = 0;
        private LayoutElement layoutElement;
        private ScrollRect scrollRect;
        private bool isDragging = false;
        private float draggingTime = 0f;
        private float startPositionLeft = 0f;
        private float startPositionTop = 0f;
        private float horizontalPositionDelta = 0f;
        private float verticalPositionDelta = 0f;
        private int activePage;
        private int currentPagesCount = 0;

        public int ActivePage
        {
            get
            {
                return activePage;
            }

            set
            {
                content.anchoredPosition = GetChild(value).sizeDelta.x * value * Vector2.left;

                activePage = value;
            }
        }

        public int NumberOfPage
        {
            get => content.childCount;
        }

        int childCount
        {
            get => scrollRect.content.childCount - 1;
        }

        bool isFastSwiping
        {
            get => draggingTime <= Threshold / 1000;
        }

        float viewportWidth
        {
            get => scrollRect.viewport.rect.width;
        }

        float viewportHeight
        {
            get => scrollRect.viewport.rect.height;
        }

        bool isHorizontal
        {
            get => scrollRect.horizontal;
        }

        public RectTransform content
        {
            get => scrollRect.content;
        }

        public void GoToPage(int pageIndex)
        {
            RunTimePageIndex = pageIndex;
            OnPageChange.Invoke(RunTimePageIndex);

            StopAllCoroutines();
            StartCoroutine(AnimateContentTo(GetPagePositionByPageIndex(pageIndex)));
        }

        public void GoToPageForce(int pageIndex)
        {
            RunTimePageIndex = pageIndex;
            OnPageChange.Invoke(RunTimePageIndex);
            SetContentPosition(GetPagePositionByPageIndex(pageIndex));
        }

        public void SetIsDraggig(bool state)
        {
            isDragging = state;

            if (state)
            {
                startPositionLeft = scrollRect.content.localPosition.x;
                startPositionTop = scrollRect.content.localPosition.y;
            }
            else
            {
                FinishMovementAfterDragging();
            }
        }

        void Start()
        {
            scrollRect.onValueChanged.AddListener(HandleScrollRectValueChanged);
        }

        void OnEnable()
        {
            scrollRect = GetComponent<ScrollRectEx>();
            UpdateSize();
        }

        RectTransform GetChild(int i)
        {
            return content.GetChild(i).GetComponent<RectTransform>();
        }

        void HandleScrollRectValueChanged(Vector2 _)
        {
            if (!isDragging)
            {
                return;
            }

            if (isHorizontal)
            {
                var rectWidth = scrollRect.content.rect.width;
                var position = scrollRect.content.localPosition.x;

                horizontalPositionDelta = startPositionLeft - position;
            }

            if (scrollRect.vertical)
            {
                var rectHeight = scrollRect.content.rect.height;
                var position = scrollRect.content.localPosition.y;

                verticalPositionDelta = startPositionTop - position;
            }
        }

        void Update()
        {
            if (!scrollRect || !content || NumberOfPage == 0)
            {
                return;
            }

            if (currentPagesCount != NumberOfPage)
            {
                currentPagesCount = NumberOfPage;
                UpdateSize();
            }

            if (isDragging)
            {
                draggingTime += Time.deltaTime;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateSize();
        }

        void UpdateSize()
        {
            if (!scrollRect || !content || NumberOfPage == 0)
            {
                return;
            }

            foreach (RectTransform t in content)
            {
                t.anchorMin = Vector2.up;
                t.anchorMax = Vector2.up;
                t.pivot = Vector2.up;

                t.sizeDelta = new Vector2(viewportWidth, viewportHeight);

                t.anchoredPosition = isHorizontal
                    ? new Vector2(t.rect.width * t.GetSiblingIndex(), 0)
                    : new Vector2(0, -t.rect.height * t.GetSiblingIndex());
            }

            if (isHorizontal)
            {
                content.offsetMax = new Vector2(viewportWidth * (NumberOfPage - 1) + content.offsetMin.x, 0);
            }
            else
            {
                content.offsetMin = new Vector2(0, viewportHeight * (NumberOfPage - 1) + content.offsetMax.x);
            }

        }

        bool IsMinimalDistancePassed()
        {
            var ratio = isHorizontal
                ? horizontalPositionDelta / viewportWidth
                : verticalPositionDelta / viewportHeight;

            var passedDistance = Mathf.Abs(ratio) * 100;

            return passedDistance >= MinimalDistancePercent;
        }

        void UpdatePageIndex()
        {
            if (IsMinimalDistancePassed() || isFastSwiping)
            {
                var positionDelta = isHorizontal
                    ? horizontalPositionDelta
                    : verticalPositionDelta;

                var isNextPage = isHorizontal ? positionDelta > 0 : positionDelta < 0;

                if (isNextPage)
                {
                    RunTimePageIndex++;
                    RunTimePageIndex = RunTimePageIndex > childCount ? childCount : RunTimePageIndex;
                    OnPageChange.Invoke(RunTimePageIndex);
                }
                else
                {
                    RunTimePageIndex--;
                    RunTimePageIndex = RunTimePageIndex < 0 ? 0 : RunTimePageIndex;
                    OnPageChange.Invoke(RunTimePageIndex);
                }
            }
        }

        float GetPagePositionByPageIndex(int PageIndex)
        {
            var viewportUnit = isHorizontal ? viewportWidth : viewportHeight;
            var direction = isHorizontal ? -1 : 1;

            return viewportUnit * PageIndex * direction;
        }

        void SetContentPosition(float position)
        {
            var vectorPosition = isHorizontal
                  ? new Vector2(position, 0)
                  : new Vector2(0, position);

            scrollRect.content.transform.localPosition = vectorPosition;
        }

        void FinishMovementAfterDragging()
        {
            StopAllCoroutines();
            UpdatePageIndex();
            StartCoroutine(AnimateContentTo(GetPagePositionByPageIndex(RunTimePageIndex)));

            draggingTime = 0f;
        }

        IEnumerator AnimateContentTo(float endPosition)
        {
            var startPosition = isHorizontal
                ? scrollRect.content.transform.localPosition.x
                : scrollRect.content.transform.localPosition.y;

            float time = 0f;

            while (time <= 1.0f)
            {
                time += Time.deltaTime / AnimationSpeed;

                var newPosition = Mathf.Lerp(startPosition, endPosition, Mathf.SmoothStep(0f, 1f, time));

                SetContentPosition(newPosition);

                yield return null;
            }
        }
    }
}
