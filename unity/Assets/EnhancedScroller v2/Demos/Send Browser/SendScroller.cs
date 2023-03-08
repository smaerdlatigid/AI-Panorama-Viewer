using UnityEngine;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;

/// <summary>
/// This demo shows how you can respond to events from your cells views using delegates
/// </summary>

namespace SendScroller
{    
    public class Controller : MonoBehaviour, IEnhancedScrollerDelegate
    {
        List<UserProfile> _data = new List<UserProfile>();

        public EnhancedScroller scroller;
        public EnhancedScrollerCellView cellViewPrefab;
        public float cellSize;

        void Start()
        {
            scroller.Delegate = this;
            _data = new List<UserProfile>();
        }

        public void LoadData(List<UserProfile> data)
        {
            Debug.Log($"data.Count: {data.Count}");
            _data = data;
            scroller.ReloadData();
            //scroller.RefreshActiveCellViews();
        }

        #region EnhancedScroller Handlers

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _data.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return cellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            CellView cellView = scroller.GetCellView(cellViewPrefab) as CellView;

            // Set handlers for the cell views delegates.
            // Each handler will respond to a different type of event
            cellView.cellButtonTextClicked = CellButtonTextClicked;
            cellView.cellButtonFixedIntegerClicked = CellButtonFixedIntegerClicked;

            cellView.SetData(_data[dataIndex]);

            return cellView;
        }

        #endregion

        /// <summary>
        /// Handler for when the cell view fires a fixed text button click event
        /// </summary>
        /// <param name="value">value of the text</param>
        private void CellButtonTextClicked(string room_name)
        {
            Debug.Log("Cell Text Button Clicked! Value = " + room_name);
        }

        /// <summary>
        /// Handler for when the cell view fires a fixed integer button click event
        /// </summary>
        /// <param name="value">value of the integer</param>
        private void CellButtonFixedIntegerClicked(int value)
        {
            Debug.Log("Cell Fixed Integer Button Clicked! Value = " + value);
        }

        /// <summary>
        /// Handler for when the cell view fires a data integer button click event
        /// </summary>
        /// <param name="value">value of the integer</param>
        private void CellButtonDataIntegerClicked(int value)
        {
            Debug.Log("Cell Data Integer Button Clicked! Value = " + value);
        }
    }

}