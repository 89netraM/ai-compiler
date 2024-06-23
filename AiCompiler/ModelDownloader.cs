using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace AiCompiler;

public static class ModelDownloader
{
    private static readonly string BaseUrl =
        "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-onnx/resolve/main/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4";
    private static readonly ProductInfoHeaderValue UserAgent =
        new(
            Assembly.GetExecutingAssembly().GetName().Name,
            Assembly.GetExecutingAssembly().GetName().Version.ToString()
        );

    private static readonly string[] Files =
    [
        "added_tokens.json",
        "config.json",
        "configuration_phi3.py",
        "genai_config.json",
        "phi3-mini-4k-instruct-cpu-int4-rtn-block-32-acc-level-4.onnx",
        "phi3-mini-4k-instruct-cpu-int4-rtn-block-32-acc-level-4.onnx.data",
        "special_tokens_map.json",
        "tokenizer.json",
        "tokenizer.model",
        "tokenizer_config.json",
    ];

    private static readonly string ModelDirectoryPath = Path.Join(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
        "Model"
    );

    public static Model GetModel(CancellationToken cancellationToken)
    {
        var modelDirectory = new DirectoryInfo(ModelDirectoryPath);
        if (!modelDirectory.Exists)
        {
            modelDirectory.Create();

            Task.WhenAll(Files.Select(DownloadFile)).Wait(cancellationToken);
        }
        return new Model(modelDirectory.FullName);
    }

    private static async Task DownloadFile(string fileName)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(UserAgent);
            using var remoteStream = await client.GetStreamAsync($"{BaseUrl}/{fileName}");
            using var localStream = new FileInfo(Path.Join(ModelDirectoryPath, fileName)).OpenWrite();
            await remoteStream.CopyToAsync(localStream);
        }
        catch (Exception ex)
        {
            throw new Exception($"Download filed: {fileName}", ex);
        }
    }
}
