using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;
using System.Collections.Generic;
using System;

namespace EnhancedScrollerDemos.NestedWallet
{
    /// <summary>
    /// The master cell view contains a detail EnhancedScroller
    /// </summary>
    public class MasterCellView : EnhancedScrollerCellView, IEnhancedScrollerDelegate
    {
        private bool reloadDataNextFrame = false;

        /// <summary>
        /// The detail scroller containing our detail cells
        /// </summary>
        public EnhancedScroller detailScroller;

        /// <summary>
        /// The list of detail cells for this master cell
        /// </summary>
        private MasterData _data;
        public bool preloadCells = true;

        /// <summary>
        /// Detail cell prefab to instantiate
        /// </summary>
        public EnhancedScrollerCellView detailCellViewPrefab;

        /// <summary>
        /// Sets the detail scroller delegate and data
        /// </summary>
        /// <param name="data"></param>
        public void SetData(MasterData data)
        {
            // set up delegates and callbacks
            detailScroller.Delegate = this;
            detailScroller.scrollerScrolled = ScrollerScrolled;

            // set the scroller's cell view visbility changed delegate to a method in this controller
            detailScroller.cellViewVisibilityChanged = CellViewVisibilityChanged;
            detailScroller.cellViewWillRecycle = CellViewWillRecycle;

            // preload the cells by looking ahead (both behind and after)
            if (preloadCells)
            {
                detailScroller.lookAheadBefore = 1000f;
                detailScroller.lookAheadAfter = 1000f;
            }

            // assign data and flag that the detail scroller needs to be reloaded.
            // we have to reload on the next frame through the update so that the 
            // main scroller has time to set up the master cell views first.
            _data = data;
            reloadDataNextFrame = true;
        }

        /// <summary>
        /// Check to see if the scroller needs to be reloaded
        /// </summary>
        void Update()
        {
            if (reloadDataNextFrame)
            {
                // scroller needs reloaded, so we unflag and reload the detail data

                reloadDataNextFrame = false;
                detailScroller.ReloadData(_data.normalizedScrollPosition);
            }
        }

        #region EnhancedScroller Handlers

        /// <summary>
        /// This tells the scroller the number of cells that should have room allocated. This should be the length of your data array.
        /// </summary>
        /// <param name="scroller">The scroller that is requesting the data size</param>
        /// <returns>The number of cells</returns>
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            // in this example, we just pass the number of our detail data elements
            return _data.childData.Count;
        }

        /// <summary>
        /// This tells the scroller what the size of a given cell will be. Cells can be any size and do not have
        /// to be uniform. For vertical scrollers the cell size will be the height. For horizontal scrollers the
        /// cell size will be the width.
        /// </summary>
        /// <param name="scroller">The scroller requesting the cell size</param>
        /// <param name="dataIndex">The index of the data that the scroller is requesting</param>
        /// <returns>The size of the cell</returns>
        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            // in this example, we set the cells at 100 pixels wide
            return 600f;
        }

        /// <summary>
        /// Gets the cell to be displayed. You can have numerous cell types, allowing variety in your list.
        /// Some examples of this would be headers, footers, and other grouping cells.
        /// </summary>
        /// <param name="scroller">The scroller requesting the cell</param>
        /// <param name="dataIndex">The index of the data that the scroller is requesting</param>
        /// <param name="cellIndex">The index of the list. This will likely be different from the dataIndex if the scroller is looping</param>
        /// <returns>The cell for the scroller to use</returns>
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            // first, we get a cell from the scroller by passing a prefab.
            // if the scroller finds one it can recycle it will do so, otherwise
            // it will create a new cell.
            DetailCellView detailCellView = scroller.GetCellView(detailCellViewPrefab) as DetailCellView;

            // set the name of the game object to the cell's data index.
            // this is optional, but it helps up debug the objects in 
            // the scene hierarchy.
            detailCellView.name = "Detail Cell " + dataIndex.ToString();

            // in this example, we just pass the data to our cell's view which will update its UI
            detailCellView.SetData(_data.childData[dataIndex]);

            // return the cell to the scroller
            return detailCellView;
        }


        /// <summary>
        /// This handler will be called any time a cell view is shown or hidden
        /// </summary>
        /// <param name="cellView">The cell view that was shown or hidden</param>
        private void CellViewVisibilityChanged(EnhancedScrollerCellView cellView)
        {
            // cast the cell view to our custom view
            DetailCellView view = cellView as DetailCellView;

            // if the cell is active, we set its data,
            // otherwise we will clear the image back to
            // its default state

            if (cellView.active)
                view.SetData(_data.childData[cellView.dataIndex]);
            else
                view.ClearImage();
        }

        /// <summary>
        /// Tells the cell view that it is about to be recycled
        /// </summary>
        /// <param name="cellView">Cell view.</param>
        private void CellViewWillRecycle(EnhancedScrollerCellView cellView)
        {
            (cellView as DetailCellView).WillRecycle();
        }

        /// <summary>
        /// Capture the scroll position to use when the scroller is recycled
        /// </summary>
        private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            _data.normalizedScrollPosition = scroller.NormalizedScrollPosition;
        }

        #endregion
    }
}
