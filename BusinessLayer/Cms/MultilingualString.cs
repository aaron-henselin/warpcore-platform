using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarpCore.DbEngines.AzureStorage;

namespace Cms
{
    [SerializedComplexObject]
    public class MultilingualString : Dictionary<string,string>
    {
        public string this[CultureInfo x]
        {
            get => ToString(x,false);
            set => this[x.TwoLetterISOLanguageName] = value;
        }

        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture, true);
        }

        public string ToString(CultureInfo cultureInfo, bool fallbackToAnyLanguage)
        {
            var key = cultureInfo.TwoLetterISOLanguageName;
            if (this.ContainsKey(key))
                return this[key];

            if (fallbackToAnyLanguage)
            {
                key = this.Keys.First();
                return this[key];
            }

            throw new Exception("No translation is available for " + cultureInfo.TwoLetterISOLanguageName);
        }
    }
}
