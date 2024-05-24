using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HSoft.JSRuntimeBase
{
    public abstract class WebViewJSRuntime : JSRuntime
    {
        internal TaskCompletionSource InitalizedTaskSource = new TaskCompletionSource();
        public WebViewJSRuntime()
        {
        }
        public async Task AttatchObjectToWindowAsync<T>(string objectName, T o) where T : class
        {
            if (!InitalizedTaskSource.Task.IsCompleted)
            {
                await InitalizedTaskSource.Task;
            }
            await this.InvokeVoidAsync("_registerInstance", objectName, DotNetObjectReference.Create(o));
        }


        protected abstract void PostWebMessageAsString(string  message);
        protected void OnWebMessageReceived(string json)
        {
            var objects = JsonSerializer.Deserialize<JsonElement[]>(json, JsonSerializerOptions);
            var messageType = objects[0].GetString();
            switch (messageType)
            {
                case "beginInvokeDotNetFromJS":
                    {
                        string callId = objects[1].GetString();
                        string assemblyName = objects[2].GetString();
                        string methodIdentifier = objects[3].GetString();
                        long dotNetObjectId = objects[4].GetInt64();
                        string argsJson = objects[5].GetString();
                        DotNetInvocationInfo dotNetInvocationInfo = new DotNetInvocationInfo(assemblyName, methodIdentifier, dotNetObjectId, callId);
                        DotNetDispatcher.BeginInvokeDotNet(this, dotNetInvocationInfo, argsJson);
                        break;
                    }
                case "endInvokeJSFromDotNet":
                    {
                        string callId = objects[1].GetString();
                        bool succeeded = objects[2].GetBoolean();
                        string resultOrError = objects[3].GetString();
                        DotNetDispatcher.EndInvokeJS(this, resultOrError);
                        break;
                    }
                case "initialized":
                    {
                        if(!InitalizedTaskSource.Task.IsCompleted)
                        {
                            InitalizedTaskSource.SetResult();
                        }
    
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

        }


        protected override void BeginInvokeJS(long taskId, string identifier, [StringSyntax("Json")] string? argsJson, JSCallResultType resultType, long targetInstanceId)
        {
            var json = JsonSerializer.Serialize(new object[] { "BeginInvokeJS", taskId, identifier, argsJson, resultType, targetInstanceId }, JsonSerializerOptions);
            PostWebMessageAsString(json);
        }

        protected override void EndInvokeDotNet(DotNetInvocationInfo invocationInfo, in DotNetInvocationResult invocationResult)
        {
            var resultJsonOrErrorMessage = invocationResult.Success
    ? invocationResult.ResultJson
    : invocationResult.Exception.ToString();
            var json = JsonSerializer.Serialize(new object[] { "EndInvokeDotNet", invocationInfo.CallId, invocationResult.Success, resultJsonOrErrorMessage }, JsonSerializerOptions);
            PostWebMessageAsString(json);
        }
    }
}
