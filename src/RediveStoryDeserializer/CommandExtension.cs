using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RediveStoryDeserializer
{
    public static class CommandExtension
    {
        public static Dictionary<string, string[]> ToDict(this Command command)
        {
            if (command.CommandName == null)
                return null;

            var dict = new Dictionary<string, string[]> {{command.CommandName, command.Args}};
            return dict;
        }
    }
}
