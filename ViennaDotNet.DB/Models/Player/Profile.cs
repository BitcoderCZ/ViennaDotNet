using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViennaDotNet.DB.Models.Player
{
    public sealed class Profile
    {
        public int health;
        public int experience;
        public int level;
        public Rubies rubies;

        public Profile()
        {
            health = 20;
            experience = 0;
            level = 1;
            rubies = new Rubies();
        }
    }
}
