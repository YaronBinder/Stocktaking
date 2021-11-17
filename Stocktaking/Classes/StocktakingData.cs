using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stocktaking.Classes;

internal class StocktakingData
{
    public StocktakingData(DateTime date, int cellCount, string time, string tray, string cart, string cellType)
    {
        this.date = date;
        this.cellCount = cellCount;
        this.time = time;
        this.tray = tray;
        this.cart = cart;
        this.cellType = cellType;
    }

    public readonly DateTime date;
    public readonly int cellCount;
    public readonly string time;
    public readonly string tray;
    public readonly string cart;
    public readonly string cellType;
}
