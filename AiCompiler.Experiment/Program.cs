using System;
using Microsoft.ML.OnnxRuntimeGenAI;

using var model = new Model(@"..\phi3-mini\cpu_and_mobile\cpu-int4-rtn-block-32-acc-level-4");
using var tokenizer = new Tokenizer(model);

using var tokens = tokenizer.Encode(
    """
    <|system|>
    You are an expert C# developer. You are tasked with implementing a method
    given the documentation and signature.
    When using libraries, you can not assume they are imported. Always fully
    qualify types, with global:: and namespaces. Even for the standard library.
    You only ever respond with the body of the method. No additional information
    or comments are needed. The code should be correct and complete. No
    formatting. No imports. No boilerplate code. No additional methods. No
    method declaration. Only the body of the method.
    <|end|>
    <|user|>
    /// <summary>
    /// This method should add two numbers together
    /// </summary>
    /// <param name=""a"">The first number to add</param>
    /// <param name=""b"">The second number to add</param>
    /// <returns>The sum of the two numbers</returns>
    public int Add(int a, int b)
    <|end|>
    <|assistant|>
    """
);

using var generatorParams = new GeneratorParams(model);
generatorParams.SetInputSequences(tokens);

using var generator = new Generator(model, generatorParams);
while (!generator.IsDone())
{
    generator.ComputeLogits();
    generator.GenerateNextToken();
    var outputTokens = generator.GetSequence(0);
    var newToken = outputTokens.Slice(outputTokens.Length - 1, 1);
    var output = tokenizer.Decode(newToken);
    Console.Write(output);
}


// The absolute path to the folder where the Phi-3 model is stored (folder to the ".onnx" file)
// using System;
// using Microsoft.ML.OnnxRuntimeGenAI;

// var modelPath = @"C:\Users\marten.asberg\Downloads\phi3-cpu\";
// var model = new Model(modelPath);
// var tokenizer = new Tokenizer(model);

// // System prompt will be used to instruct the AI how to response to the user prompt
// var systemPrompt =
//     "You are a knowledgeable and friendly assistant made by Build5Nines named Jarvis. Answer the following question as clearly and concisely as possible, providing any relevant information and examples.";

// // Create a loop for taking input from the user
// while (true)
// {
//     // Get user prompt
//     Console.Write("Type Prompt then Press [Enter] or CTRL-C to Exit: ");
//     var userPrompt = Console.ReadLine();

//     // show in console that the assistant is responding
//     Console.WriteLine("");
//     Console.Write("Assistant: ");

//     // Build the Prompt
//     // Single User Prompt with System Prompt
//     var fullPrompt = $"<|system|>{systemPrompt}<|end|><|user|>{userPrompt}<|end|><|assistant|>";

//     // Tokenize the prompt
//     var tokens = tokenizer.Encode(fullPrompt);

//     // Set generator params
//     var generatorParams = new GeneratorParams(model);
//     generatorParams.SetSearchOption("max_length", 2048);
//     generatorParams.SetSearchOption("past_present_share_buffer", false);
//     generatorParams.SetInputSequences(tokens);

//     // Generate the response
//     var generator = new Generator(model, generatorParams);
//     // Output response as each token in generated
//     while (!generator.IsDone())
//     {
//         generator.ComputeLogits();
//         generator.GenerateNextToken();
//         var outputTokens = generator.GetSequence(0);
//         var newToken = outputTokens.Slice(outputTokens.Length - 1, 1);
//         var output = tokenizer.Decode(newToken);
//         Console.Write(output);
//     }
//     Console.WriteLine();
// }
