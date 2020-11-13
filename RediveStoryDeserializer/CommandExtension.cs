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

            Dictionary<string, string[]> dict = new Dictionary<string, string[]>();
            dict.Add(command.CommandName, command.Args);
            return dict;
        }
    }
}
