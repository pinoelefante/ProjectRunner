using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.Views
{
    public class SportItem
    {
        public Sports SportEnumValue { get; }
        public string SportName { get; }
        public SportItem(Sports e, string n)
        {
            SportEnumValue = e;
            SportName = n;
        }
    }
}
