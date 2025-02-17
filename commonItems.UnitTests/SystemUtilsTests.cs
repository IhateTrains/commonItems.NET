﻿using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace commonItems.UnitTests; 

[Collection("Sequential")]
[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SystemUtilsTests {
	private const string testFilesPath = "TestFiles/SystemUtilsTestFiles";

	[Fact]
	public void GetAllFilesInFolderDoesntWorkRecursively() {
		var files = SystemUtils.GetAllFilesInFolder(testFilesPath);
		var expected = new SortedSet<string>{
			"keyValuePair.txt"
		};
		Assert.Equal(expected, files);
	}
	[Fact]
	public void GetAllFilesInFolderReturnsEmptySetOnNonexistentFolder() {
		var files = SystemUtils.GetAllFilesInFolder(testFilesPath + "/missingDir");
		Assert.Empty(files);
	}
	[Fact]
	public void GetAllFilesInFolderRecursiveWorkRecursively() {
		var files = SystemUtils.GetAllFilesInFolderRecursive(testFilesPath);
		var expected = new SortedSet<string>{
			"keyValuePair.txt",
			Path.Combine("subfolder", "subfolder_file.txt"),
			Path.Combine("subfolder", "subfolder_file2.txt"),
			Path.Combine("subfolder2", "subfolder2_file.txt")
		};
		Assert.Equal(expected, files);
	}
	[Fact]
	public void GetAllFilesInFolderRecursiveReturnsEmptySetOnNonexistentFolder() {
		var files = SystemUtils.GetAllFilesInFolderRecursive(testFilesPath + "/missingDir");
		Assert.Empty(files);
	}
	[Fact]
	public void GetAllSubfoldersGetsSubfolders() {
		var subfolders = SystemUtils.GetAllSubfolders(testFilesPath);
		var expected = new SortedSet<string>{
			"subfolder",
			"subfolder2"
		};
		Assert.Equal(expected, subfolders);
	}
	[Fact]
	public void GetAllSubfoldersReturnsEmptySetOnNonexistentFolder() {
		var subfolders = SystemUtils.GetAllSubfolders(testFilesPath + "/missingDir");
		Assert.Empty(subfolders);
	}
	[Fact]
	public void TryCreateFolderCreatesFolder() {
		var created = SystemUtils.TryCreateFolder(testFilesPath + "/newFolder");
		Assert.True(created);
		Assert.True(Directory.Exists(testFilesPath + "/newFolder"));
		Directory.Delete(testFilesPath + "/newFolder", recursive: true); // cleanup
	}
	[Fact]
	public void TryCreateFolderLogsErrorOnEmptyPath() {
		var output = new StringWriter();
		Console.SetOut(output);

		const string path = "";
		var created = SystemUtils.TryCreateFolder(path);
		Assert.False(created);
		Assert.False(Directory.Exists(path));
		Assert.Contains("[ERROR] Could not create directory: " + path +
		                " : System.ArgumentException: Path cannot be the empty string or all whitespace. (Parameter 'path')",
			output.ToString());
	}

	[Fact]
	public void TryCopyFileCopiesFile() {
		const string sourcePath = testFilesPath + "/subfolder2/subfolder2_file.txt";
		const string destPath = testFilesPath + "/subfolder/subfolder2_file.txt";
		var success = SystemUtils.TryCopyFile(sourcePath, destPath);
		Assert.True(success);
		Assert.True(File.Exists(destPath));
		File.Delete(destPath); // cleanup
	}
	[Fact]
	public void TryCopyFileLogsErrorOnMissingSourceFile() {
		var output = new StringWriter();
		Console.SetOut(output);

		const string sourcePath = testFilesPath + "/subfolder/missingFile.txt";
		const string destPath = testFilesPath + "/newFolder/file.txt";
		var success = SystemUtils.TryCopyFile(sourcePath, destPath);
		Assert.False(success);
		Assert.False(File.Exists(destPath));
		Assert.Contains($"[WARN] Could not copy file {sourcePath} to {destPath} - System.IO.FileNotFoundException: Could not find file",
			output.ToString());
	}

	[Fact]
	public void CopyFolderCopiesFolder() {
		const string sourcePath = testFilesPath + "/subfolder2";
		const string destPath = testFilesPath + "/subfolder3";
		Assert.False(Directory.Exists(destPath));
		var success = SystemUtils.TryCopyFolder(sourcePath, destPath);
		Assert.True(success);
		Assert.True(Directory.Exists(destPath));
		Directory.Delete(destPath, recursive: true); // cleanup
	}
	[Fact]
	public void CopyFolderLogsErrorOnMissingSourceFolder() {
		var output = new StringWriter();
		Console.SetOut(output);

		const string sourcePath = testFilesPath + "/missingFolder";
		const string destPath = testFilesPath + "/newFolder";
		var success = SystemUtils.TryCopyFolder(sourcePath, destPath);
		Assert.False(success);
		Assert.False(Directory.Exists(destPath));
		Assert.Contains("[ERROR] Could not copy folder: " +
		                "System.IO.DirectoryNotFoundException: Source directory does not exist or could not be found",
			output.ToString());
	}

	[Fact]
	public void RenameFolderRenamesFolder() {
		const string path = testFilesPath + "/subfolder2";
		const string newPath = testFilesPath + "/subfolderRenamed";
		Assert.True(Directory.Exists(path));
		Assert.False(Directory.Exists(newPath));
		var success = SystemUtils.TryRenameFolder(path, newPath);
		Assert.True(success);
		Assert.False(Directory.Exists(path));
		Assert.True(Directory.Exists(newPath));
		SystemUtils.TryRenameFolder(newPath, path); // cleanup
	}
	[Fact]
	public void RenameFolderLogsErrorOnMissingSourceFolder() {
		var output = new StringWriter();
		Console.SetOut(output);

		const string sourcePath = testFilesPath + "/missingFolder";
		const string destPath = testFilesPath + "/newFolder";
		var success = SystemUtils.TryRenameFolder(sourcePath, destPath);
		Assert.False(success);
		Assert.False(Directory.Exists(destPath));
		Assert.Contains("[ERROR] Could not rename folder: System.IO.DirectoryNotFoundException: Could not find a part of the path",
			output.ToString());
	}

	[Fact]
	public void DeleteFolderDeletesFolder() {
		const string path = testFilesPath + "/tempFolder";
		SystemUtils.TryCreateFolder(path);
		Assert.True(Directory.Exists(path));
		var success = SystemUtils.TryDeleteFolder(path);
		Assert.True(success);
		Assert.False(Directory.Exists(path));
	}
	[Fact]
	public void DeleteFolderLogsErrorOnMissingSourceFolder() {
		var output = new StringWriter();
		Console.SetOut(output);

		const string path = testFilesPath + "/missingFolder";
		var success = SystemUtils.TryDeleteFolder(path);
		Assert.False(success);
		Assert.Contains($"[ERROR] Could not delete folder: {path} : " +
		                "System.IO.DirectoryNotFoundException: Could not find a part of the path",
			output.ToString());
	}
}