using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;


namespace SendScroller
{
    /// <summary>
    /// These delegates will publish events when a button is clicked
    /// </summary>
    /// <param name="value"></param>
    public delegate void CellButtonTextClickedDelegate(string value);
    public delegate void CellButtonIntegerClickedDelegate(int value);

    public class CellView : EnhancedScrollerCellView
    {
        private UserProfile _data;

        public Text someTextText;

        /// <summary>
        ///  These delegates will fire whenever one of the events occurs
        /// </summary>
        public CellButtonTextClickedDelegate cellButtonTextClicked;
        public CellButtonIntegerClickedDelegate cellButtonFixedIntegerClicked;

        public void SetData(UserProfile data)
        {
            _data = data;
            someTextText.text = $"{data.username}\n{data.address}";
        }

        // Handle the click of the fixed text button (this is hooked up in the Unity editor in the button's click event)
        public void CellButtonText_OnClick()
        {
            // fire event if anyone has subscribed to it
            //NFTController.GetComponent<MarketPlace>().Burn(token);
            if (cellButtonTextClicked != null) cellButtonTextClicked(_data.address);
        }

        // Handle the click of the fixed integer button (this is hooked up in the Unity editor in the button's click event)
        public void CellButtonFixedInteger_OnClick(int value)
        {
            // fire event if anyone has subscribed to it
            if (cellButtonFixedIntegerClicked != null) cellButtonFixedIntegerClicked(value);
        }

    }

}