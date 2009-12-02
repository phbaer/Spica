/*
 * ROS support library: Stack class
 *
 * 2009 Spica Robotics Project (http://spica-robotics.net/)
 * 2009 DFKI RIC Bremen, Germany (http://dfki.de/robotik/)
 *
 * Author: Philipp A. Baer <philipp.baer at dfki.de>
 *
 * The code is derived from the software contributed to Carpe Noctem by
 * the Carpe Noctem Team.
 *
 * Licensed under the FreeBSD license (modified BSD license).
 *
 * You should have received a copy of the license along with this
 * software. The license is also available online:
 * http://spica-robotics.net/license.txt
 */

using System;
using System.Xml;

namespace Spica.ROS
{

	public class Stack : Module
	{
		public Stack() : base() {}
		public Stack(string dir) : base(dir) {}

		public override string ManifestFile
		{
			get { return "stack.xml"; }
		}

		public override string CacheFile
		{
			get { return ".rosstack_cache"; }
		}

		protected override void ProcessingHook(XmlTextReader reader)
		{
			string name = reader.Name.ToLower();

			// Extract dependencies
			if (name.Equals("depend"))
			{
				if (reader.MoveToAttribute("stack"))
				{
					this.dep_names.Add(reader.Value);
				}
			}
		}
	}
}
