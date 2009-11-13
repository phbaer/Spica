using Antlr.StringTemplate;
using Antlr.StringTemplate.Language;

namespace Spica
{

	public class StringRenderer : IAttributeRenderer
    {

		public string ToString(object o)
        {
			return o.ToString();
		}

		public string ToString(object o, string format)
        {
			string temp = (string)o;

			switch (format) {

				case "upper":
					return temp.ToUpper();

				case "lower":
					return temp.ToLower();

				case "fupper":
					return temp.Substring(0, 1).ToUpper() + temp.Substring(1, temp.Length - 1);

				case "flower":
					return temp.Substring(0, 1).ToLower() + temp.Substring(1, temp.Length - 1);

			}

			return temp.ToString();
		}
	}
}

