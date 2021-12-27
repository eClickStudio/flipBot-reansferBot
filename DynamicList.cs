using System.Collections.Generic;

namespace flipBot
{
    public class DynamicList<T>
    {
        public List<T> list; //must be < 255 and >= 0
        public byte maxCount;
        public DynamicList(in byte maxCount)
        {
            list = new List<T>();
            this.maxCount = maxCount;
        }
        public void Add(T element)
        {
            if (list.Count + 1 > maxCount)
            {
                for (byte i = 0; i < maxCount-1; i++)
                {
                    list[i] = list[i+1];
                }
                list[maxCount - 1] = element;
            }
            else
            {
                list.Add(element);
            }
            

        }
    }
}
