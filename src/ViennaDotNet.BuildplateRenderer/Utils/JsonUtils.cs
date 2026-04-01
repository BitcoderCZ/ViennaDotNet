// <copyright file="JsonUtils.cs" company="BitcoderCZ">
// Copyright (c) BitcoderCZ. All rights reserved.
// </copyright>

using System.Text.Json;
using ViennaDotNet.BuildplateRenderer.JsonConverters;

namespace ViennaDotNet.BuildplateRenderer.Utils;

internal static class JsonUtils
{
	private static readonly JsonSerializerOptions DefaultJsonOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

	static JsonUtils()
	{
		DefaultJsonOptions.Converters.Add(new JsonConverter_float2());
		DefaultJsonOptions.Converters.Add(new JsonConverter_int3());
		DefaultJsonOptions.Converters.Add(new JsonConverter_float3());
		DefaultJsonOptions.Converters.Add(new JsonConverter_double3());
	}

	public static T? DeserializeJson<T>(ReadOnlySpan<char> json)
		=> JsonSerializer.Deserialize<T>(json, DefaultJsonOptions);

	public static T? DeserializeJson<T>(ReadOnlySpan<byte> utf8Json)
		=> JsonSerializer.Deserialize<T>(utf8Json, DefaultJsonOptions);

	public static string SerializeJson<T>(T value)
		=> JsonSerializer.Serialize(value, DefaultJsonOptions);
}
