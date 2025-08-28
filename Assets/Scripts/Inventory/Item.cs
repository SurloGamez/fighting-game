using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.UIElements;
using UnityEngine;

[System.Serializable]
public class Item
{
    public GameObject gameObject; //game object linked with this instance of class Item
    int id; //index on the the items list
    [SerializeField] Vector2Int positionIndex; //the index of the very bottom left slot of the item
    [SerializeField] Vector2Int size; //unrotated dimensions of the item
    [SerializeField] bool rotated = false; //true = vertical, false = horizontal
    int[] occupiedSlots; //local Indicies of the slots this item occupies

    public Item(Vector2Int position, Vector2Int size)
    {
        this.positionIndex = position; 
        this.size = size;
    }
    public int ID
    {
        get { return id; } set { id = value; }
    }
    public Vector2Int PositionIndex{
        get { return positionIndex; } set { positionIndex = value; }
    }
    public Vector2Int Size
    {
        get { return size; } set { size = value; }
    }
    public int[] OccupiedSlots
    {
        get { return occupiedSlots; }
    }
    public bool Rotated
    {
        get { return rotated; }
    }
    public void CalcuateSlotsOccupied(Vector2Int InventorySize)
    {
        occupiedSlots = new int[size.x * size.y];

        int i = 0;
        for(int y = 0; y < (!rotated ? size.y : size.x); y++)
        {
            for(int x = 0; x < (!rotated ? size.x : size.y); x++)
            {
                occupiedSlots[i] = y * InventorySize.x + x;
                i++;
            }
        }
    }

    public void RotateItem(Vector2Int InventorySize)
    {
        rotated = !rotated;
        CalcuateSlotsOccupied(InventorySize);
    }

    public void Render(Vector2 wrldPos, float unitSpacing, Vector2 canvasScale)
    {
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.position = wrldPos * canvasScale;
        if (rotated){rect.rotation = Quaternion.Euler(0, 0, 90); rect.position += Vector3.right * unitSpacing * canvasScale.x; }
        else{rect.rotation = Quaternion.Euler(0, 0, 0);}
    }

    public void Render(Vector2 wrldPos, float unitSpacing, Vector2 canvasScale, float rotatedScale)
    {
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.position = wrldPos * canvasScale;
        if (rotated) { rect.rotation = Quaternion.Euler(0, 0, 90); rect.position += Vector3.right * unitSpacing * rotatedScale; }
        else { rect.rotation = Quaternion.Euler(0, 0, 0); }
    }
}

public class Ammo : Item 
{
    int amount;
    public Ammo(Vector2Int position, Vector2Int size, int amount) : base(position, size)
    {
        this.amount = amount;
    }

    public void Test()
    {
        Debug.Log(amount);
    }
}

