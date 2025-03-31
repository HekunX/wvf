

import { DotNet } from "@microsoft/dotnet-js-interop";

let dispatcher: DotNet.ICallDispatcher;
dispatcher = DotNet.attachDispatcher({
  sendByteArray: sendByteArray,
  beginInvokeDotNetFromJS: beginInvokeDotNetFromJS,
  endInvokeJSFromDotNet: endInvokeJSFromDotNet,
});


function sendByteArray(id: number, data: Uint8Array): void {
    const dataBase64Encoded = base64EncodeByteArray(data);
    window.chrome.webview.postMessage([
      "ReceiveByteArrayFromJS",
      id,
      dataBase64Encoded,
    ]);
  }
  
function beginInvokeDotNetFromJS(
  callId: number,
  assemblyName: string | null,
  methodIdentifier: string,
  dotNetObjectId: number | null,
  argsJson: string
): void {
  console.log("beginInvokeDotNetFromJS");
  window.chrome.webview.postMessage([
    "beginInvokeDotNetFromJS",
    callId ? callId.toString() : null,
    assemblyName,
    methodIdentifier,
    dotNetObjectId || 0,
    argsJson,
  ]);
}

function endInvokeJSFromDotNet(
  callId: number,
  succeeded: boolean,
  resultOrError: any
): void {
  console.log("beginInvokeDotNetFromJS");
  window.chrome.webview.postMessage([
    "endInvokeJSFromDotNet",
    callId ? callId.toString() : null,
    succeeded,
    resultOrError,
  ]);
}


function base64EncodeByteArray(data: Uint8Array) {
  // Base64 encode a (large) byte array
  // Note `btoa(String.fromCharCode.apply(null, data as unknown as number[]));`
  // isn't sufficient as the `apply` over a large array overflows the stack.
  const charBytes = new Array(data.length);
  for (var i = 0; i < data.length; i++) {
    charBytes[i] = String.fromCharCode(data[i]);
  }
  const dataBase64Encoded = btoa(charBytes.join(""));
  return dataBase64Encoded;
}
// https://stackoverflow.com/a/21797381
// TODO: If the data is large, consider switching over to the native decoder as in https://stackoverflow.com/a/54123275
// But don't force it to be async all the time. Yielding execution leads to perceptible lag.
function base64ToArrayBuffer(base64: string): Uint8Array {
    const binaryString = atob(base64);
    const length = binaryString.length;
    const result = new Uint8Array(length);
    for (let i = 0; i < length; i++) {
      result[i] = binaryString.charCodeAt(i);
    }
    return result;
  }
window.chrome.webview.addEventListener("message", (e: any) => {
    var ob = JSON.parse(e.data);
  
    switch (ob[0]) {
      case "EndInvokeDotNet": {
        dispatcher.endInvokeDotNetFromJS(ob[1], ob[2], ob[3]);
        break;
      }
      case "BeginInvokeJS": {
        dispatcher.beginInvokeJSFromDotNet(ob[1], ob[2], ob[3], ob[4], ob[5]);
        break;
      }
      case "SendByteArrayToJS": {
        let id = ob[1];
        let base64Data = ob[2];
        const data = base64ToArrayBuffer(base64Data);
        dispatcher.receiveByteArray(id,data);
        break;
      }
      default: {
        console.error(`不支持的消息类型${e.data}`);
      }
    }
  });

(window as any)["DotNet"] = DotNet;
export { DotNet };
