using System;
using System.Collections.Generic;
using Veldrid;
using MyraKey = global::Myra.Platform.Keys;

namespace LibAurora.Input;

/// <summary>
/// Bidirectional mapping between Veldrid <see cref="Key"/> and Myra <see cref="MyraKey"/>.
/// Covers all alphanumeric keys, F1-F12, navigation, modifiers, and common system keys.
/// Used by <see cref="FillKeysDown"/> to populate Myra's bool[] key-state array from
/// <see cref="IInput.IsKeyDown"/>.
/// </summary>
public static class MyraKeyMapUtils
{
	private readonly static Dictionary<Key, MyraKey> _veldridToMyra;
	private readonly static Dictionary<MyraKey, Key> _myraToVeldrid;

	static MyraKeyMapUtils()
	{
		_veldridToMyra = new Dictionary<Key, MyraKey>
		{
			[Key.A] = MyraKey.A, [Key.B] = MyraKey.B, [Key.C] = MyraKey.C,
			[Key.D] = MyraKey.D, [Key.E] = MyraKey.E, [Key.F] = MyraKey.F,
			[Key.G] = MyraKey.G, [Key.H] = MyraKey.H, [Key.I] = MyraKey.I,
			[Key.J] = MyraKey.J, [Key.K] = MyraKey.K, [Key.L] = MyraKey.L,
			[Key.M] = MyraKey.M, [Key.N] = MyraKey.N, [Key.O] = MyraKey.O,
			[Key.P] = MyraKey.P, [Key.Q] = MyraKey.Q, [Key.R] = MyraKey.R,
			[Key.S] = MyraKey.S, [Key.T] = MyraKey.T, [Key.U] = MyraKey.U,
			[Key.V] = MyraKey.V, [Key.W] = MyraKey.W, [Key.X] = MyraKey.X,
			[Key.Y] = MyraKey.Y, [Key.Z] = MyraKey.Z,

			[Key.Number0] = MyraKey.D0, [Key.Number1] = MyraKey.D1,
			[Key.Number2] = MyraKey.D2, [Key.Number3] = MyraKey.D3,
			[Key.Number4] = MyraKey.D4, [Key.Number5] = MyraKey.D5,
			[Key.Number6] = MyraKey.D6, [Key.Number7] = MyraKey.D7,
			[Key.Number8] = MyraKey.D8, [Key.Number9] = MyraKey.D9,

			[Key.F1] = MyraKey.F1, [Key.F2] = MyraKey.F2, [Key.F3] = MyraKey.F3,
			[Key.F4] = MyraKey.F4, [Key.F5] = MyraKey.F5, [Key.F6] = MyraKey.F6,
			[Key.F7] = MyraKey.F7, [Key.F8] = MyraKey.F8, [Key.F9] = MyraKey.F9,
			[Key.F10] = MyraKey.F10, [Key.F11] = MyraKey.F11, [Key.F12] = MyraKey.F12,

			[Key.Enter] = MyraKey.Enter, [Key.Escape] = MyraKey.Escape,
			[Key.Space] = MyraKey.Space, [Key.Tab] = MyraKey.Tab,
			[Key.Delete] = MyraKey.Delete,
			[Key.Home] = MyraKey.Home, [Key.End] = MyraKey.End,
			[Key.PageUp] = MyraKey.PageUp, [Key.PageDown] = MyraKey.PageDown,

			[Key.Left] = MyraKey.Left, [Key.Right] = MyraKey.Right,
			[Key.Up] = MyraKey.Up, [Key.Down] = MyraKey.Down,

			[Key.ShiftLeft] = MyraKey.LeftShift, [Key.ShiftRight] = MyraKey.RightShift,
			[Key.ControlLeft] = MyraKey.LeftControl, [Key.ControlRight] = MyraKey.RightControl,
			[Key.AltLeft] = MyraKey.LeftAlt, [Key.AltRight] = MyraKey.RightAlt,

			[Key.CapsLock] = MyraKey.CapsLock, [Key.NumLock] = MyraKey.NumLock,
			[Key.PrintScreen] = MyraKey.PrintScreen, [Key.Pause] = MyraKey.Pause,
			[Key.Insert] = MyraKey.Insert,
			[Key.BackSpace] = MyraKey.Back,
		};

		_myraToVeldrid = new Dictionary<MyraKey, Key>();
		foreach (var (vk, mk) in _veldridToMyra)
			_myraToVeldrid[mk] = vk;
	}

	/// <summary>Populates the Myra key-state array from <paramref name="input"/> using the bidirectional key map.</summary>
	public static void FillKeysDown(bool[] keys, IInput input)
	{
		Array.Fill(keys, false);

		foreach (var (veldridKey, myraKey) in _veldridToMyra)
		{
			if (!input.IsKeyDown(veldridKey)) continue;
			var idx = (int)myraKey;
			if (idx >= 0 && idx < keys.Length)
				keys[idx] = true;
		}
	}

	/// <summary>Converts a Veldrid <see cref="Key"/> to a Myra <see cref="MyraKey"/>.</summary>
	public static bool TryGetMyraKey(Key veldridKey, out MyraKey myraKey)
		=> _veldridToMyra.TryGetValue(veldridKey, out myraKey);

	/// <summary>Converts a Myra <see cref="MyraKey"/> to a Veldrid <see cref="Key"/>.</summary>
	public static bool TryGetVeldridKey(MyraKey myraKey, out Key veldridKey)
		=> _myraToVeldrid.TryGetValue(myraKey, out veldridKey);
}