using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Finder
{
    public class ArgumentBuilder
    {
        private List<string> arguments;
        public ArgumentBuilder()
        {
            arguments = new List<string>();
        }
        public void AppendArgument(string argument)
        {
            arguments.Add(argument);
        }
        public void RemoveArgument(string argument)
        {
            arguments.Remove(argument);
        }
        public override string ToString()
        {
            string content = string.Empty;
            for (int i = 0; i < arguments.Count; i++)
            {
                if (i != arguments.Count - 1)
                {
                    content += arguments[i] + " ";
                }
                else
                {
                    content += arguments[i];
                }
            }
            return content;
        }

        public void Clear()
        {
            arguments.Clear();
        }
    }
}
