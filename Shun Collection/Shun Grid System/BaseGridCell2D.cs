using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shun_Grid_System
{
    public abstract class BaseGridCell2D<TItem>
    {
        [Header("Base")]
        public readonly List<BaseGridCell2D<TItem>> AdjacentCells = new();
        private readonly Dictionary<BaseGridCell2D<TItem> ,double> _adjacentCellCosts = new();
        public readonly int XIndex, YIndex;
        public TItem Item;
        public bool IsObstacle = false;

        [Header("Pathfinding Debug")] 
        public BaseGridCell2D<TItem> ParentXZCell2D = null; 
        public double FCost;
        public double HCost;
        public double GCost;


        protected BaseGridCell2D(int xIndex, int yIndex, TItem item = default)
        {
            XIndex = xIndex;
            YIndex = yIndex;
            Item = item;
        }

        public void SetAdjacencyCell(BaseGridCell2D<TItem>[] adjacentRawCells, double [] adjacentCellCost = null)
        {
            foreach (var adjacentCell in adjacentRawCells)
            {
                SetAdjacencyCell(adjacentCell);
            }
        }
    
        public void SetAdjacencyCell(BaseGridCell2D<TItem> adjacentCell, double adjacentCellCost = 0)
        {
            if (!AdjacentCells.Contains(adjacentCell))
            {
                AdjacentCells.Add(adjacentCell);
                _adjacentCellCosts[adjacentCell] = adjacentCellCost;
            }
        }

        public void RemoveAdjacency(BaseGridCell2D<TItem>[] adjacentRawCells)
        {
            foreach (var adjacentCell in adjacentRawCells)
            {
                RemoveAdjacency(adjacentCell);
            }
        }
    
        public void RemoveAdjacency(BaseGridCell2D<TItem> adjacentCell)
        {
            if (!AdjacentCells.Contains(adjacentCell)) return;
            
            AdjacentCells.Remove(adjacentCell);
            _adjacentCellCosts.Remove(adjacentCell);
        }

        public double GetAdditionalAdjacentCellCost(BaseGridCell2D<TItem> adjacentCell)
        {
            return AdjacentCells.Contains(adjacentCell)? _adjacentCellCosts[adjacentCell] : 0;
        }
    }
}
