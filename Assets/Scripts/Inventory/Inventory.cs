using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

/*problem: 
 * how to instantiate an item as ammo
 */

public class Inventory : MonoBehaviour
{
    [SerializeField] Vector2 StartPos;
    [SerializeField] float SpacingUnit;
    [SerializeField] Vector2Int Size; //inventory dimensions
    [SerializeField] RectTransform hoverOver;
    public List<Item> items;
    int[] itemIndicies;
    int[] SlotsOccupiedWrld;
    Item ItemInHand;
    RectTransform Canvas;
    //Item ammo = Item as Ammo(Vector2Int.one, Vector2Int.one, 5);
    void Start()
    {
        //ammo.Test();
        Canvas = transform.parent.GetComponent<RectTransform>();
        itemIndicies = new int[Size.x * Size.y];
        for(int i = 0; i< itemIndicies.Length; i++)
        {
            itemIndicies[i] = -1;
        }

        RenderItemsOnGrid();
        updateItemIndiciesGrid(items.ToArray());
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            RenderItemsOnGrid();
            updateItemIndiciesGrid(items.ToArray());
        }
        if(ItemInHand != null)
        {
            SlotsOccupiedWrld = GetSlotsOccupiedBy(ItemInHand);
            Render(hoverOver, StartPos + ((Vector2)PositionToUnitPosition(Input.mousePosition, Canvas.localScale) * SpacingUnit), SpacingUnit, Canvas.localScale, ItemInHand.Rotated);
            hoverOver.sizeDelta = ItemInHand.gameObject.GetComponent<RectTransform>().sizeDelta;
        }
        else
        {
            SlotsOccupiedWrld = null;
        }
        
        if(Input.GetKeyDown(KeyCode.R))
        {
            if(ItemInHand != null)
            {
                ItemInHand.RotateItem(Size);
            }
        }
        if(Input.GetMouseButtonDown(0))
        {
            int index = PositionToIndex(Input.mousePosition, Canvas.localScale);
            if(index > -1 && index < Size.x * Size.y)
            {
                if(ItemInHand != null)
                {
                    ItemInHand.PositionIndex = PositionToUnitPosition(Input.mousePosition, Canvas.localScale);
                    List<int> overlappedItems = GetItemOverlaps(SlotsOccupiedWrld);
                    if (overlappedItems.Count == 0)
                    {
                        replaceSlotsOccupiedWith(ItemInHand.ID, ItemInHand);
                        ItemInHand.Render(StartPos + ((Vector2)ItemInHand.PositionIndex * SpacingUnit), SpacingUnit,Canvas.localScale);
                        ItemInHand = null;
                    }
                }
                else
                {
                    bool clickedSmt = CheckIfClickedOnItem(index);
                }
                

            }
        }

        RenderItemInHand();
    }
    bool CheckIfClickedOnItem(int clickedIndex) //if clicked on an item, ItemInHand will be set to it
    {
        if (itemIndicies[clickedIndex] != -1)
        {
            ItemInHand = items[itemIndicies[clickedIndex]];
            replaceSlotsOccupiedWith(-1, ItemInHand);

            return true;
        }
        return false;
        
    }

    List<int> GetItemOverlaps(int[] slotsOccupiedWrld)
    {
        List<int> overlappedItems = new List<int>();
        foreach(int slot in slotsOccupiedWrld)
        {
            if(itemIndicies[slot] != -1 && !overlappedItems.Contains(itemIndicies[slot]))
            {
                overlappedItems.Add(itemIndicies[slot]);
            }
        }
        return overlappedItems;
    }

    int[] GetSlotsOccupiedBy(Item item)
    {
        int[] occupied = new int[item.OccupiedSlots.Length];
        int itemIndex = PositionToIndex(Input.mousePosition, Canvas.localScale);
        for (int i = 0; i < item.OccupiedSlots.Length; i++)
        {
            occupied[i] = itemIndex + item.OccupiedSlots[i];
        }
        return occupied;
    }

    void updateItemIndiciesGrid(Item[] items)
    {
        for(int i = 0; i < items.Length; i++)
        {
            items[i].ID = i;
            items[i].CalcuateSlotsOccupied(Size);
            replaceSlotsOccupiedWith(i, items[i]);
        }
    }

    void replaceSlotsOccupiedWith(int replacement, Item item)
    {
        int itemIndex = UnitPositionToIndex(item.PositionIndex);
        for (int i = 0; i < item.OccupiedSlots.Length; i++)
        {
            itemIndicies[itemIndex + item.OccupiedSlots[i]] = replacement;
        }
    }

    void RenderItemInHand()
    {
        if(ItemInHand != null)
        {
            ItemInHand.Render(Input.mousePosition, SpacingUnit, Vector3.one, Canvas.localScale.x);
        }
    }
    void RenderItemsOnGrid()
    {
        foreach (var item in items)
        {
            item.Render(StartPos + ((Vector2)item.PositionIndex * SpacingUnit), SpacingUnit, Canvas.localScale);
            if (item is Ammo){}
        }
    }

    Vector2Int PositionToUnitPosition(Vector2 mousePos, Vector2 canvasScaler)
    {
        Vector2 unitPos = ((mousePos / canvasScaler) - StartPos) / SpacingUnit;
        return new Vector2Int(Mathf.FloorToInt(unitPos.x), Mathf.FloorToInt(unitPos.y));
    }

    int PositionToIndex(Vector2 mousePos, Vector2 canvasScaler)
    {
        Vector2Int unitIndex = PositionToUnitPosition(mousePos, canvasScaler);
        return UnitPositionToIndex(unitIndex);
    }

    int UnitPositionToIndex(Vector2Int unitpos)
    {
        return (unitpos.y * Size.x) + unitpos.x;
    }

    public void Render(RectTransform rect, Vector2 wrldPos, float unitSpacing, Vector2 canvasScale, bool rotated)
    {
        rect.position = wrldPos * canvasScale;
        if (rotated) { rect.rotation = Quaternion.Euler(0, 0, 90); rect.position += Vector3.right * unitSpacing * canvasScale.x; }
        else { rect.rotation = Quaternion.Euler(0, 0, 0); }
    }
}
