using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    class Common
    {
    }

    public static class Extensions
    {
        public static List<int> FindCommonList(this List<int> firstList, List<int> secondList)
        {
            List<int> commonList = new List<int>();
            foreach (var item in firstList)
            {
                if (secondList.Contains(item))
                {
                    commonList.Add(item);
                }
            }
            return commonList;
        }

        public static List<int> ExcludeCommonList(this List<int> firstList, List<int> secondList)
        {
            secondList.ForEach(s =>
            {
                firstList.Remove(s);
            });
            return firstList;
        }

        public static List<int> FindRandomItemsFromList(this List<int> list, int count)
        {
            List<int> resultList = new List<int>();
            int listCount = list.Count();

            Random rnd = new Random();

            if (listCount <= count)
            {
                return list;
            }

            while(resultList.Count() < count)
            {
                int item = rnd.Next(0, listCount - 1);
                if (resultList.Contains(list[item]))
                {
                    continue;
                }

                resultList.Add(list[item]);
            }

            return resultList;
        }
    }
}
