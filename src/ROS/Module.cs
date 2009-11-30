using System;
using System.Xml;
using System.Collections.Generic;

namespace Spica.ROS
{

	public class ModuleInfo<T> where T: Module, new()
	{
		protected T            instance;
		protected string       path_env;
		protected List<string> paths;
		protected List<T>      list;

		public T             Instance { get { return this.instance; } }
		public string        PathEnv  { get { return this.path_env; } }
		public List<T>       List     { get { return this.list; } }
		public IList<string> Paths    { get { return this.paths; } }
		public string        Cache    { get { return this.instance.Cache; } }
		public string        ID       { get { return typeof(T).Name.ToUpper(); } }

		public ModuleInfo()
		{
			this.instance = new T();
			this.path_env = Environment.GetEnvironmentVariable(String.Format("ROS_{0}_PATH", ID));
			this.path_env = (this.path_env == null ? "" : this.path_env);
			this.list = new List<T>();

			this.paths = new List<string>(PathEnv.Split(new char[] { ':' },
				 										StringSplitOptions.RemoveEmptyEntries));
		}

		public void Clear()
		{
			this.list.Clear();
		}
	}

	public abstract class Module
	{
		protected string dir = null;
		protected string path = null;
		protected string name = null;

		public Module()
		{
		}

		public Module(string dir)
		{
			Init(dir);
		}

		public Module Init(string dir)
		{
			this.dir = dir;
			this.path = System.IO.Path.Combine(this.dir, Manifest);

			this.name = System.IO.Path.GetFileName(this.dir);

			Console.WriteLine("Creating package {0}", Name);

			XmlTextReader reader = new XmlTextReader(this.path);

			while (reader.Read())
			{
				reader.MoveToElement();
				ProcessNode(reader);
			}

			return this;
		}

		public string Directory { get { return this.dir; } }
		public string Path { get { return this.path; } }
		public string Name { get { return this.name; } }

		public abstract string Manifest { get; }
		public abstract string Cache { get; }

		protected abstract void ProcessNode(XmlTextReader reader);
	}
}
