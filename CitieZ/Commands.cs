using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CitieZ
{
    public static class Commands
    {
        public static void City(CommandArgs e)
        {
            e.Player.SendInfoMessage("You will be teleported, as soon as Renerte finishes this functionality ;)");
        }
    }
}
