using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Castor;

namespace Spica
{
    public class TypeMap
    {
		protected List<string> spica_types = null;
		protected List<string> native_types = null;
		protected List<string> default_values = null;
		protected List<Regex> type_regexs = null;

		public TypeMap()
		{
			this.spica_types = new List<string>();
			this.native_types = new List<string>();
			this.default_values = new List<string>();
			this.type_regexs = new List<Regex>();
		}

        public TypeMap(string template_path)
        {
			this.spica_types = new List<string>();
			this.native_types = new List<string>();
			this.default_values = new List<string>();
			this.type_regexs = new List<Regex>();

			string mapping = Path.Combine(template_path, "TypeMapping");

            StreamReader reader = new StreamReader(File.OpenRead(mapping));

			int line_cnt = 0;
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
				line = line.Trim();

				if (line.Length == 0)
				{
					continue;
				}

				string[] elements = line.Split(new char[] { '\t' },
											   StringSplitOptions.RemoveEmptyEntries);

				line_cnt++;

				if (elements.Length != 4)
				{
					throw new CException("Unable to load type mapping from {0}, error in line {1}!",
										 mapping, line_cnt);
				}

				this.spica_types.Add(elements[0]);
				this.native_types.Add(elements[1]);
				this.default_values.Add(elements[2]);
				this.type_regexs.Add(new Regex(elements[3]));
            }
        }

		public string GetNativeType(string spica_type)
		{
			int index = this.spica_types.IndexOf(spica_type);

			// Do not map if no mapping exists
			if (index < 0)
			{
				return spica_type;
			}

			return this.native_types[index];
		}

		public string GetDefaultValue(string spica_type)
		{
			int index = this.spica_types.IndexOf(spica_type);

			// Do not map if no mapping exists
			if (index < 0)
			{
				return spica_type;
			}

			return this.default_values[index];
		}

		public bool CheckType(string spica_type, string value)
		{
			int index = this.spica_types.IndexOf(spica_type);

			// Type value is valid if no mapping exists
			if (index < 0)
			{
				return true;
			}

			return this.type_regexs[index].IsMatch(value);
		}
    }
}

