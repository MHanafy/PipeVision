using System.Collections.Generic;
using System.Linq;

namespace PipeVision.Domain
{

    public static class ApplyHelper
    {
        public static void Apply<T, TKey>(this IList<T> list, IEnumerable<T> updatedList)
            where T : IApplicable<T, TKey>
        {
            if (updatedList == null)
            {
                list.Clear();
                return;
            }
            var current = list.ToDictionary(x => x.Id, x => x);
            foreach (var item in updatedList)
            {
                if (current.ContainsKey(item.Id)) current[item.Id].Apply(item);
                else
                {
                    list.Add(item);
                }
            }
        }
    }

    public interface IApplicable<in T, out TKey> : IIdentifiable<TKey>
    {
        void Apply(T updated);
    }

    public interface IIdentifiable<out TKey>
    {
        TKey Id { get; }
    }
}
