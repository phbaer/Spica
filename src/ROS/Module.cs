/*
 * ROS support library: Module class
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
using System.Collections.Generic;
using Castor;

namespace Spica.ROS
{

	/**
	 * Helper class providing information relevant for processing the
	 * ROS package structure. This is a generic class that may be
	 * instanciated for @p Stack and @p Package types.
	 */
	internal class ModuleInfo<T> where T: Module, new()
	{
		protected T            instance;
		protected string       path_env;
		protected List<string> paths;
		protected List<T>      list;

		public T             Instance	{ get { return this.instance; } }
		public string        PathEnv	{ get { return this.path_env; } }
		public List<T>       List		{ get { return this.list; } }
		public IList<string> Paths		{ get { return this.paths; } }
		public string        CacheFile	{ get { return this.instance.CacheFile; } }
		public string        ID			{ get { return typeof(T).Name.ToUpper(); } }

		/**
		 * Default constructor. It initialises the instance by gathering the
		 * relevant paths for the manifest files and creating the list for
		 * maintaining the module instances
		 */
		public ModuleInfo()
		{
			this.instance = new T();
			this.path_env = Environment.GetEnvironmentVariable(String.Format("ROS_{0}_PATH", ID));
			this.path_env = (this.path_env == null ? "" : this.path_env);
			this.list = new List<T>();

			this.paths = new List<string>(PathEnv.Split(new char[] { ':' },
				 										StringSplitOptions.RemoveEmptyEntries));
		}

		/**
		 * Resets the instance
		 */
		public void Clear()
		{
			this.list.Clear();
		}
	}

	/**
	 * Base class of ROS stacks and packages. Reads out and initialises the
	 * relevant fields for the module (stack/package)
	 */
	public abstract class Module
	{
		protected string dir = null;
		protected string path = null;
		protected string name = null;

		protected string description = null;
		protected string description_brief = null;
		protected string license = null;
		protected string license_url = null;
		protected string author = null;

		protected List<string> dep_names = null;
		protected List<Module> deps = null;

		public string Description		{ get { return this.description; } }
		public string DescriptionBrief	{ get { return this.description; } }
		public string License			{ get { return this.description; } }
		public string LicenseUrl		{ get { return this.description; } }
		public string Author			{ get { return this.description; } }

		public IList<Module> Deps		{ get { return this.deps; } }

		/**
		 * Default constructor. Does nothing except constructing the module.
		 */
		public Module()
		{
			this.dep_names = new List<string>();
			this.deps = new List<Module>();
		}

		/**
		 * Constructor that initialises the module. It creates the modules and
		 * implicitly calls the @p Init method.
		 *
		 * @param dir Module directory
		 */
		public Module(string dir) : this()
		{
			Init(dir);
		}

		/**
		 * Initialises the module given the provided directory. This method
		 * tries to load the manifest file in this directory, reads it and
		 * extracts the relevant elements.
		 *
		 * @param dir Directory of the module
		 * @return An instance of the current module
		 */
		public Module Init(string dir)
		{
			this.dir = dir;
			this.path = System.IO.Path.Combine(this.dir, ManifestFile);
			this.name = System.IO.Path.GetFileName(this.dir);

			// Create new XML reader for the given manifest
			XmlTextReader reader = new XmlTextReader(this.path);

			// Process the manifest
			while (reader.Read())
			{
				reader.MoveToElement();

				// Only process XML elements and ignore the rest
				if (reader.NodeType != XmlNodeType.Element) continue;

				string name = reader.Name.ToLower();


				// Extract description and the (optional) brief description
				if (name.Equals("description"))
				{
					// Get brief description
					if (reader.MoveToAttribute("brief"))
					{
						this.description_brief = reader.Value;
					}

					// Get description
					this.description = reader.ReadString();
				}


				// Extract author (only a single author is supported)
				if (name.Equals("author"))
				{
					this.author = reader.ReadString();
				}

				// Extract the license and its (optional) URL
				if (name.Equals("license"))
				{
					// Get license URL
					if (reader.MoveToAttribute("url"))
					{
						this.license_url = reader.Value;
					}

					// Get licence
					this.license = reader.ReadString();
				}

				// Trigger curstom processing
				ProcessingHook(reader);
			}

			InitHook();

			return this;
		}

		/**
		 * Resolved the dependencies of the corrent module.
		 *
		 * @param modules A list of available modules
		 * @return Returns true if all dependencies were resolved, false otherwise.
		 */
		public bool Resolve<T>(IList<T> modules) where T: Module
		{
			foreach (string s in this.dep_names)
			{
				bool found = false;

				foreach (T m in modules)
				{
					if (m.Name.Equals(s))
					{
						if (!this.deps.Contains(m))
						{
							this.deps.Add(m);
						}
						found = true;
					}
				}

				if (!found)
				{
					throw new CException("Unable to resolve {0} in {1}/{2}", s, GetType().Name, Name);
				}
			}

			this.dep_names.Clear();

			return true;
		}

		/**
		 * Returns the directory of the module
		 */
		public string Directory { get { return this.dir; } }

		/**
		 * Returns the path to the module manifest
		 */
		public string ManifestPath { get { return this.path; } }

		/**
		 * Returns the name of the module
		 */
		public string Name { get { return this.name; } }

		/**
		 * Abstract: Manifest property that returns the name of the manifest
		 * file (no path)
		 */
		public abstract string ManifestFile { get; }

		/**
		 * Abstract: Cache propoerty that returns the name of the cache file
		 * (no path)
		 */
		public abstract string CacheFile { get; }

		/**
		 * Processing hook called in the processing loop of the @p Init method
		 * @param reader An instance of the XML reader class
		 */
		protected abstract void ProcessingHook(XmlTextReader reader);

		/**
		 * Init hook method
		 */
		protected virtual void InitHook()
		{
		}
	}
}
