using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ViennaDotNet.DB.Models.Player
{
    public sealed class Rubies
    {
        public int purchased;
        public int earned;

        public Rubies()
        {
            purchased = 0;
            earned = 0;
        }

        public bool spend(int amount)
        {
            if (amount > purchased + earned)
                return false;

            // TODO: in what order should purchased/earned rubies be spent?
            if (amount > purchased)
            {
                amount -= purchased;
                purchased = 0;
            }
            else
            {
                purchased -= amount;
                amount = 0;
            }

            if (amount > 0)
                earned -= amount;

            if (purchased < 0 || earned < 0)
                throw new InvalidOperationException();

            return true;
        }
    }
}
