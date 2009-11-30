using System;
using System.Xml;

namespace Spica.ROS
{

	public class Stack : Module
	{
		protected bool in_stack = false;

		protected string description = null;
		protected string description_brief = null;
		protected string license = null;
		protected string license_url = null;
		protected string author = null;

		public Stack() : base() {}
		public Stack(string dir) : base(dir) {}

		public override string Manifest
		{
			get { return "stack.xml"; }
		}

		public override string Cache
		{
			get { return ".rosstack_cache"; }
		}

		protected override void ProcessNode(XmlTextReader reader)
		{
			if (reader.NodeType != XmlNodeType.Element) return;

			switch (reader.Name.ToLower())
			{
				case "stack":
					this.in_stack = true;
					break;

				case "description":
					{
						if (!this.in_stack) break;


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
						if (!this.in_stack) break;

						// Get author
						this.author = reader.ReadString();
					}
					break;

				case "license":
					{
						if (!this.in_stack) break;

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

//			Console.WriteLine("{0}: {1}", this.path, reader.Name);
		}
	}
}
