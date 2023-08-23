using System;
using UnityEngine;

namespace Shun_Grid_System
{
    public class GridXY<TItem> : BaseGrid2D<GridXYCell<TItem>, TItem>
    {
        public GridXY(int width, int height, float cellWidthSize, float cellHeightSize, Vector3 worldOriginPosition) 
            : base(width, height, cellWidthSize, cellHeightSize, worldOriginPosition)
        {
            
        }

        public override Vector2Int GetIndexDifferenceFrom(BaseGridCell2D<TItem> subtrahend,BaseGridCell2D<TItem> minuend)
        {
            GridXYCell<TItem> subtrahendCell = (GridXYCell<TItem>) subtrahend;
            GridXYCell<TItem> minuendCell = (GridXYCell<TItem>) minuend;
            return new (minuendCell.XIndex - subtrahendCell.XIndex, minuendCell.YIndex - subtrahendCell.YIndex);
        }

        public override Vector2Int GetIndexDifferenceAbsolute(BaseGridCell2D<TItem> subtrahend,BaseGridCell2D<TItem> minuend)
        {
            GridXYCell<TItem> subtrahendCell = (GridXYCell<TItem>) subtrahend;
            GridXYCell<TItem> minuendCell = (GridXYCell<TItem>) minuend;
            return new (Math.Abs(minuendCell.XIndex - subtrahendCell.XIndex), Math.Abs(minuendCell.YIndex - subtrahendCell.YIndex));
        }

    }
}
