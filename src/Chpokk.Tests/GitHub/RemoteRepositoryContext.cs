using System;
using System.IO;
using Chpokk.Tests.GitHub.Infrastructure;
using Chpokk.Tests.Infrastructure;
using ChpokkWeb.Features.Repa;
using GitSharp;
using LibGit2Sharp.Tests.TestHelpers;
using MbUnit.Framework;

namespace Chpokk.Tests.GitHub {
	public class RemoteRepositoryContext : SimpleConfiguredContext, IDisposable {
		public string RepositoryPath { get; private set; }
		public string FileName { get; private set; }
		public string FilePath {
			get { return Path.Combine(RepositoryPath, FileName); }
		}
		[SetUp]
		public override void Create() {
			base.Create();

			RepositoryPath  = Path.Combine(Path.GetFullPath(@".."), RepositoryInfo.Path);
			if (Directory.Exists(RepositoryPath))
				DirectoryHelper.DeleteDirectory(RepositoryPath);			
			FileName = Guid.NewGuid().ToString();
			var content = "stuff";
			Api.CommitFile(FileName, content);
		}

		public void Dispose() {
			if (Directory.Exists(RepositoryPath))
				DirectoryHelper.DeleteDirectory(RepositoryPath);			
		}
	}

	public class ClonedRepositoryContext : RemoteRepositoryContext {
		public const string REPO_URL = "git@github.com:uluhonolulu/Chpokk-Scratchpad.git"; // "git://github.com/uluhonolulu/Chpokk-Scratchpad.git";
		public override void Create() {
			base.Create();
			Git.Clone(REPO_URL, RepositoryPath);
		}
	}

	public class ModifiedRepositoryContext : ClonedRepositoryContext {
		public override void Create() {
			base.Create();
			var content = File.ReadAllText(FilePath);
			var newContent = content + Environment.NewLine + DateTime.Now.ToString();
			File.WriteAllText(FilePath, newContent);
		}
	}
}