using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;


/// <summary>
/// These delegates will publish events when a button is clicked
/// </summary>
/// <param name="value"></param>
public delegate void SendCellButtonTextClickedDelegate(nft snft, string value);

public class SendCellView : EnhancedScrollerCellView
{
    private NFTSendInfo _data;

    public Text text;
    public Button button;
    /// <summary>
    ///  These delegates will fire whenever one of the events occurs
    /// </summary>
    public SendCellButtonTextClickedDelegate cellButtonTextClicked;

    public void SetData(NFTSendInfo data)
    {
        _data = data;
        text.text = $"{data.name}\n{data.address}";
        if (data.address == "close")
        {
            // set button text
            button.GetComponentInChildren<Text>().text = "Close";
        }
        else
        {
            button.GetComponentInChildren<Text>().text = "Send";
        }
    }

    // Handle the click of the fixed text button (this is hooked up in the Unity editor in the button's click event)
    public void CellButtonText_OnClick()
    {
        //if (cellButtonTextClicked != null)
        //{
            if (_data.address == "close")
            {
                cellButtonTextClicked(null, "close");
            }

            cellButtonTextClicked(_data.token, _data.address);
    }

    // // Handle the click of the fixed integer button (this is hooked up in the Unity editor in the button's click event)
    // public void CellButtonFixedInteger_OnClick(int value)
    // {
    //     // fire event if anyone has subscribed to it
    //     if (cellButtonFixedIntegerClicked != null) cellButtonFixedIntegerClicked(value);
    // }

    // // Handle the click of the data integer button (this is hooked up in the Unity editor in the button's click event)
    // public void CellButtonDataInteger_OnClick()
    // {
    //     // fire event if anyone has subscribed to it
    //     if (cellButtonDataIntegerClicked != null) cellButtonDataIntegerClicked(_data.currentPlayers);
    // }
}
