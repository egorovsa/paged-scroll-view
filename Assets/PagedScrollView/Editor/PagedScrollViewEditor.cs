using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.PagedScrollView
{
    [CustomEditor(typeof(PagedScrollView))]
    public class PagedScrollViewEditor : Editor
    {
        static PagedScrollView _PagedScrollView;

        SerializedProperty Threshold;
        SerializedProperty MinimalDistancePercent;
        SerializedProperty AnimationSpeed;
        SerializedProperty OnPageChange;

        private void OnEnable()
        {
            _PagedScrollView = target as PagedScrollView;

            Threshold = serializedObject.FindProperty("Threshold");
            MinimalDistancePercent = serializedObject.FindProperty("MinimalDistancePercent");
            AnimationSpeed = serializedObject.FindProperty("AnimationSpeed");
            OnPageChange = serializedObject.FindProperty("OnPageChange");
        }

        void HeaderInformation()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle style = new GUIStyle
            {
                fontSize = 30,

                alignment = TextAnchor.MiddleCenter,
            };

            GUILayout.Label("Paged Scroll View", style);

            GUILayout.EndVertical();
        }

        void NewPageLayout(out GameObject newPageGO)
        {
            string pageName = string.Format("page_{0}", _PagedScrollView.NumberOfPage);
            newPageGO = new GameObject(pageName, typeof(RectTransform), typeof(Image));

            RectTransform rectTransformNewPageGO = newPageGO.GetComponent<RectTransform>();

            rectTransformNewPageGO.anchorMin = Vector2.up;
            rectTransformNewPageGO.anchorMax = Vector2.up;
            rectTransformNewPageGO.pivot = Vector2.up;

            Image imgNewPageGO = newPageGO.GetComponent<Image>();

            imgNewPageGO.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            imgNewPageGO.type = Image.Type.Sliced;

            GameObjectUtility.SetParentAndAlign(newPageGO, _PagedScrollView.content.gameObject);
        }

        void CreateNewPage()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 25,
                alignment = TextAnchor.MiddleCenter,
            };

            if (GUILayout.Button("Add Page", style, GUILayout.Height(100)))
            {
                NewPageLayout(out GameObject newPageGO);

                Undo.RegisterCreatedObjectUndo(newPageGO, "Create " + newPageGO.name);
            }

            GUILayout.EndHorizontal();
        }

        [MenuItem("GameObject/UI/Paged Scroll View/Not Paged Scroll", false)]
        static void CreateVerticalScrollView()
        {
            //ScrollViewGO
            Type[] components = new Type[] { typeof(RectTransform), typeof(Image), typeof(ScrollRectEx) };
            GameObject scrollViewGO = new GameObject("Scroll View (Not Paged Scroll)", components);

            RectTransform rectTransform = scrollViewGO.GetComponent<RectTransform>();
            StretchRectTransform(rectTransform);

            Image imgComponent = scrollViewGO.GetComponent<Image>();
            imgComponent.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            imgComponent.color = new Color(1, 1, 1, 0);
            imgComponent.type = Image.Type.Sliced;
            ScrollRectEx scrollRectEx = scrollViewGO.GetComponent<ScrollRectEx>();
            scrollRectEx.vertical = true;
            scrollRectEx.horizontal = true;

            GameObjectUtility.SetParentAndAlign(scrollViewGO, Selection.activeGameObject);

            //Viewport
            components = new Type[] { typeof(RectTransform), typeof(Image), typeof(Mask) };

            GameObject viewportGO = new GameObject("Viewport", components);

            RectTransform ViewportRectTransform = viewportGO.GetComponent<RectTransform>();
            ViewportRectTransform.anchorMin = Vector2.zero;
            ViewportRectTransform.anchorMax = Vector2.one;
            ViewportRectTransform.pivot = Vector2.up;
            ViewportRectTransform.offsetMin = Vector2.zero;
            ViewportRectTransform.offsetMax = Vector2.zero;

            Image imgViewportGO = viewportGO.GetComponent<Image>();
            imgViewportGO.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            imgViewportGO.type = Image.Type.Sliced;

            Mask maskviewportGO = viewportGO.GetComponent<Mask>();
            maskviewportGO.showMaskGraphic = false;

            GameObjectUtility.SetParentAndAlign(viewportGO, scrollViewGO);

            // Content
            components = new Type[] { typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter) };

            GameObject contentGO = new GameObject("Content", components);

            RectTransform rectTransformContentGO = contentGO.GetComponent<RectTransform>();
            rectTransformContentGO.anchorMin = Vector2.up;
            rectTransformContentGO.anchorMax = Vector2.one;
            rectTransformContentGO.pivot = Vector2.up;
            rectTransformContentGO.offsetMin = Vector2.zero;
            rectTransformContentGO.offsetMax = Vector2.zero;

            VerticalLayoutGroup verticalLayoutGroupContentGO = contentGO.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroupContentGO.childControlWidth = true;
            verticalLayoutGroupContentGO.spacing = 6;

            int numberOfInnerGO = 25;
            for (int i = 0; i < numberOfInnerGO; i++)
            {
                GameObject InnerGO = new GameObject("InnerElement", typeof(RectTransform), typeof(Image));
                GameObjectUtility.SetParentAndAlign(InnerGO, contentGO);
            }

            ContentSizeFitter contentSizeFitterContentGO = contentGO.GetComponent<ContentSizeFitter>();
            contentSizeFitterContentGO.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObjectUtility.SetParentAndAlign(contentGO, viewportGO);

            scrollRectEx.content = rectTransformContentGO;
            scrollRectEx.viewport = ViewportRectTransform;

            Undo.RegisterCreatedObjectUndo(scrollViewGO, "Create " + scrollViewGO.name);
        }

        static void StretchRectTransform(RectTransform target)
        {
            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.pivot = new Vector2(0.5f, 0.5f);
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }

        static void CreateScrollViewLayouts(GameObject parent, Vector2 dir, out GameObject pagedScrollView)
        {
            //  PagedScrollView
            pagedScrollView = new GameObject("Paged Scroll View");
            RectTransform pagedScrollViewRectTransform = pagedScrollView.AddComponent<RectTransform>();
            StretchRectTransform(pagedScrollViewRectTransform);
            ScrollRectEx scrollSnapScrollRect = pagedScrollView.AddComponent<ScrollRectEx>();
            scrollSnapScrollRect.horizontal = dir == Vector2.right || dir == Vector2.one;
            scrollSnapScrollRect.vertical = dir == Vector2.up || dir == Vector2.one;
            scrollSnapScrollRect.elasticity = 0.1f;
            scrollSnapScrollRect.inertia = false;
            scrollSnapScrollRect.scrollSensitivity = 1f;
            scrollSnapScrollRect.decelerationRate = 0.01f;
            GameObjectUtility.SetParentAndAlign(pagedScrollView, parent);
            pagedScrollView.AddComponent<PagedScrollView>();

            //hide ScrollRectEx
            //pagedScrollView.GetComponent<ScrollRectEx>().hideFlags = HideFlags.HideInInspector;

            //add image component
            Image bacgroundImg = pagedScrollView.AddComponent<Image>();
            bacgroundImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            bacgroundImg.color = new Color(1, 1, 1, 0);
            bacgroundImg.type = Image.Type.Sliced;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            RectTransform viewportRectTransform = viewport.AddComponent<RectTransform>();
            viewportRectTransform.anchorMin = new Vector2(0, 0);
            viewportRectTransform.anchorMax = new Vector2(1, 1);
            viewportRectTransform.offsetMin = Vector2.zero;
            viewportRectTransform.offsetMax = Vector2.zero;
            viewportRectTransform.pivot = new Vector2(0f, 1f);

            viewport.AddComponent<Mask>();
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            viewportImage.color = new Color(1, 1, 1, 1 - 0.8f);
            viewportImage.type = Image.Type.Sliced;

            scrollSnapScrollRect.viewport = viewportRectTransform;
            GameObjectUtility.SetParentAndAlign(viewport, pagedScrollView);

            // Content
            GameObject content = new GameObject("Content");
            RectTransform contentRectTransform = content.AddComponent<RectTransform>();

            contentRectTransform.anchorMin = Vector2.zero;
            contentRectTransform.anchorMax = Vector2.one;
            contentRectTransform.pivot = new Vector2(0, 1f);
            contentRectTransform.offsetMin = Vector2.zero;
            contentRectTransform.offsetMax = Vector2.zero;
            scrollSnapScrollRect.content = contentRectTransform;

            GameObjectUtility.SetParentAndAlign(content, viewport);
        }

        [MenuItem("GameObject/UI/Paged Scroll View/Paged Horizontal", false)]
        static void CreateHorizontalPagedScrollView()
        {
            // Canvas

            GameObject currentSelected = Selection.activeGameObject;
            Canvas canvas = currentSelected && currentSelected.GetComponent<Canvas>() ? currentSelected.GetComponent<Canvas>() : FindObjectOfType<Canvas>();
            GameObject canvasGO;

            if (canvas == null)
            {
                Type[] components = new Type[] { typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasScaler) };
                canvasGO = new GameObject("Canvas", components);

                Canvas canvasCanvasGO = canvasGO.GetComponent<Canvas>();
                canvasCanvasGO.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler canvasScalerCanvasGO = canvasGO.GetComponent<CanvasScaler>();
                canvasScalerCanvasGO.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScalerCanvasGO.referenceResolution = new Vector2(480, 854);
            }
            else
            {
                canvasGO = canvas.gameObject;
            }


            CreateScrollViewLayouts(canvasGO, Vector2.right, out GameObject pagedScrollView);
            pagedScrollView.name = "Paged Scroll View (Paged Horizontal)";
            Selection.activeGameObject = pagedScrollView;

            // Event System
            if (!FindObjectOfType<EventSystem>())
            {
                GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem));
                eventObject.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventObject, "Create " + eventObject.name);
            }

            //Undo.RegisterCreatedObjectUndo(canvasGO, "Create " + "Base");
        }

        [MenuItem("GameObject/UI/Paged Scroll View/Paged Vertical", false)]
        static void CreateVerticalPagedScrollView()
        {
            // Canvas
            GameObject currentSelected = Selection.activeGameObject;
            Canvas canvas = currentSelected && currentSelected.GetComponent<Canvas>() ? currentSelected.GetComponent<Canvas>() : FindObjectOfType<Canvas>();
            GameObject canvasGO;

            if (canvas == null)
            {
                Type[] components = new Type[] { typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasScaler) };
                canvasGO = new GameObject("Canvas", components);

                Canvas canvasCanvasGO = canvasGO.GetComponent<Canvas>();
                canvasCanvasGO.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler canvasScalerCanvasGO = canvasGO.GetComponent<CanvasScaler>();
                canvasScalerCanvasGO.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScalerCanvasGO.referenceResolution = new Vector2(480, 854);
            }
            else
            {
                canvasGO = canvas.gameObject;
            }


            CreateScrollViewLayouts(canvasGO, Vector2.up, out GameObject pagedScrollView);
            pagedScrollView.name = "Paged Scroll View (Paged Vertical)";
            Selection.activeGameObject = pagedScrollView;

            // Event System
            if (!FindObjectOfType<EventSystem>())
            {
                GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem));
                eventObject.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventObject, "Create " + eventObject.name);
            }

            //Undo.RegisterCreatedObjectUndo(canvasGO, "Create " + "Base");
        }

        void PageButtons()
        {
            if (!_PagedScrollView.content || _PagedScrollView.NumberOfPage == 0)
            {
                return;
            }

            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUIStyle style = new GUIStyle()
            {
                fontSize = 10,

                alignment = TextAnchor.MiddleCenter,
            };

            for (int i = 0; i < _PagedScrollView.NumberOfPage; i++)
            {
                style.normal.textColor = i == _PagedScrollView.ActivePage ? Color.red : Color.black;

                if (GUILayout.Button(i.ToString(), style))
                {
                    _PagedScrollView.ActivePage = i;
                }
            }

            GUILayout.EndHorizontal();
        }

        void DrawSettings()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(Threshold, new GUIContent("Threshold", "???"), GUILayout.Height(20f));
            EditorGUILayout.PropertyField(MinimalDistancePercent, new GUIContent("MinimalDistancePercent", "???"), GUILayout.Height(20f));
            EditorGUILayout.PropertyField(AnimationSpeed, new GUIContent("AnimationSpeed", "???"), GUILayout.Height(20f));
            EditorGUILayout.PropertyField(OnPageChange, new GUIContent("OnPageChange", "???"));

            serializedObject.ApplyModifiedProperties();

            GUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            HeaderInformation();
            CreateNewPage();
            PageButtons();
            DrawSettings();
        }
    }
}
