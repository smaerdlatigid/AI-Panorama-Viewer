using UnityEngine;
using System.Collections.Generic;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using Photon.Pun;
using Photon.Realtime;

// Server/Room Information
public class Data
{
    public string name;
    public string host;
    public int maxPlayers;
    public int currentPlayers;
    public string room_name;
}

/// <summary>
/// This demo shows how you can respond to events from your cells views using delegates
/// </summary>
public class Controller : MonoBehaviour, IEnhancedScrollerDelegate
{
    private List<Data> _data;

    public EnhancedScroller scroller;
    public EnhancedScrollerCellView cellViewPrefab;
    public float cellSize;

    public GameObject Multiplayer;

    void Start()
    {
        // set the application frame rate.
        // this improves smoothness on some devices
        Application.targetFrameRate = 60;

        scroller.Delegate = this;
        _data = new List<Data>();

    }

    public void LoadData(Dictionary<string, RoomInfo> rooms)
    {
        _data.Clear();
        foreach (KeyValuePair<string, RoomInfo> room in rooms)
        {
            _data.Add(new Data()
            {
                name = room.Key.Split('+')[0],
                host =  room.Key.Split('+')[1],
                maxPlayers = room.Value.MaxPlayers,
                currentPlayers = room.Value.PlayerCount,
                room_name = room.Key
            });
        }
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
        cellView.cellButtonDataIntegerClicked = CellButtonDataIntegerClicked;

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
        Multiplayer.GetComponent<LoginScreenUI>().MultiplayerLogin(room_name);
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

