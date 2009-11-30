using System;
using System.Xml;

namespace Spica.ROS
{

	public class Package : Module
	{
		protected bool in_package = false;

		protected string description = null;
		protected string description_brief = null;
		protected string license = null;
		protected string license_url = null;
		protected string author = null;

		public Package() : base() {}
		public Package(string dir) : base(dir) {}

		public override string Manifest
		{
			get { return "manifest.xml"; }
		}

		public override string Cache
		{
			get { return ".rospack_cache"; }
		}

		protected override void ProcessNode(XmlTextReader reader)
		{
			if (reader.NodeType != XmlNodeType.Element) return;

			switch (reader.Name.ToLower())
			{
				case "package":
					this.in_package = true;
					break;

				case "description":
					{
						if (!this.in_package) break;

						// Get brief description
						if (reader.MoveToAttribute("brief"))
						{
							this.description_brief = reader.Value;
						}

						// Get description
						this.description = reader.ReadString();
					}
					break;

				case "author":
					{
						if (!this.in_package) break;

						// Get author
						this.author = reader.ReadString();
					}
					break;

				case "license":
					{
						if (!this.in_package) break;

						// Get license URL
						if (reader.MoveToAttribute("url"))
						{
							this.license_url = reader.Value;
						}

						// Get licence
						this.license = reader.ReadString();
					}
					break;
			}


//			Console.WriteLine("{0}: {1}", this.dir, reader.Name);
		}
	}
}
