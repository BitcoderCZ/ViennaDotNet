using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViennaDotNet.DB.Models.Common
{
    public record NonStackableItemInstance(
        string instanceId,
        int wear
    )
    {
    }
}
