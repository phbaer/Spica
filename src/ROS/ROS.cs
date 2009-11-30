using System;
using System.IO;
using System.Collections.Generic;
using Castor;

namespace Spica.ROS
{

	public class ROS
	{
		protected bool enable_ros = false;
		protected string ros_root = null;

		protected long ros_cache_timeout = 60000;

		protected ModuleInfo<Stack> stacks = null;
//		protected string stack_path_env = null;
//		protected IList<string> stack_paths = null;
//		protected List<Stack> stacks = null;
		public IList<Stack> Stacks
		{
			get { return this.stacks.List; }
		}

		protected ModuleInfo<Package> packages = null;
//		protected string package_path_env = null;
//		protected IList<string> package_paths = null;
//		protected List<Package> packages = null;
		public IList<Package> Packages
		{
			get { return this.packages.List; }
		}

		public ROS(bool enable_ros)
		{
//			this.stack_paths = new List<string>();
//			this.stacks = new List<Stack>();

//			this.package_paths = new List<string>();
//			this.packages = new List<Package>();

			if (enable_ros)
			{
				this.enable_ros = enable_ros;
				this.ros_root = Environment.GetEnvironmentVariable("ROS_ROOT");

				if (this.ros_root == null)
				{
					Debug.WriteLine("ROS not found, support disabled");
					this.enable_ros = false;
				}

				this.stacks = new ModuleInfo<Stack>();
				this.packages = new ModuleInfo<Package>();
			}

			if (this.enable_ros)
			{
				// Get ROS cache timeout
				try
				{
					this.ros_cache_timeout = (long)(Double.Parse(Environment.GetEnvironmentVariable("ROS_CACHE_TIMEOUT")) * 60 * 1000);
				}
				catch (Exception)
				{
					this.ros_cache_timeout = 60000;
				}

				// Output verbose info for now
				Debug.WriteLine("ROS support enabled");
				Debug.WriteLine("+ Root: {0}", this.ros_root);
				Debug.WriteLine("+ Cache timeout: {0} s", this.ros_cache_timeout / 1000);
			}
		}

		public void Scan()
		{
			if (!this.enable_ros) return;

			string file = null;

			Debug.WriteLine("Scanning ROS directories...");

			foreach (string s in this.stacks.Paths)
			{
				Debug.WriteLine("+ Stack: {0}", s);
			}

			foreach (string s in this.packages.Paths)
			{
				Debug.WriteLine("+ Package: {0}", s);
			}

			DoScan(this.stacks);
			DoScan(this.packages);
		}

		protected void DoScan<T>(ModuleInfo<T> mi) where T: Module, new()
		{
			mi.Clear();

			// Check if there is a cache file
			string cache_file = Path.Combine(this.ros_root, mi.Cache);
			DateTime cache_file_time = File.GetLastWriteTimeUtc(cache_file);

			// Check if the cache is still valid
			long file_age = (DateTime.UtcNow - cache_file_time).Ticks / 10000;
			if (file_age < this.ros_cache_timeout)
			{
				Debug.WriteLine("... using cache file {0} (age {1} s)", mi.Cache, file_age / 1000);

				StreamReader reader = new StreamReader(File.OpenRead(cache_file));

				string line = null;
				while ((line = reader.ReadLine()) != null)
				{
					// the environment variables are ignored for now
					if (line.StartsWith("#")) continue;

					T instance = new T();
					instance.Init(line);

					mi.List.Add(instance);
				}
			}
			else
			{
				// Add all stacks or packages
				foreach (string p in mi.Paths)
				{
					mi.List.AddRange(GetAllInPath<T>(p));
				}

				// Write cache file
				StreamWriter cache = new StreamWriter(File.Create(cache_file));
				cache.WriteLine("#ROS_ROOT={0}", this.ros_root);
				cache.WriteLine("#ROS_STACK_PATH={0}", mi.ID);
			
				foreach (Module m in mi.List)
				{
					cache.WriteLine(m.Directory);
				}

				cache.Flush();
				}
		}

		public void AddPackagePath(string path)
		{
			this.packages.Paths.Add(path);
		}

		protected IList<string> GetEnvPaths(string paths)
		{
			if (paths == null) return new string[0];

			// TODO: UNIX-specific
			return paths.Split(new char[] { ':' },
							   StringSplitOptions.RemoveEmptyEntries);
		}

		/**
		 * Returns a list of stacks or packages. This method recursively scans 
		 * the given directory (@p start) and tries to find all ROS stacks and packages,
		 * depending on what type is searched for (@p T).
		 * @start The directory to start at
		 * @return A list of stacks or packages that are contained in the given directory
		 */
		protected IList<T> GetAllInPath<T>(string start) where T: Module, new()
		{
			T instance = new T();

			// Check the start directory
			string[] files = Directory.GetFiles(start, instance.Manifest);
			if (files.Length > 0)
			{
				return new T[] { instance.Init(start) as T };
			}

			List<T> result = new List<T>();
			// Check all subdirectories only if there is no description file found
			// (packages and stacks cannot be nested)
			foreach (string d in Directory.GetDirectories(start))
			{
				result.AddRange(GetAllInPath<T>(d));
			}

			return result;
		}
	}
}

