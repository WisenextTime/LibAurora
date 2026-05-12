using System;
using CSCore;
using LibAurora.Audio;
namespace LibAurora.Resources;

public static partial class AssetServer
{
	internal static void InitWaveSourceProcessor() =>
		RegisterProcesser(new ResourceProcesser<IWaveSource>(
			load: AudioUtils.Decode,
			save: (_, _) => throw new NotSupportedException()
		));
}