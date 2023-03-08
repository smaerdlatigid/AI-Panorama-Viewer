using UnityEngine;
using System.Collections.Generic;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;

/// <summary>
/// This demo shows how you can respond to events from your cells views using delegates
/// </summary>
public class SendController : MonoBehaviour, IEnhancedScrollerDelegate
{
    private List<NFTSendInfo> _data;

    public EnhancedScroller scroller;
    public EnhancedScrollerCellView cellViewPrefab;
    public float cellSize;
    void Start()
    {
        // set the application frame rate.
        // this improves smoothness on some devices
        Application.targetFrameRate = 60;
        scroller.gameObject.SetActive(false);

        scroller.Delegate = this;
        _data = new List<NFTSendInfo>();
    }

    public void LoadData(List<NFTSendInfo> data)
    {
        Debug.Log($"LoadData: {data.Count}");
        _data = data;
        scroller.ReloadData();
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
        SendCellView cellView = scroller.GetCellView(cellViewPrefab) as SendCellView;

        // Set handlers for the cell views delegates.
        // Each handler will respond to a different type of event
        cellView.cellButtonTextClicked = SendCellButtonClickedDelegate;
        //cellView.cellButtonFixedIntegerClicked = CellButtonFixedIntegerClicked;
        //cellView.cellButtonDataIntegerClicked = CellButtonDataIntegerClicked;
        cellView.SetData(_data[dataIndex]);
        return cellView;
    }
    
    #endregion

    List<NFTSendInfo> userProfiles = new List<NFTSendInfo>();

    public void ShowMenu(nft token)
    {
        scroller.gameObject.SetActive(true);

        userProfiles.Clear();
        NFTSendInfo close = new NFTSendInfo();
        close.name = "";
        close.address = "close";
        userProfiles.Add(close);
        NFTSendInfo burner = new NFTSendInfo();
        burner.name = "Burn";
        burner.address = "0x000000000000000000000000000000000000dEaD";
        burner.token = token;
        userProfiles.Add(burner);

        // TODO multiplayer list
        LoadData(userProfiles);
    }

    /// <summary>
    /// Handler for when the cell view fires a fixed text button click event
    /// </summary>
    /// <param name="value">value of the text</param>
    private void SendCellButtonClickedDelegate(nft snft, string address)
    {
        if (address == "close")
        {
            scroller.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"Sending {snft.name} to: {address}");
            GetComponent<MarketPlace>().Transfer(snft, address, TransferFinished);
        }
    }

    public void TransferFinished()
    {
        Debug.Log("Transfer Finished");
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

