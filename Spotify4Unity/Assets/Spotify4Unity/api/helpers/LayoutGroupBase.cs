using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Spotify4Unity.Helpers
{
    /// <summary>
    /// Spotify4Unity
    /// Base class for creating a vertical list of prefabs which can be interacted with
    /// (Make sure you read the Wiki documentation for all information & help!)
    /// </summary>
    /// <typeparam name="ListElementT">The data class that will be used to populate the prefab with data</typeparam>
    public class LayoutGroupBase<ListElementT> : SpotifyUIBase where ListElementT : class
    {
        [Tooltip("The amount of pixels inbetween each prefab element in the list")]
        public Vector2 Padding = Vector2.one;

        [SerializeField, Tooltip("The prefab for one element of the control")]
        protected GameObject m_prefab = null;

        [SerializeField, Tooltip("The canvas that will hold all instantiated prefabs & be resized to fit them")]
        protected RectTransform m_resizeCanvas = null;

        [SerializeField, Tooltip("The ScrollRect that will control scrolling the list, usually the parent GameObject of the ResizeCanvas")]
        protected ScrollRect m_scrollRect = null;

        [SerializeField, Tooltip("Maximum limit of how many entries in the list. 0 for unlimited")]
        protected int m_listLimit = 1000;

        [SerializeField]
        ScrollRect.MovementType m_scrollType = ScrollRect.MovementType.Clamped;

        protected float m_prefabHeight { get { return m_prefab != null ? m_prefab.GetComponent<RectTransform>().rect.height : 0f; } }

        protected List<GameObject> m_instantiatedPrefabs = null;
        protected Coroutine m_updateRoutine = null;

        #region MonoBehaviours
        protected virtual void Start()
        {
            if (m_resizeCanvas != null)
                DestroyChildren(m_resizeCanvas);

            if (m_scrollRect != null)
            {
                m_scrollRect.movementType = m_scrollType;
                //m_scrollRect.verticalScrollbar.onValueChanged.AddListener(OnScrollChanged);
            }

            if(m_prefab != null && m_resizeCanvas.GetComponent<LayoutGroup>() != null)
            {
                ConfigureLayoutGroup(m_resizeCanvas.GetComponent<LayoutGroup>());
            }
        }
        #endregion

        protected virtual void ConfigureLayoutGroup(LayoutGroup layoutGroup)
        {
            Rect prefabRect = m_prefab.GetComponent<RectTransform>().rect;
            int width = (int)Padding.x;
            int height = (int)Padding.y;
            layoutGroup.padding = new RectOffset(width, width, height, height);
        }

        protected void UpdateUI(List<ListElementT> dataList)
        {
            DestroyChildren(m_resizeCanvas);

            if (dataList == null || dataList != null && dataList.Count <= 0)
                return;

            if (m_prefab == null)
            {
                Analysis.LogError($"Can't populate list on '{this.name}' since no prefab has been specified", Analysis.LogLevel.Vital);
                return;
            }

            //Remove any excess data we don't need if limit is above 0
            if (m_listLimit > 0 && dataList.Count > m_listLimit)
                dataList.RemoveRange(m_listLimit, dataList.Count - m_listLimit);

            List<GameObject> createdPrefabs = new List<GameObject>();
            foreach (ListElementT element in dataList)
            {
                GameObject instPrefab = Instantiate(m_prefab, m_resizeCanvas);
                SetPrefabInfo(instPrefab, element);
                createdPrefabs.Add(instPrefab);
            }

            float prefabHeight = m_prefab.GetComponent<RectTransform>().rect.height;
            float newCanvasHeight = prefabHeight * dataList.Count;
            SetScrollView(ref m_resizeCanvas, ref m_scrollRect, new Vector2(0, newCanvasHeight), prefabHeight);

            m_instantiatedPrefabs = createdPrefabs;
            OnUIUpdateFinished();
        }

        /// <summary>
        /// Populate each prefab in the list with it's data
        /// </summary>
        /// <param name="instantiatedPrefab">The currently instantiated prefab</param>
        /// <param name="data">The current data the list is on</param>
        protected virtual void SetPrefabInfo(GameObject instantiatedPrefab, ListElementT data)
        {
        }

        /// <summary>
        ///  Updates the list UI in a vertical pattern using Unity's Couroutines
        /// </summary>
        /// <param name="dataList"></param>
        protected virtual void UpdateUICoroutine(List<ListElementT> dataList)
        {
            DestroyChildren(m_resizeCanvas);

            if (dataList == null || dataList != null && dataList.Count <= 0)
                return;

            //Remove any excess data we don't need if limit is above 0
            if (m_listLimit > 0 && dataList.Count > m_listLimit)
                dataList.RemoveRange(m_listLimit, dataList.Count - m_listLimit);

            if (this.isActiveAndEnabled)
                m_updateRoutine = StartCoroutine(UpdateRoutine(dataList));
            else
                Utility.RunCoroutineEmptyObject(UpdateRoutine(dataList));
        }

        protected virtual System.Collections.IEnumerator UpdateRoutine(List<ListElementT> dataList)
        {
            if (m_prefab == null)
            {
                Analysis.LogError($"Can't populate list on '{this.name}' since no prefab has been specified", Analysis.LogLevel.Vital);
                yield return null;
            }

            List<GameObject> createdPrefabs = new List<GameObject>();
            foreach (ListElementT data in dataList)
            {
                if (data == null)
                    continue;

                GameObject instPrefab = Instantiate(m_prefab, m_resizeCanvas);
                SetPrefabInfo(m_prefab, data);

                createdPrefabs.Add(instPrefab);

                yield return new WaitForEndOfFrame();
            }

            float prefabHeight = m_prefab.GetComponent<RectTransform>().rect.height;
            float newCanvasHeight = prefabHeight * dataList.Count;
            SetScrollView(ref m_resizeCanvas, ref m_scrollRect, new Vector2(0, newCanvasHeight), prefabHeight);

            m_instantiatedPrefabs = createdPrefabs;
            OnUIUpdateFinished();

            m_updateRoutine = null;
        }

        /// <summary>
        /// Sets the canvas to the target size and right position, and sets the ScrollRect
        /// </summary>
        /// <param name="canvas">The canvas</param>
        /// <param name="scrollRect">The scroll rect of the canvas</param>
        /// <param name="canvasSize">The target new size of the canvas</param>
        /// <param name="scrollSensitivity">The amount to scroll by on one scroll</param>
        protected void SetScrollView(ref RectTransform canvas, ref ScrollRect scrollRect, Vector2 canvasSize, float scrollSensitivity)
        {
            //Set canvas new size
            //canvas.sizeDelta = canvasSize;
            //Set scrollbar position with canvas position
            canvas.localPosition = new Vector3(canvas.localPosition.x, -(canvas.rect.height / 2), canvas.localPosition.z);
            //Set sensitivity to scroll 1 track every scroll wheel click
            scrollRect.scrollSensitivity = scrollSensitivity;
            //ScrollRect to top position
            scrollRect.verticalNormalizedPosition = 1;
        }

        /// <summary>
        /// Destroys all children of the parent transform, won't destroy the parent transform passed through
        /// </summary>
        /// <param name="parent">The parent Transform containing children to destroy</param>
        protected void DestroyChildren(Transform parent)
        {
            List<Transform> children = parent.GetComponentsInChildren<Transform>().ToList();
            if (children.Contains(parent))
                children.Remove(parent);

            if (children.Count > 0)
            {
                foreach (Transform child in children)
                    GameObject.Destroy(child.gameObject);
            }

            if (m_instantiatedPrefabs != null)
            {
                m_instantiatedPrefabs.Clear();
                m_instantiatedPrefabs = null;
            }
        }

        /// <summary>
        /// Callback for when the UI has finished populating the list
        /// </summary>
        protected virtual void OnUIUpdateFinished()
        {
        }
    }
}