using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class CatalogueComparerByBin : Comparer<Catalogue>
    {
        public override int Compare(Catalogue x, Catalogue y)
        {
            int iteratingCount = 0;
            if (y.bin.Length >= x.bin.Length)
            {
                iteratingCount = x.bin.Length;
            }
            else
            {
                iteratingCount = y.bin.Length;
            }
            for (int i = 0; i < iteratingCount; i++)
            {
                if (x.bin[i] > y.bin[i])
                {
                    return 1;
                }
                else if (x.bin[i] < y.bin[i])
                {
                    return -1;
                }
                else if (x.bin[i] == y.bin[i])
                {
                    if (i == iteratingCount - 1)
                        return 0;
                }
            }
            return 0;
        }
    }
}