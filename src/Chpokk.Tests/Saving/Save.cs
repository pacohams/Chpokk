﻿using Arractas;
using Chpokk.Tests.Exploring;
using ChpokkWeb.Features.Editor.SaveCommit;
using FubuCore;
using MbUnit.Framework;
using LibGit2Sharp.Tests.TestHelpers;

namespace Chpokk.Tests.Saving {
	[TestFixture]
	public class Save : BaseCommandTest<PhysicalCodeFileContext> {
		private const string NEW_CONTENT = "---";
		[Test]
		public void ShouldHaveFileContentsChangedToNew() {
			var system = Context.Container.Get<FileSystem>();
			system.FileExists(Context.FilePath).ShouldBeTrue();
			var content = system.ReadStringFromFile(Context.FilePath);
			content.ShouldEqual(NEW_CONTENT);
		}

		public override void Act() {
			var controller = Context.Container.Get<SaveCommitController>();
			controller.Save(new SaveCommitModel() {Content = NEW_CONTENT, FilePath = Context.FilePath});
		}
	}
}
