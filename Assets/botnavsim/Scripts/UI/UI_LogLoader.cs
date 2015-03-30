﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class UI_LogLoader : IToolbar {

	public UI_LogLoader() {
		if (!Directory.Exists(Strings.logFileDirectory))
			Directory.CreateDirectory(Strings.logFileDirectory);
		// initialise file browsing
		_files = FileBrowser.ListFiles(Strings.logFileDirectory);
		_folders = FileBrowser.ListFolders(Strings.logFileDirectory);
		_subPath = "";
		// initialise window stack
		_windows = new Stack<GUI.WindowFunction>();
		_windows.Push(Legend);
		hidden = true;
	}
	
	public bool contextual {
		get {
			return BotNavSim.isIdle || 
				BotNavSim.isViewingData;
		}
	}

	public bool hidden {
		get; set;
	}

	public string windowTitle {
		get {
			return "Data Playback";
		}
	}

	public Rect windowRect {
		get; set;
	}

	public GUI.WindowFunction windowFunction {
		get {
			return _windows.Peek();
		}
	}
	
	public string currentDir {
		get {
			return Strings.logFileDirectory + _subPath;
		}
	}
	
	private Stack<GUI.WindowFunction> _windows;
	private List<string> _files;
	private List<string> _folders;
	private string _subPath;
	
	/// <summary>
	/// Refresh this the files and folders in current directory
	/// </summary>
	private void Refresh() {
		Debug.Log(currentDir);
		_files = FileBrowser.ListFiles(currentDir, "*.csv");
		_folders = FileBrowser.ListFolders(currentDir);
	}
	
	/// <summary>
	/// Determines whether the mouse was hovering over the last drawn rect object
	/// </summary>
	/// <returns><c>true</c> if mouse over; otherwise, <c>false</c>.</returns>
	private bool IsMouseOver() {
		return Event.current.type == EventType.Repaint &&
			GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
	}
	
	/// <summary>
	/// Top window function - browse CSV files. 
	/// </summary>
	/// <param name="windowID">Window ID.</param>
	private void CsvBrowser(int windowID) {
		// hide button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_windows.Pop();
		}
		// refresh files and folders
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			Refresh();
		}
		GUILayout.EndHorizontal();
		
		// go up one directory
		if (_subPath != "") {
			if (GUILayout.Button("..")) {
				_subPath = Directory.GetParent(_subPath).Name;
				Refresh();
			}
		}
		// list subdirectories
		for (int i = 0; i < _folders.Count; i++) {
			// enter subdirectory
			if (GUILayout.Button(_folders[i])) {
				_subPath += "\\" + new DirectoryInfo(_folders[i]).Name;
				Refresh();
			}
		}
		// list files
		for (int i = 0; i < _files.Count; i++) {
			// try paths from file
			if (GUILayout.Button(_files[i])) {
				// change state when loading first data
				if (BotNavSim.isIdle) {
					BotNavSim.state = BotNavSim.State.ViewingData;
				}
				LogLoader.LoadPaths(currentDir + "\\" + _files[i]);
			}
		}
		
		//GUI.DragWindow();
	}
	
	private void Legend(int windowID) {
		// back button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			hidden = true;
		}
		if (GUILayout.Button("Load from CSV...")) {
			_windows.Push(CsvBrowser);
		}
		GUILayout.EndHorizontal();
		
		// Hacky, temporary camera placement
		Camera.main.orthographicSize = 50f;
		Camera.main.transform.position = Simulation.bounds.center + Vector3.up * 100f;
		
		if (LogLoader.paths.Count < 1) {
			GUILayout.Label("No paths loaded.");
		}
		
		// list botpaths
		for(int i = 0; i < LogLoader.paths.Count; i++) {
			if (GUILayout.Button(LogLoader.paths[i].csvName)) {
				LogLoader.paths[i].visible = !LogLoader.paths[i].visible;
				continue;
			}
			if (IsMouseOver()) {
				LogLoader.paths[i].highlight = true;
			} else {
				LogLoader.paths[i].highlight = false;
			}
			if (LogLoader.paths[i].visible) {
				LogLoader.paths[i].DrawPath();
			}
		}
		
		//GUI.DragWindow();
	}
	
	
}
