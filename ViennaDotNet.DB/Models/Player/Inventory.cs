using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.DB.Models.Common;

namespace ViennaDotNet.DB.Models.Player
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Inventory
    {
        [JsonProperty]
        private Dictionary<string, int?> stackableItems;
        [JsonProperty]
        private Dictionary<string, Dictionary<string, NonStackableItemInstance>> nonStackableItems;

        public Inventory()
        {
            stackableItems = new();
            nonStackableItems = new();
        }

        public Inventory copy()
        {
            Inventory inventory = new Inventory();
            inventory.stackableItems.AddRange(stackableItems);
            Dictionary<string, Dictionary<string, NonStackableItemInstance>> nonStackableItems = new();
            this.nonStackableItems.ForEach((id, instances) => nonStackableItems.Add(id, new Dictionary<string, NonStackableItemInstance>(instances)));
            inventory.nonStackableItems.AddRange(nonStackableItems);
            return inventory;
        }

        public record StackableItem(
            string id,
            int? count
        )
        {
        }

        public StackableItem[] getStackableItems()
        {
            return stackableItems.Select(item => new StackableItem(item.Key, item.Value)).ToArray();
        }

        public record NonStackableItem(
            string id,
            NonStackableItemInstance[] instances
        )
        {
        }

        public NonStackableItem[] getNonStackableItems()
        {
            return nonStackableItems.Select(item => new NonStackableItem(item.Key, item.Value.Values.ToArray())).ToArray();
        }

        public int getItemCount(string id)
        {
            int? count = stackableItems.GetOrDefault(id, null);
            if (count != null)
                return count.Value;

            Dictionary<string, NonStackableItemInstance>? instances = nonStackableItems!.GetOrDefault(id, null);

            if (instances != null)
                return instances.Count;

            return 0;
        }

        public NonStackableItemInstance[] getItemInstances(string id)
        {
            Dictionary<string, NonStackableItemInstance>? instances = nonStackableItems!.GetOrDefault(id, null);
            if (instances != null)
                return instances.Values.ToArray();

            return Array.Empty<NonStackableItemInstance>();
        }

        public NonStackableItemInstance? getItemInstance(string id, string instanceId)
        {
            Dictionary<string, NonStackableItemInstance>? instances = nonStackableItems!.GetOrDefault(id, null);
            if (instances != null)
                return instances.GetOrDefault(instanceId, null);

            return null;
        }

        public void addItems(string id, int count)
        {
            if (count < 0)
                throw new ArgumentException(nameof(count));

            stackableItems[id] = stackableItems.GetOrDefault(id, 0) + count;
        }

        public void addItems(string id, NonStackableItemInstance[] instances)
        {
            Dictionary<string, NonStackableItemInstance> instancesMap = nonStackableItems.ComputeIfAbsent(id, id1 => new Dictionary<string, NonStackableItemInstance>())!;

            foreach (NonStackableItemInstance instance in instances)
                instancesMap.Add(instance.instanceId, instance);
        }

        public bool takeItems(string id, int count)
        {
            if (count < 0)
                throw new ArgumentException(nameof(count));

            int currentCount = stackableItems.GetOrDefault(id, 0)!.Value;
            if (currentCount < count)
                return false;

            stackableItems[id] = currentCount - count;
            return true;
        }

        public NonStackableItemInstance[]? takeItems(string id, string[] instanceIds)
        {
            Dictionary<string, NonStackableItemInstance>? instanceMap = nonStackableItems.GetOrDefault(id, null);
            if (instanceMap == null)
                return null;

            LinkedList<NonStackableItemInstance> instances = new();
            foreach (string instanceId in instanceIds)
            {
                NonStackableItemInstance? instance = instanceMap.JavaRemove(instanceId);
                if (instance == null)
                    return null;

                instances.AddLast(instance);
            }
            return instances.ToArray();
        }
    }
}
