using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace Replica.Windows;

public sealed class ModuleConfigWindow : Window, IDisposable
{
	private Action? _body;

	private string _title = "Module Configuration";

	public ModuleConfigWindow()
		: base("Module Configuration###ReplicaModuleConfig")
	{
		base.SizeConstraints = new WindowSizeConstraints
		{
			MinimumSize = new Vector2(460f, 340f),
			MaximumSize = new Vector2(1600f, 1400f)
		};
	}

	public void Dispose()
	{
	}

	public override void PreDraw()
	{
		Ui.PushTheme();
	}

	public override void PostDraw()
	{
		Ui.PopTheme();
	}

	public void Open(string title, Action body)
	{
		_title = title;
		_body = body;
		base.WindowName = title + "###ReplicaModuleConfig";
		base.IsOpen = true;
		BringToFront();
	}

	public override void Draw()
	{
		ImGui.SetWindowFontScale(1.15f);
		ImGui.TextColored(in Ui.Accent, _title);
		ImGui.SetWindowFontScale(1f);
		ImGui.Separator();
		ImGui.Spacing();
		if (_body == null)
		{
			ImGui.TextDisabled("No module selected.");
		}
		else
		{
			_body();
		}
	}
}
